using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Service
{
    public static class Resolution
    {
        private static readonly object Lock = new object();

        private static readonly ConcurrentDictionary<string, Assembly> AssemblyLoadMap = new ConcurrentDictionary<string, Assembly>();
        private static readonly ConcurrentDictionary<string, string> AssemblyPathMap = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<Type, List<Type>> TypeResolutionMap = new ConcurrentDictionary<Type, List<Type>>();

        public static readonly Dictionary<Type, List<Type>> GetGenericsByBaseClassCache = new Dictionary<Type, List<Type>>();

        private static readonly object GetGenericsByBaseClassLock = new object();

        private static readonly List<string> IgnoreList = new List<string> { "System.", "Microsoft.", "mscorlib", "netstandard" };

        static Resolution()
        {
            //Modules.Log.System.Add("Warm-up START", Message.EContentType.StartupSequence);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var self = Assembly.GetEntryAssembly();

            if (self != null) RegisterAssembly(self);

            // 1st cycle: Local (base directory) assemblies

            LoadAssembliesFromDirectory(Host.BaseDirectory);

            //2nd cycle: Directories/assemblies referenced by system

            //    First by process-specific variables...
            LoadAssembliesFromDirectory(System.Environment.GetEnvironmentVariable(Strings.zen_ver, EnvironmentVariableTarget.Process));
            //    then by user-specific variables...
            LoadAssembliesFromDirectory(System.Environment.GetEnvironmentVariable(Strings.zen_ver, EnvironmentVariableTarget.User));
            //    and finally system-wide variables.
            LoadAssembliesFromDirectory(System.Environment.GetEnvironmentVariable(Strings.zen_ver, EnvironmentVariableTarget.Machine));

            //Now try to load:

            var lastErrCount = -1;
            var errCount = 0;

            while (errCount != lastErrCount)
            {
                lastErrCount = errCount;

                //var modList = AssemblyCache.Select(i => i.Value.ToString().Split(',')[0]).ToJson();

                foreach (var item in AssemblyLoadMap)
                    try { item.Value.GetTypes(); } catch (Exception e) { Base.Log.Add("Error while loading " + item.Key, e); }
            }
            Base.Log.KeyValuePair("Assembly Loader", $"{AssemblyLoadMap.Count} assemblies registered", Message.EContentType.StartupSequence);
        }

        public static List<T> GetInstances<T>(bool excludeCoreNullDefinitions = true) where T : class { return GetClassesByInterface<T>(excludeCoreNullDefinitions).Select(i => i.CreateInstance<T>()).ToList(); }

        private static Assembly GetAssemblyByName(string name) { return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name); }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var shortName = args.Name.Split(',')[0];
            var probe = GetAssemblyByName(shortName);
            return probe;
        }

        private static void LoadAssembliesFromDirectory(string path)
        {
            if (path == null) return;

            if (path.IndexOf(";", StringComparison.Ordinal) > -1) //Semicolon-split list. Parse and process one by one.
            {
                var list = path.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var item in list) LoadAssembliesFromDirectory(item);
            }
            else
            {
                var attr = File.GetAttributes(path);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    //It's a directory: Load all assemblies and set up monitor.

                    var assylist = Directory.GetFiles(path, "*.dll");

                    foreach (var dll in assylist) LoadAssembly(dll);
                    //Modules.Log.System.Add($"    Watching {path}", Message.EContentType.Info);
                }
                else
                {
                    //It's a file: Load it directly.
                    lock (Lock) { LoadAssembly(path); }
                }
            }
        }

        public static Assembly RegisterAssembly(Assembly assy)
        {
            var assyName = assy.GetName().Name;

            if (AssemblyLoadMap.ContainsKey(assyName)) return assy;

            AssemblyLoadMap[assyName] = assy;

            var refAssemblies = assy.GetReferencedAssemblies().Where(i => !IgnoreList.Any(j => i.Name.StartsWith(j))).ToList();

            if (!refAssemblies.Any()) return assy;

            foreach (var assemblyName in refAssemblies)
            {
                var refAssy = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);

                RegisterAssembly(refAssy);
            }

            return assy;
        }

        private static void LoadAssembly(string physicalPath)
        {
            if (physicalPath == null) return;

            try
            {
                var p = Path.GetFileName(physicalPath);

                if (AssemblyPathMap.ContainsKey(p)) return;

                AssemblyPathMap.TryAdd(p, physicalPath);

                var assy = Assembly.LoadFrom(physicalPath);

                RegisterAssembly(assy);
            }
            catch (Exception e)
            {
                if (e is ReflectionTypeLoadException exception)
                {
                    var typeLoadException = exception;
                    var loaderExceptions = typeLoadException.LoaderExceptions.ToList();

                    //if (loaderExceptions.Count > 0) Modules.Log.System.Add("    Fail " + path + ": " + loaderExceptions[0].Message);
                    //else Modules.Log.System.Add("    Fail " + path + ": Undefined.");
                }
            }
        }

        public static List<Type> GetClassesByBaseClass(Type refType, bool limitToMainAssembly = false)
        {
            try
            {
                var classCol = new List<Type>();

                var assySource = new List<Assembly>();

                if (limitToMainAssembly) assySource.Add(Host.ApplicationAssembly);
                else
                    lock (Lock) { assySource = AssemblyLoadMap.Values.ToList(); }

                foreach (var asy in assySource)
                    classCol.AddRange(asy
                                          .GetTypes()
                                          .Where(type => type.BaseType != null)
                                          .Where(
                                              type =>
                                                  type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == refType
                                                  || type.BaseType == refType));

                return classCol;
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var item in ex.LoaderExceptions)
                {
                    //Current.Log.Add(currAssembly?.GetName().Name + " @ " + currAssembly?.Location + ": " + item.Message, Message.EContentType.Warning);
                }

                throw ex;
            }
            catch (Exception ex)
            {
                //Current.Log.Add($"GetClassesByBaseClass ERR for {refType.Name}: {ex.Message}", Message.EContentType.Warning);
                throw ex;
            }
        }

        public static List<Type> GetGenericsByBaseClass(Type refType)
        {
            lock (GetGenericsByBaseClassLock)
            {
                if (GetGenericsByBaseClassCache.ContainsKey(refType)) return GetGenericsByBaseClassCache[refType];

                var classCol = new List<Type>();

                try
                {
                    foreach (var asy in AssemblyLoadMap.Values.ToList())
                        foreach (var st in asy.GetTypes())
                        {
                            if (st.BaseType == null) continue;
                            if (!st.BaseType.IsGenericType) continue;
                            if (st == refType) continue;

                            try
                            {
                                foreach (var gta in st.BaseType.GenericTypeArguments)
                                    if (gta == refType)
                                        classCol.Add(st);
                            }
                            catch { }
                        }

                    GetGenericsByBaseClassCache.Add(refType, classCol);
                }
                catch (Exception)
                {
                    // Current.Log.Add(e);
                }

                return classCol;
            }
        }

        /// <summary>
        ///     Gets a list of classes by implemented interface/base class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="excludeCoreNullDefinitions">
        ///     if set to <c>true</c> it ignores all core null providers, retuning only
        ///     external providers.
        /// </param>
        /// <returns>The list of classes.</returns>
        public static List<Type> GetClassesByInterface<T>(bool excludeCoreNullDefinitions = true) { return GetClassesByInterface(typeof(T), excludeCoreNullDefinitions); }

        public static IServiceCollection AddZenProvider<T>(this IServiceCollection serviceCollection, string descriptor = null) where T : class
        {
            var types = GetClassesByInterface<T>(false).FirstOrDefault();
            if (types == null) return serviceCollection;

            var probe = types.CreateInstance<T>();

            serviceCollection.AddSingleton(s => probe);
            Events.AddLog(descriptor, probe.ToString());

            return serviceCollection;
        }

        public static List<Type> GetClassesByInterface(Type targetType, bool excludeCoreNullDefinitions = true)
        {
            lock (Lock)
            {
                if (TypeResolutionMap.ContainsKey(targetType)) return TypeResolutionMap[targetType];

                //Modules.Log.System.Add("Scanning for " + type);

                var currentExecutingAssembly = Assembly.GetExecutingAssembly();

                var globalTypeList = new List<Type>();

                foreach (var item in AssemblyLoadMap.Values)
                {
                    if (excludeCoreNullDefinitions && item == currentExecutingAssembly) continue;

                    try
                    {
                        var partialTypeList =
                            from target in item.GetTypes() // Get a list of all Types in the cached Assembly
                            where !target.IsInterface // that aren't interfaces
                            where !target.IsAbstract // and also not abstract (so it can be instantiated)
                            where targetType.IsAssignableFrom(target) // that can be assigned to the specified type
                            where targetType != target // (and obviously not the type itself)
                            select target;

                        globalTypeList.AddRange(partialTypeList);
                    }
                    catch (Exception e)
                    {
                        if (e is ReflectionTypeLoadException)
                        {
                            var typeLoadException = e as ReflectionTypeLoadException;
                            var loaderExceptions = typeLoadException.LoaderExceptions.ToList();

                            //if (loaderExceptions.Count > 0) Modules.Log.System.Add("    Fail " + item + ": " + loaderExceptions[0].Message);
                            //else Modules.Log.System.Add("    Fail " + item + ": Undefined.");
                        }

                        // Well, this loading can fail by a (long) variety of reasons. 
                        // It's not a real problem not to catch exceptions here. 
                    }
                }

                var typesByPriorityLevelMap = globalTypeList
                    .Select(i => new KeyValuePair<int, Type>(((PriorityAttribute)i.GetCustomAttributes(typeof(PriorityAttribute), true).FirstOrDefault() ?? new PriorityAttribute()).Level, i))
                    .OrderBy(i => -i.Key);

                var typesByPriorityLevel = typesByPriorityLevelMap
                    .Select(i => i.Value)
                    .ToList();

                TypeResolutionMap.TryAdd(targetType, typesByPriorityLevel); // Caching results, so similar queries will return from cache

                return typesByPriorityLevel;
            }
        }
    }
}