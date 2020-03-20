using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Zen.Base.Module.Log
{
    public class TimeLog : Dictionary<TimeSpan, string>, IDisposable
    {
        private readonly Stopwatch _s = new Stopwatch();

        private string _callerMemberName = "";

        public string CurrentMessage { get; private set; }

        #region IDisposable

        public void Dispose() { End(); }

        #endregion

        public string Log(string message, bool verbose = true, [CallerMemberName] string callerMemberName = null)
        {
            if (Host.IsDevelopment) _callerMemberName = $"[{callerMemberName}] ";

            Add(_s.Elapsed, message);
            CurrentMessage = message;
            return message;
        }

        public TimeLog Start(string message = null, bool verbose = true)
        {
            _s.Start();
            if (verbose)
                if (message!= null)
                    Log(message);

            return this;
        }

        public void End(bool dumpInfo = true)
        {
            if (!_s.IsRunning) return;

            _s.Stop();

            if (!dumpInfo) return;
            foreach (var entries in this) Current.Log.Info($"{_callerMemberName}{entries.Key:\\:hh\\:mm\\:ss\\.fff} {entries.Value}");
            Current.Log.Info($"{_s.Elapsed:\\:hh\\:mm\\:ss\\.fff} [Total elapsed time]");
        }
    }
}