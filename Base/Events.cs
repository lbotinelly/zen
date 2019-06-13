using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Base
{
    public static class Events
    {
        public class ActionQueue
        {
            public List<Action> Actions = new List<Action>();
        }

        public static ActionQueue Bootup = new ActionQueue();
        public static ActionQueue Shutdown = new ActionQueue();

        public static void Start()
        {
            foreach (var ba in Bootup.Actions)
            {
                try { ba(); } catch (Exception e) { Current.Log.Add(e); }
            }
        }

        public static void Stop()
        {
            foreach (var sa in Shutdown.Actions)
            {
                try { sa(); }
                catch (Exception e) { Current.Log.Add(e); }
            }
        }

        public static void End(string pReason = "(None)")
        {
            Current.Log.Add("Stack shutdown initiated: " + pReason, Message.EContentType.ShutdownSequence);

            if (Status.State == Status.EState.Shuttingdown) return;

            Status.ChangeState(Status.EState.Shuttingdown);

            Current.Log.Add("Shutting down...", Message.EContentType.MoreInfo);
            Current.Log.Add(@"  _|\_/|  ZZZzzz", Message.EContentType.Info);
            Current.Log.Add(@"c(_(-.-)", Message.EContentType.Info);

            Stop();

            //try { MediaTypeNames.Application.Exit(); }
            //catch { }
            try { Environment.Exit(0); }
            catch { }
        }


        private static bool _doShutdown = true;

        private static Thread _workerThread;

        public static void ScheduleTakeDown(int seconds = 30)
        {
            _doShutdown = true;

            if (_workerThread != null) return;

            _workerThread = new Thread(() => TakeDown(seconds)) { IsBackground = false };
            _workerThread.Start();
        }

        private static void TakeDown(int seconds)
        {
            Current.Log.Add("Scheduling shutdown: {0} seconds".format(seconds), Message.EContentType.Maintenance);

            Thread.Sleep(seconds * 1000);

            if (_doShutdown)
            {
                Current.Log.Add("Starting scheduled shutdown", Message.EContentType.Maintenance);
                Thread.Sleep(2 * 1000);
                End("Scheduled shutdown");
            }
        }

        private static void CancelTakeDown()
        {
            if (!_doShutdown)
            {
                Current.Log.Add("CancelTakeDown: No scheduled shutdown", Message.EContentType.Info);
                return;
            }

            _doShutdown = false;
            _workerThread.Abort();

            Current.Log.Add("CancelTakeDown successful.", Message.EContentType.ShutdownSequence);
        }
    }
}
