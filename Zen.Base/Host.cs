using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Zen.Base
{
    public static class Host
    {
        //todo: Move Base and Data to proper container class
        static Host()
        {
            BaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DataDirectory = $"{BaseDirectory}{Path.DirectorySeparatorChar}data";

            Version = Assembly.GetCallingAssembly().GetName().Version.ToString();
            Process = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            ApplicationAssembly = GetAppAssembly();
            ApplicationAssemblyName = ApplicationAssembly.GetName().Name;
            ApplicationAssemblyVersion = ApplicationAssembly.GetName().Version.ToString();

            IsDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            IsProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

            IsContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            if (!IsDevelopment) if (Environment.GetEnvironmentVariable("zen_Web__Development__QualifiedServerName") != null) IsDevelopment = true;

            if (!Directory.Exists(DataDirectory)) Directory.CreateDirectory(DataDirectory);
        }

        public static bool IsProduction { get; set; }

        public static class Keys
        {
            public static string WebHttpPort = "WebHttpPort";
            public static string WebHttpsPort = "WebHttpsPort";
        }

        public static Dictionary<string, object> Variables = new Dictionary<string, object>();

        public static bool IsContainer { get; }
        public static string ApplicationAssemblyVersion { get; set; }
        public static string BaseDirectory { get; }
        public static string DataDirectory { get; }
        public static string Version { get; }
        public static string ApplicationAssemblyName { get; }
        public static Assembly ApplicationAssembly { get; }
        public static string Process { get; }
        public static bool IsDevelopment { get; }

        private static Assembly GetAppAssembly() { return Assembly.GetEntryAssembly(); }
    }
}