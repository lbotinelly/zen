using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Zen.Base.Common;

namespace Zen.Base.Assembly
{
    public static class Management
    {
        private static readonly object Lock = new object();

        public static readonly ConcurrentDictionary<string, System.Reflection.Assembly> AssemblyCache =
            new ConcurrentDictionary<string, System.Reflection.Assembly>();

        private static readonly ConcurrentDictionary<Type, List<Type>> InterfaceClassesCache =
            new ConcurrentDictionary<Type, List<Type>>();

        private static readonly List<FileSystemWatcher> FsMonitors = new List<FileSystemWatcher>();
        private static readonly List<string> WatchedSources = new List<string>();

        public static readonly ConcurrentDictionary<string, string> UniqueAssemblies =
            new ConcurrentDictionary<string, string>();

        private static readonly List<string> MonitorWhiteList = new List<string>
        {
            "InstallUtil.InstallLog",
            "*.InstallLog",
            "*.InstallState"
        };

        public static readonly Dictionary<Type, List<Type>> GetGenericsByBaseClassCache =
            new Dictionary<Type, List<Type>>();

        private static readonly object GetGenericsByBaseClassLock = new object();

        static Management()
        {
            //Modules.Log.System.Add("Warm-up START", Message.EContentType.StartupSequence);

#pragma warning disable 618
            AppDomain.CurrentDomain.SetShadowCopyFiles();

            var targetScDir = $"{Configuration.DataDirectory}{Path.DirectorySeparatorChar}sc";

            if (!Directory.Exists(targetScDir)) Directory.CreateDirectory(targetScDir);

            AppDomain.CurrentDomain.SetCachePath(targetScDir);

#pragma warning restore 618

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var self = System.Reflection.Assembly.GetEntryAssembly();

            if (self != null) AssemblyCache.TryAdd(self.ToString(), self);

            // 1st cycle: Local (base directory) assemblies

            LoadAssembliesFromDirectory(Configuration.BaseDirectory);

            //2nd cycle: Directories/assemblies referenced by system

            //    First by process-specific variables...
            LoadAssembliesFromDirectory(
                Environment.GetEnvironmentVariable(Strings.zen_ver, EnvironmentVariableTarget.Process));
            //    then by user-specific variables...
            LoadAssembliesFromDirectory(
                Environment.GetEnvironmentVariable(Strings.zen_ver, EnvironmentVariableTarget.User));
            //    and finally system-wide variables.
            LoadAssembliesFromDirectory(
                Environment.GetEnvironmentVariable(Strings.zen_ver, EnvironmentVariableTarget.Machine));

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
                            //foreach (var exSub in ((ReflectionTypeLoadException) e).LoaderExceptions) Modules.Log.System.Add(exSub.Message, Message.EContentType.Warning);
                        }

                        errCount++;
                    }
                }
            }
        }

        private static System.Reflection.Assembly GetAssemblyByName(string name) { return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name); }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //if (args.RequestingAssembly != null) Modules.Log.System.Add("        " + args.RequestingAssembly.FullName + ": Resolution request");

            //Modules.Log.System.Add("            Resolving " + args.Name);
            var shortName = args.Name.Split(',')[0];

            var probe = GetAssemblyByName(shortName);

            //if (probe == null) Modules.Log.System.Add("            [ERR] NOT FOUND");
            //else Modules.Log.System.Add("            OK      : " + probe);
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
                if (WatchedSources.Contains(path)) return;
                WatchedSources.Add(path);

                AppDomain.CurrentDomain.SetShadowCopyPath(path);

                var attr = File.GetAttributes(path);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    //It's a directory: Load all assemblies and set up monitor.

                    var assylist = Directory.GetFiles(path, "*.dll");

                    foreach (var dll in assylist) LoadAssemblyFromPath(dll);

                    var watcher = new FileSystemWatcher
                    {
                        Path = path,
                        IncludeSubdirectories = false,
                        NotifyFilter = NotifyFilters.LastWrite,
                        Filter = "*.*"
                    };
                    watcher.Changed += FileSystemWatcher_OnChanged;
                    watcher.EnableRaisingEvents = true;

                    FsMonitors.Add(watcher);

                    //Modules.Log.System.Add($"    Watching {path}", Message.EContentType.Info);
                }
                else
                {
                    //It's a file: Load it directly.
                    LoadAssemblyFromPath(path);
                }
            }
        }

        public static void MonitorFile(string path)
        {
            var _path = path;
            var filter = "*.*";

            var attr = File.GetAttributes(path);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
            {
                var fileinfo = new FileInfo(path);

                _path = fileinfo.DirectoryName;
                filter = fileinfo.Name;
            }

            var watcher = new FileSystemWatcher
            {
                Path = _path,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = filter
            };

            watcher.Changed += FileSystemWatcher_OnChanged;
            watcher.EnableRaisingEvents = true;

            FsMonitors.Add(watcher);

            //Modules.Log.System.Add("Monitoring [" + path + "]", Message.EContentType.StartupSequence);
        }

        public static string WildcardToRegex(string pattern) { return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$"; }

        private static void FileSystemWatcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            if (Status.State == Status.EState.Shuttingdown) return;

            var name = Path.GetFileName(e.FullPath);

            foreach (var i in MonitorWhiteList)
                if (i.IndexOf("*", StringComparison.Ordinal) != -1)
                {
                    var match = i.Replace("*.", "");

                    //TODO: Improve wildcard detection
                    if (name.IndexOf(match) > -1) return;
                }
                else if (i.Equals(name)) { return; }

            // No need for system monitors anymore, better to interrupt and dispose all of them.
            foreach (var i in FsMonitors)
            {
                i.EnableRaisingEvents = false;
                i.Changed -= FileSystemWatcher_OnChanged;
                i.Dispose();
            }

            FsMonitors.Clear();

            Status.ChangeState(Status.EState.Shuttingdown);

            //Current.Log.UseScheduler = false;
            //Current.Log.Add("[" + e.FullPath + "]: Change detected", Message.EContentType.ShutdownSequence);
            //Modules.Log.System.Add("[" + e.FullPath + "]: Change detected", Message.EContentType.ShutdownSequence);

            //For Web apps
            try
            {
                //HttpRuntime.UnloadAppDomain();
            } catch { }

            //For WinForm apps
            try
            {
                //Application.Restart(); Environment.Exit(0);
            } catch { }
        }

        private static void LoadAssemblyFromPath(string path)
        {
            if (path == null) return;

            try
            {
                var p = Path.GetFileName(path);

                if (UniqueAssemblies.ContainsKey(p)) return;

                UniqueAssemblies.TryAdd(p, path);

                var assy = System.Reflection.Assembly.LoadFrom(path);

                lock (Lock)
                {
                    if (!AssemblyCache.ContainsKey(assy.ToString())) AssemblyCache.TryAdd(assy.ToString(), assy);
                }
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

                var assySource = new List<System.Reflection.Assembly>();

                if (limitToMainAssembly) assySource.Add(Configuration.ApplicationAssembly);
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
                } catch (Exception e)
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
        public static List<Type> GetClassesByInterface<T>(bool excludeCoreNullDefinitions = true)
        {
            lock (Lock)
            {
                var type = typeof(T);
                var preRet = new List<Type>();

                if (InterfaceClassesCache.ContainsKey(type)) return InterfaceClassesCache[type];

                //Modules.Log.System.Add("Scanning for " + type);

                foreach (var item in AssemblyCache.Values)
                {
                    if (excludeCoreNullDefinitions && item == System.Reflection.Assembly.GetExecutingAssembly()) continue;

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

        public static List<Type> GetClassesByInterface<T, TU>(bool excludeCoreNullDefinitions = true)
        {
            lock (Lock)
            {
                var typeT = typeof(T);
                var typeU = typeof(TU);
                var preRet = new List<Type>();

                //Modules.Log.System.Add("Scanning for " + typeT + "+" + typeU);

                foreach (var item in AssemblyCache.Values)
                {
                    if (excludeCoreNullDefinitions && item == System.Reflection.Assembly.GetExecutingAssembly()) continue;

                    Type[] preTypes;

                    try { preTypes = item.GetTypes(); } catch (Exception e)
                    {
                        if (e is ReflectionTypeLoadException typeLoadException)
                        {
                            var loaderExceptions = typeLoadException.LoaderExceptions.ToList();

                            //if (loaderExceptions.Count > 0) Modules.Log.System.Add("    Fail " + item + ": " + loaderExceptions[0].Message);
                            //else Modules.Log.System.Add("    Fail " + item + ": Undefined.");
                        }

                        // Well, this loading can fail by a (long) variety of reasons. 
                        // It's not a real problem not to catch exceptions here. 
                        continue;
                    }

                    foreach (var item3 in preTypes)
                    {
                        if (item3.IsInterface) continue;

                        if (!typeT.IsAssignableFrom(item3)) continue;
                        if (!typeU.IsAssignableFrom(item3)) continue;

                        if (typeT == item3) continue;
                        if (typeU != item3) preRet.Add(item3);
                    }
                }

                var priorityList = new List<KeyValuePair<int, Type>>();

                //Modules.Log.System.Add("    " + preRet.Count + " [" + typeT + "] items");

                foreach (var item in preRet)
                {
                    var level = 0;

                    var attrs = (PriorityAttribute) item.GetCustomAttributes(typeof(PriorityAttribute), true)
                        .FirstOrDefault();

                    if (attrs != null) level = attrs.Level;

                    priorityList.Add(new KeyValuePair<int, Type>(level, item));
                }

                priorityList.Sort((firstPair, nextPair) => nextPair.Key - firstPair.Key);

                //foreach (var item in priorityList) Modules.Log.System.Add("        " + item.Key + " " + item.Value.Name);

                var ret = priorityList.Select(item => item.Value).ToList();

                return ret;
            }
        }
    }
}