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

namespace Zen.Base.Module.Service
{
    public static class Resolution
    {
        private static readonly object Lock = new object();

        public static readonly ConcurrentDictionary<string, Assembly> AssemblyCache = new ConcurrentDictionary<string, Assembly>();
        public static readonly ConcurrentDictionary<string, string> UniqueAssemblies = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<Type, List<Type>> InterfaceClassesCache = new ConcurrentDictionary<Type, List<Type>>();

        private static readonly List<string> MonitorWhiteList = new List<string>
        {
            "InstallUtil.InstallLog",
            "*.InstallLog",
            "*.InstallState"
        };

        public static readonly Dictionary<Type, List<Type>> GetGenericsByBaseClassCache =
            new Dictionary<Type, List<Type>>();

        private static readonly object GetGenericsByBaseClassLock = new object();

        private static readonly List<string> _parserBlackList = new List<string> {"System.", "Microsoft."};

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

                foreach (var item in AssemblyCache)
                {
                    errCount = 0;

                    try { item.Value.GetTypes(); } catch (Exception e)
                    {
                        if (e.Message.IndexOf("LoaderExceptions", StringComparison.Ordinal) != -1)
                        {
                            // foreach (var exSub in ((ReflectionTypeLoadException) e).LoaderExceptions) Modules.Log.System.Add(exSub.Message, Message.EContentType.Warning);
                        }

                        errCount++;
                    }
                }
            }
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

            if (AssemblyCache.ContainsKey(assyName)) return assy;

            AssemblyCache[assyName] = assy;

            var refAssemblies = assy.GetReferencedAssemblies().Where(i => !_parserBlackList.Any(j => i.Name.StartsWith(j))).ToList();

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

                if (UniqueAssemblies.ContainsKey(p)) return;

                UniqueAssemblies.TryAdd(p, physicalPath);

                var assy = Assembly.LoadFrom(physicalPath);

                RegisterAssembly(assy);
            } catch (Exception e)
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
                    lock (Lock) { assySource = AssemblyCache.Values.ToList(); }

                foreach (var asy in assySource)
                    classCol.AddRange(asy
                                          .GetTypes()
                                          .Where(type => type.BaseType != null)
                                          .Where(
                                              type =>
                                                  type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == refType
                                                  || type.BaseType == refType));

                return classCol;
            } catch (ReflectionTypeLoadException ex)
            {
                foreach (var item in ex.LoaderExceptions)
                {
                    //Current.Log.Add(currAssembly?.GetName().Name + " @ " + currAssembly?.Location + ": " + item.Message, Message.EContentType.Warning);
                }

                throw ex;
            } catch (Exception ex)
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
                    foreach (var asy in AssemblyCache.Values.ToList())
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
                        } catch { }
                    }

                    GetGenericsByBaseClassCache.Add(refType, classCol);
                } catch (Exception)
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

        public static List<Type> GetClassesByInterface(Type type, bool excludeCoreNullDefinitions = true)
        {
            lock (Lock)
            {
                var preRet = new List<Type>();

                if (InterfaceClassesCache.ContainsKey(type)) return InterfaceClassesCache[type];

                //Modules.Log.System.Add("Scanning for " + type);

                foreach (var item in AssemblyCache.Values)
                {
                    if (excludeCoreNullDefinitions && item == Assembly.GetExecutingAssembly()) continue;

                    Type[] preTypes;

                    try { preTypes = item.GetTypes(); } catch (Exception e)
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
                        continue;
                    }

                    preRet.AddRange(
                        from target in preTypes
                        where !target.IsInterface
                        where !target.IsAbstract
                        where type.IsAssignableFrom(target)
                        where type != target
                        select target);
                }

                var priorityList = new List<KeyValuePair<int, Type>>();

                //Modules.Log.System.Add("    " + preRet.Count + " [" + type + "] items");

                foreach (var item in preRet)
                {
                    var level = 0;

                    var attrs = item.GetCustomAttributes(typeof(PriorityAttribute), true).FirstOrDefault();

                    if (attrs != null) level = ((PriorityAttribute) attrs).Level;

                    priorityList.Add(new KeyValuePair<int, Type>(level, item));
                }

                priorityList.Sort((firstPair, nextPair) => nextPair.Key - firstPair.Key);

                //foreach (var item in priorityList) Modules.Log.System.Add("        " + item.Key + " " + item.Value.Name);

                var ret = priorityList.Select(item => item.Value).ToList();

                InterfaceClassesCache.TryAdd(type, ret); // Caching results, so similar queries will return from cache

                return ret;
            }
        }
    }
}