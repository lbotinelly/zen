// ReSharper disable InconsistentNaming

namespace Zen.Base.Common
{
    // https://stackoverflow.com/a/5762502/1845714
    public static class AnsiTerminalColorCode
    {
        public static string ANSI_RESET = "\x1b[0m";

        public static string ANSI_BLACK = "\x1b[30m";
        public static string ANSI_RED = "\x1b[31m";
        public static string ANSI_GREEN = "\x1b[32m";
        public static string ANSI_YELLOW = "\x1b[33m";
        public static string ANSI_BLUE = "\x1b[34m";
        public static string ANSI_PURPLE = "\x1b[35m";
        public static string ANSI_CYAN = "\x1b[36m";
        public static string ANSI_WHITE = "\x1b[37m";

        public static string ANSI_BRIGHT_BLACK = "\x1b[90m";
        public static string ANSI_BRIGHT_RED = "\x1b[91m";
        public static string ANSI_BRIGHT_GREEN = "\x1b[92m";
        public static string ANSI_BRIGHT_YELLOW = "\x1b[93m";
        public static string ANSI_BRIGHT_BLUE = "\x1b[94m";
        public static string ANSI_BRIGHT_PURPLE = "\x1b[95m";
        public static string ANSI_BRIGHT_CYAN = "\x1b[96m";
        public static string ANSI_BRIGHT_WHITE = "\x1b[97m";

        public static string ANSI_BLACK_BACKGROUND = "\x1b[40m";
        public static string ANSI_RED_BACKGROUND = "\x1b[41m";
        public static string ANSI_GREEN_BACKGROUND = "\x1b[42m";
        public static string ANSI_YELLOW_BACKGROUND = "\x1b[43m";
        public static string ANSI_BLUE_BACKGROUND = "\x1b[44m";
        public static string ANSI_PURPLE_BACKGROUND = "\x1b[45m";
        public static string ANSI_CYAN_BACKGROUND = "\x1b[46m";
        public static string ANSI_WHITE_BACKGROUND = "\x1b[47m";
    }
}