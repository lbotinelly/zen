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
            ApplicationAssemblyVersion = ApplicationAssembly.ImageRuntimeVersion;

            if (!Directory.Exists(DataDirectory)) Directory.CreateDirectory(DataDirectory);
        }
        public static string ApplicationAssemblyVersion { get; set; }
        public static string BaseDirectory { get; }
        public static string DataDirectory { get; }
        public static string Version { get; }
        public static string ApplicationAssemblyName { get; }
        public static Assembly ApplicationAssembly { get; }
        public static string Process { get; }

        private static Assembly GetAppAssembly() { return Assembly.GetEntryAssembly(); }
    }
}