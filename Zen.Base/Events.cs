using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;

namespace Zen.Base
{
    public static class Events
    {
        internal static List<KeyValuePair<string, string>> BootLog = new List<KeyValuePair<string, string>>();

        public static ActionQueue StartupSequence = new ActionQueue();
        public static ActionQueue ShutdownSequence = new ActionQueue();

        private static bool _doShutdown = true;
        private static Thread _workerThread;

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            End("Process Exit");
        }

        public static void AddLog(string key, string value)
        {
            if (Status.State != Status.EState.Running) BootLog.Add(new KeyValuePair<string, string>(key, value));
            else Current.Log.KeyValuePair(key, value);
        }

        internal static void Start()
        {
            AutoService.Add();

            Status.SetState(Status.EState.Starting);

            Instances.ServiceData.StartTimeStamp = DateTime.Now;

            foreach (var ba in StartupSequence.Actions)
                try
                {
                    ba();
                }
                catch (Exception e)
                {
                    Current.Log.Add(e);
                }

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            DumpStartInfo();

            Status.SetState(Status.EState.Running);
        }

        private static void DumpStartInfo()
        {
            //var divider = new string('_', 130);

            //Current.Log.Debug(divider);
            Current.Log.Debug("");
            Current.Log.Info(@"Zen " + Assembly.GetCallingAssembly().GetName().Version);
            //Current.Log.Debug(divider);
            Current.Log.Debug("");
            Current.Log.Add("Providers:");

            var providersKeys = Status.Providers.ToList();

            foreach (var key in providersKeys)
            {
                var svc = key.Service();
                Current.Log.KeyValuePair(key.Name, $"{svc.GetType().Name} | {svc.GetState()}");
            }

            Current.Log.KeyValuePair("Base Directory", Host.BaseDirectory);
            Current.Log.KeyValuePair("Data Directory", Host.DataDirectory);

            Current.Log.Add("State:");

            foreach (var kvp in BootLog) Current.Log.KeyValuePair(kvp.Key, kvp.Value);

            //Current.Log.Debug(divider);
            Current.Log.Debug("");
        }

        private static void ExecuteShutdownSequenceActions()
        {
            foreach (var sa in ShutdownSequence.Actions)
                try
                {
                    sa();
                }
                catch (Exception e)
                {
                    Current.Log.Add(e);
                }
        }

        public static void End(string pReason = "(None)")
        {
            Current.Log.KeyValuePair("Stack shutdown initiated", pReason, Message.EContentType.ShutdownSequence);

            if (Status.State == Status.EState.ShuttingDown) return;

            Status.SetState(Status.EState.ShuttingDown);

            Instances.ServiceData.EndTimeStamp = DateTime.Now;

            Current.Log.KeyValuePair("Session Start", Instances.ServiceData.StartTimeStamp.ToString(),
                Message.EContentType.ShutdownSequence);
            Current.Log.KeyValuePair("Session End", Instances.ServiceData.EndTimeStamp.ToString(),
                Message.EContentType.ShutdownSequence);
            Current.Log.KeyValuePair("Session lifetime", Instances.ServiceData.UpTime.ToString(),
                Message.EContentType.ShutdownSequence);
            Current.Log.Add(@"  _|\_/|  ZZZzzz", Message.EContentType.Info);
            Current.Log.Add(@"c(_(-.-)", Message.EContentType.Info);

            ExecuteShutdownSequenceActions();

            //try { MediaTypeNames.Application.Exit(); }
            //catch { }
            try
            {
                Environment.Exit(0);
            }
            catch
            {
            }
        }

        public static void ScheduleShutdown(int seconds = 30)
        {
            _doShutdown = true;

            if (_workerThread != null) return;

            _workerThread = new Thread(() => Shutdown(seconds)) {IsBackground = false};
            _workerThread.Start();
        }

        private static void Shutdown(int seconds)
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

        private static void CancelShutdown()
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

        public class ActionQueue
        {
            public List<Action> Actions = new List<Action>();
        }
    }
}