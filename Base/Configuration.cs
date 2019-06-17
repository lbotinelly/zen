using System.Diagnostics;
using System.IO;

namespace Zen.Base
{
    public static class Configuration
    {
        //todo: Move Base and Data to proper container class
        static Configuration()
        {
            BaseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            DataDirectory = $"{BaseDirectory}{Path.DirectorySeparatorChar}data";

            Version = System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString();
            Host = Process.GetCurrentProcess().ProcessName;

            ApplicationAssembly = GetAppAssembly();
            ApplicationAssemblyName = ApplicationAssembly.GetName().Name;

            if (!Directory.Exists(DataDirectory)) Directory.CreateDirectory(DataDirectory);
        }

        public static string BaseDirectory { get; }
        public static string DataDirectory { get; }
        public static string Version { get; }
        public static string ApplicationAssemblyName { get; }
        public static System.Reflection.Assembly ApplicationAssembly { get; }
        public static string Host { get; }

        private static System.Reflection.Assembly GetAppAssembly()
        {
            return System.Reflection.Assembly.GetEntryAssembly();
        }
    }
}