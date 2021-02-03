using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Zen.Base.Module.Log
{
    public class ScopedTimeLog : List<(string Timestamp, string Message, Message.EContentType scope)>, IDisposable
    {
        private readonly Stopwatch _s = new Stopwatch();

        private string _callerMemberName = "";

        public string CurrentMessage { get; private set; }

        #region IDisposable

        public void Dispose()
        {
            End();
        }

        #endregion

        public string Log(string message, Message.EContentType scope = Message.EContentType.Undefined,
            bool verbose = true, [CallerMemberName] string callerMemberName = null)
        {
            if (Host.IsDevelopment) _callerMemberName = $"[{callerMemberName}] ";

            Add((_s.Elapsed.ToString("G"), message, scope));

            CurrentMessage = message;
            return message;
        }

        public string LastMessage()
        {
            return this.LastOrDefault().Message;
        }

        public ScopedTimeLog Start(string message = null, Message.EContentType scope = Message.EContentType.Undefined,
            bool verbose = true)
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

            foreach (var (timestamp, message, scope) in this)
                Current.Log.Add($"{_callerMemberName}{timestamp} {message}", scope);

            Current.Log.Info($"{_s.Elapsed:\\:hh\\:mm\\:ss\\.fff} [Total elapsed time]");
        }

        public List<(string Timestamp, string Message, Message.EContentType scope)> MessagesByScope(
            Message.EContentType scope = Message.EContentType.Undefined)
        {
            return this.Where(i => i.scope == scope).ToList();
        }
    }
}