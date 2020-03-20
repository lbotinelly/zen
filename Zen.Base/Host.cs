using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace Zen.Base
{
    public static class Host
    {
        public static SortedDictionary<string, object> Variables = new SortedDictionary<string, object>();

        static Host()
        {
            BaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DataDirectory = $"{BaseDirectory}{Path.DirectorySeparatorChar}data";

            Version = Assembly.GetCallingAssembly().GetName().Version.ToString();
            Process = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            ApplicationAssembly = Assembly.GetEntryAssembly();
            ApplicationAssemblyName = ApplicationAssembly?.GetName().Name;
            ApplicationAssemblyVersion = ApplicationAssembly?.GetName().Version.ToString();

            IsDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            IsProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
            IsContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            if (!IsDevelopment)
                if (Environment.GetEnvironmentVariable("zen_Web__Development__QualifiedServerName")!= null)
                    IsDevelopment = true;

            if (!Directory.Exists(DataDirectory)) Directory.CreateDirectory(DataDirectory);

            PopulateVariables();
        }

        public static bool IsProduction { get; set; }
        public static bool IsContainer { get; }
        public static string ApplicationAssemblyVersion { get; set; }
        public static string BaseDirectory { get; }
        public static string DataDirectory { get; }
        public static string Version { get; }
        public static string ApplicationAssemblyName { get; }
        public static Assembly ApplicationAssembly { get; }
        public static string Process { get; }
        public static bool IsDevelopment { get; }

        private static void PopulateVariables()
        {
            Variables[Keys.ApplicationAssembly] = ApplicationAssembly;
            Variables[Keys.ApplicationAssemblyName] = ApplicationAssemblyName;
            Variables[Keys.ApplicationAssemblyVersion] = ApplicationAssemblyVersion;
            Variables[Keys.ApplicationProcess] = Process;
            Variables[Keys.ApplicationVersion] = Version;
            Variables[Keys.EnvironmentIsContainer] = IsContainer;
            Variables[Keys.EnvironmentIsDevelopment] = IsDevelopment;
            Variables[Keys.EnvironmentIsProduction] = IsProduction;
            Variables[Keys.EnvironmentVersion] = Environment.Version;
            Variables[Keys.RuntimeFramework] = RuntimeInformation.FrameworkDescription;
        }

        public static class Keys
        {
            public static string ApplicationAssembly = "applicationAssembly";
            public static string ApplicationAssemblyName = "applicationAssemblyName";
            public static string ApplicationAssemblyVersion = "applicationAssemblyVersion";
            public static string ApplicationProcess = "applicationProcess";
            public static string ApplicationVersion = "applicationVersion";
            public static string EnvironmentIsContainer = "environmentIsContainer";
            public static string EnvironmentIsDevelopment = "environmentIsDevelopment";
            public static string EnvironmentIsProduction = "environmentIsProduction";
            public static string EnvironmentVersion = "environmentVersion";
            public static string RuntimeFramework = "runtimeFramework";
        }
    }
}