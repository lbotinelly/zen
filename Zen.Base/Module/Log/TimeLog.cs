using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zen.Base.Module.Log
{
    public class TimeLog : Dictionary<TimeSpan, string>
    {
        private readonly Stopwatch _s = new Stopwatch();

        public string CurrentMessage { get; private set; }

        public string Log(string message)
        {
            Add(_s.Elapsed, message);
            CurrentMessage = message;
            return message;
        }

        public void Start(string message = null)
        {
            _s.Start();
            if (message != null) Log(message);
        }

        public void End()
        {
            _s.Stop();

            foreach (var entries in this) Current.Log.Info($"{entries.Key:\\:hh\\:mm\\:ss\\.fff} {entries.Value}");
            Current.Log.Info($"{_s.Elapsed:\\:hh\\:mm\\:ss\\.fff} [Total elapsed time]");
        }
    }
}