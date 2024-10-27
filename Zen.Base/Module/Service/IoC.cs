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
    public static class IoC
    {
        private static readonly object Lock = new object();

        public static readonly ConcurrentDictionary<string, Assembly> AssemblyLoadMap = new ConcurrentDictionary<string, Assembly>();
        private static readonly ConcurrentDictionary<string, string> AssemblyPathMap = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<Type, List<KeyValuePair<int, Type>>> TypeResolutionMap = new ConcurrentDictionary<Type, List<KeyValuePair<int, Type>>>();
        private static readonly ConcurrentDictionary<Type, List<KeyValuePair<int, Type>>> AttributeResolutionMap = new ConcurrentDictionary<Type, List<KeyValuePair<int, Type>>>();

        public static readonly Dictionary<Type, List<Type>> GetGenericsByBaseClassCache = new Dictionary<Type, List<Type>>();

        private static readonly object GetGenericsByBaseClassLock = new object();

        private static readonly List<string> IgnoreList = new List<string> {"System.", "Microsoft.", "mscorlib", "netstandard", "Serilog.", "ByteSize", "AWSSDK.", "StackExchange.", "SixLabors.", "BouncyCastle.", "MongoDB.", "Dapper", "SharpCompress", "Remotion", "Markdig", "Westwind", "Serilog", "DnsClient", "Oracle"};

        static IoC()
        {
            //Modules.Log.System.Add("Warm-up START", Message.EContentType.StartupSequence);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var self = Assembly.GetEntryAssembly();

            if (self != null) RegisterAssembly(self);

            // 1st cycle: Local (base directory) assemblies

            LoadAssembliesFromDirectory(Host.BaseDirectory);

            //2nd cycle: Directories/assemblies referenced by system

            //    First by process-specific variables...
            LoadAssembliesFromDirectory(System.Environment.GetEnvironmentVariable(ConstantStrings.zen_ver, EnvironmentVariableTarget.Process));
            //    then by user-specific variables...
            LoadAssembliesFromDirectory(System.Environment.GetEnvironmentVariable(ConstantStrings.zen_ver, EnvironmentVariableTarget.User));
            //    and finally system-wide variables.
            LoadAssembliesFromDirectory(System.Environment.GetEnvironmentVariable(ConstantStrings.zen_ver, EnvironmentVariableTarget.Machine));

            //Now try to load:

            var lastErrCount = -1;
            var errCount = 0;

            while (errCount != lastErrCount)
            {
                lastErrCount = errCount;

                //var modList = AssemblyCache.Select(i => i.Value.ToString().Split(',')[0]).ToJson();

                foreach (var item in AssemblyLoadMap)
                    try
                    {
                        item.Value.GetTypes();
                    }
                    catch (Exception e)
                    {
                        Base.Log.Add("Error while loading " + item.Key, e);
                    }
            }

            Base.Log.KeyValuePair("Assembly Loader", $"{AssemblyLoadMap.Count} assemblies registered", Message.EContentType.StartupSequence);
            // foreach (var assembly in new SortedDictionary<string, Assembly>(AssemblyLoadMap)) Base.Log.KeyValuePair(assembly.Key, assembly.Value.Location);
        }

        public static List<T> GetInstances<T>(bool excludeCoreNullDefinitions = true) where T : class
        {
            return GetClassesByInterface<T>(excludeCoreNullDefinitions).Select(i => i.CreateInstance<T>()).ToList();
        }

        private static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

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
                    lock (Lock)
                    {
                        LoadAssembly(path);
                    }
                }
            }
        }

        public static Assembly RegisterAssembly(Assembly assy)
        {
            var assyName = assy.GetName().Name;

            if (IgnoreList.Any(j => assyName.StartsWith(j))) return null;

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

                RegisterAssembly(Assembly.LoadFrom(physicalPath));
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

        public static List<Type> TypeByName(string typeName) =>
            AssemblyLoadMap.Values.ToList()
                .SelectMany(i => i.GetTypes().Where(j => !j.IsInterface && !j.IsAbstract && (j.Name.Equals(typeName) || j.FullName?.Equals(typeName) == true)))
                .ToList();

        public static List<Type> GetClassesByBaseClass(Type refType, bool limitToMainAssembly = false)
        {
            try
            {
                var classCol = new List<Type>();

                var assySource = new List<Assembly>();

                if (limitToMainAssembly) assySource.Add(Host.ApplicationAssembly);
                else
                    lock (Lock)
                    {
                        assySource = AssemblyLoadMap.Values.ToList();
                    }

                foreach (var asy in assySource)
                    classCol.AddRange(asy.GetTypes()
                        .Where(type => type.BaseType != null)
                        .Where(type => type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == refType || type.BaseType == refType));

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
        /// <param name="excludeFallback">
        ///     if set to <c>true</c> it ignores fallback providers, retuning only defined providers.
        /// </param>
        /// <returns>The list of classes.</returns>
        public static List<Type> GetClassesByInterface<T>(bool excludeFallback = false) => GetClassesByInterface(typeof(T), excludeFallback);

        public static IServiceCollection AddZenProvider<T>(this IServiceCollection serviceCollection, string descriptor = null) where T : class
        {
            var targetType = GetClassesByInterface<T>(false).FirstOrDefault();
            if (targetType == null) return serviceCollection;

            var isInstantiable = targetType.GetConstructor(Type.EmptyTypes) != null;

            if (isInstantiable)
            {
                var probe = targetType.CreateInstance<T>();

                serviceCollection.AddSingleton(s => probe);
                Events.AddLog(descriptor, probe.ToString());
            }
            else
            {
                serviceCollection.AddSingleton(typeof(T), targetType);
            }

            return serviceCollection;
        }

        public static List<Type> GetClassesByInterface(Type targetType, bool excludeFallback = true)
        {
            lock (Lock)
            {
                if (!TypeResolutionMap.ContainsKey(targetType))
                {
                    //Modules.Log.System.Add("Scanning for " + type);

                    var currentExecutingAssembly = Assembly.GetExecutingAssembly();

                    var globalTypeList = new List<Type>();

                    foreach (var item in AssemblyLoadMap.Values)
                        try
                        {



                            var partialTypeList =
                                from target in item.GetTypes() // Get a list of all Types in the cached Assembly
                                where !target.IsInterface // that aren't interfaces
                                where !target.IsAbstract // and also not abstract (so it can be instantiated)
                                where !target.GetCustomAttributes(typeof(IoCIgnoreAttribute), false).Any() // Must not be marked to be ignored
                                where !targetType.IsInterface ? targetType.IsAssignableFrom(target): target.GetInterfaces().Contains(targetType) // that can be assigned to the specified type
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
                            }

                            // Well, this loading can fail by a (long) variety of reasons. 
                            // It's not a real problem not to catch exceptions here. 
                        }

                    var typesByPriorityLevelMap = globalTypeList
                        .Select(i => new KeyValuePair<int, Type>(((PriorityAttribute)i.GetCustomAttributes(typeof(PriorityAttribute), true).FirstOrDefault() ?? new PriorityAttribute()).Level, i))
                        .OrderBy(i => -i.Key).ToList();

                    TypeResolutionMap.TryAdd(targetType, typesByPriorityLevelMap); // Caching results, so similar queries will return from cache
                }

                var typeListCache = !excludeFallback ? TypeResolutionMap[targetType] : TypeResolutionMap[targetType].Where(i => i.Key > -1).ToList();

                var typesByPriorityLevel = typeListCache
                    .Select(i => i.Value)
                    .ToList();

                return typesByPriorityLevel;
            }
        }

        public static Dictionary<Type, T> GetClassesByAttribute<T>()
        {
            var targetAttribute = typeof(T);

            lock (Lock)
            {
                if (!AttributeResolutionMap.ContainsKey(targetAttribute))
                {
                    //Modules.Log.System.Add("Scanning for " + type);

                    var currentExecutingAssembly = Assembly.GetExecutingAssembly();

                    var attributedTypes = new List<Type>();

                    foreach (var item in AssemblyLoadMap.Values)
                        try
                        {
                            var partialTypeList =
                                from target in item.GetTypes() // Get a list of all Types in the cached Assembly
                                where !target.IsInterface // that aren't interfaces
                                where !target.IsAbstract // and also not abstract (so it can be instantiated)
                                where !target.GetCustomAttributes(typeof(IoCIgnoreAttribute), false).Any() // Must not be marked to be ignored
                                where target.GetCustomAttributes(targetAttribute, false).Any() // Must have attribute of specified type
                                select target;

                            attributedTypes.AddRange(partialTypeList);
                        }
                        catch (Exception e)
                        {
                            if (e is ReflectionTypeLoadException)
                            {
                                var typeLoadException = e as ReflectionTypeLoadException;
                                var loaderExceptions = typeLoadException.LoaderExceptions.ToList();
                            }

                            // Well, this loading can fail by a (long) variety of reasons. 
                            // It's not a real problem not to catch exceptions here. 
                        }

                    var typesByPriorityLevelMap = attributedTypes
                        .Select(i => new KeyValuePair<int, Type>(((PriorityAttribute)i.GetCustomAttributes(typeof(PriorityAttribute), true).FirstOrDefault() ?? new PriorityAttribute()).Level, i))
                        .OrderBy(i => -i.Key).ToList();

                    AttributeResolutionMap.TryAdd(targetAttribute, typesByPriorityLevelMap); // Caching results, so similar queries will return from cache
                }

                var typeListCache = AttributeResolutionMap[targetAttribute].ToList();

                var typesWithAttribute = typeListCache
                        .ToDictionary(i =>
                                i.Value, i => (T)i.Value.GetCustomAttributes(targetAttribute).Select(j=> (object)j).FirstOrDefault())
                    ;

                return typesWithAttribute;
            }
        }
    }
}