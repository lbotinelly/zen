using System;
using System.Collections.Generic;
using Zen.Base.Common;
using Zen.Base.Diagnostics;

namespace Zen.Base.Module.Log
{
    [Serializable]
    public class Message
    {
        #region Enumerators

        //Type-safe-enum pattern standard interface

        public enum EContentType
        {
            Critical = 1,
            Audit = 10,
            Exception = 20,
            StartupSequence = 30,
            ShutdownSequence = 40,
            Warning = 50,
            Maintenance = 60,
            Info = 70,
            MoreInfo = 80,
            Generic = 90,
            Debug = 100,
            Undefined = 999
        }

        public static Dictionary<EContentType, string> AnsiColors = new Dictionary<EContentType, string>
        {



            {EContentType.Critical, AnsiTerminalColorCode.ANSI_WHITE + AnsiTerminalColorCode.ANSI_RED_BACKGROUND},
            {EContentType.Audit, AnsiTerminalColorCode.ANSI_WHITE + AnsiTerminalColorCode.ANSI_BLUE_BACKGROUND},
            {EContentType.Exception, AnsiTerminalColorCode.ANSI_BRIGHT_RED},
            {EContentType.StartupSequence, AnsiTerminalColorCode.ANSI_BRIGHT_CYAN},
            {EContentType.ShutdownSequence, AnsiTerminalColorCode.ANSI_CYAN},
            {EContentType.Warning, AnsiTerminalColorCode.ANSI_BRIGHT_RED},
            {EContentType.Maintenance, AnsiTerminalColorCode.ANSI_YELLOW},
            {EContentType.Info, AnsiTerminalColorCode.ANSI_BRIGHT_CYAN},
            {EContentType.MoreInfo, AnsiTerminalColorCode.ANSI_CYAN},
            {EContentType.Generic, AnsiTerminalColorCode.ANSI_WHITE},
            {EContentType.Debug, AnsiTerminalColorCode.ANSI_BRIGHT_BLACK}
        };

        public static Dictionary<EContentType, string> ContentCode = new Dictionary<EContentType, string>
        {
            {EContentType.Critical, "CRITL"},
            {EContentType.Audit, "AUDIT"},
            {EContentType.Exception, "EXEPT"},
            {EContentType.StartupSequence, "START"},
            {EContentType.ShutdownSequence, "SDOWN"},
            {EContentType.Warning, " WARN"},
            {EContentType.Maintenance, "MAINT"},
            {EContentType.Info, " INFO"},
            {EContentType.MoreInfo, " MORE"},
            {EContentType.Generic,  "    >"},
            {EContentType.Debug,    "     "}
        };

        #endregion

        #region Exposed Properties

        public EContentType Type = EContentType.Generic;
        public Guid Id { get; set; }
        public DateTime CreationTime { get; set; }
        public string Content { get; set; }
        public Guid? ReplyToId { get; set; }
        public TraceInfoContainer TraceInfo { get; set; }
        public string Topic { get; set; }
        public string Subject { get; set; }

        #endregion

        #region Initialization

        public Message() { Initialize(); }

        public Message(string message)
        {
            Initialize();
            Content = message;
        }

        private void Initialize()
        {
            Id = Guid.NewGuid();
            TraceInfo = new TraceInfoContainer();
            CreationTime = DateTime.Now;
            TraceInfo.Gather();
        }

        #endregion
    }
}