#if DEVELOPMENT_MODE
using System;
using System.Runtime.InteropServices;

#endif

namespace Zen.Base.Module.Log
{
    internal static class WindowsConsole
    {
        internal static bool IsAnsi = false;
#if DEVELOPMENT_MODE
        public static void EnableVirtualTerminalProcessing()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            var stdout = GetStdHandle(StandardOutputHandleId);
            if (stdout == (IntPtr) InvalidHandleValue || !GetConsoleMode(stdout, out var mode)) return;

            SetConsoleMode(stdout, mode | EnableVirtualTerminalProcessingMode);
            IsAnsi = true;
        }

        private const int StandardOutputHandleId = -11;
        private const uint EnableVirtualTerminalProcessingMode = 4;
        private const long InvalidHandleValue = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int handleId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr handle, out uint mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr handle, uint mode);
#else
        public static void EnableVirtualTerminalProcessing() { }
#endif
    }
}