using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Zen.Base.Module.Log
{
    public class TimeLog : List<KeyValuePair<string, string>>, IDisposable
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

            Add(new KeyValuePair<string, string>(_s.Elapsed.ToString("G"), message));
            CurrentMessage = message;
            return message;
        }

        public string LastMessage() => this.LastOrDefault().Value;

        public TimeLog Start(string message = null, bool verbose = true)
        {
            _s.Start();
            if (verbose)
                if (message != null)
                    Log(message);

            return this;
        }

        public void End(bool dumpInfo = true)
        {
            if (!_s.IsRunning) return;

            _s.Stop();

            if (!dumpInfo) return;
            foreach (var entries in this) Current.Log.Info($"{_callerMemberName}{entries.Key} {entries.Value}");
            Current.Log.Info($"{_s.Elapsed:\\:hh\\:mm\\:ss\\.fff} [Total elapsed time]");
        }
    }
}