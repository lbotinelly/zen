using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Base.Module.Service;

namespace Zen.Base
{
    public static class Events
    {
        public static ActionQueue StartupSequence = new ActionQueue();
        public static ActionQueue ShutdownSequence = new ActionQueue();

        private static bool _doShutdown = true;
        private static Thread _workerThread;

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e) { End("Process Exit"); }

        internal static void Start()
        {
            Zen.Base.Module.Service.AutoService.Add();

            Status.SetState(Status.EState.Starting);

            Instances.ServiceData.StartTimeStamp = DateTime.Now;

            foreach (var ba in StartupSequence.Actions) try { ba(); } catch (Exception e) { Current.Log.Add(e); }

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            DumpStartInfo();

            Status.SetState(Status.EState.Running);

        }

        private static void DumpStartInfo()
        {
            Current.Log.Info(@"Zen " + System.Reflection.Assembly.GetCallingAssembly().GetName().Version);
            Current.Log.Debug("___________________________________________________________________________________________________");
            Current.Log.Debug("");
            Current.Log.Debug("Providers:");

            Current.Log.KeyValuePair("Cache", Current.Cache == null ? "(none)" : Current.Cache.ToString(), Message.EContentType.Debug);
            Current.Log.KeyValuePair("Environment", Current.Environment == null ? "(none)" : Current.Environment.ToString(), Message.EContentType.Debug);
            Current.Log.KeyValuePair("Log", Current.Log == null ? "(none)" : Current.Log.ToString(), Message.EContentType.Debug);
            Current.Log.KeyValuePair("Encryption", Current.Encryption == null ? "(none)" : Current.Encryption.ToString(), Message.EContentType.Debug);
            Current.Log.KeyValuePair("Global BundleType", Current.GlobalConnectionBundleType == null ? "(none)" : Current.GlobalConnectionBundleType.ToString(), Message.EContentType.Debug);

            Current.Log.KeyValuePair("Application", Host.ApplicationAssemblyName, Message.EContentType.Debug);
            Current.Log.KeyValuePair("App Location", Host.BaseDirectory, Message.EContentType.Debug);
            Current.Log.KeyValuePair("App Data", Host.DataDirectory, Message.EContentType.Debug);

            Current.Log.Debug("State:");
            Current.Log.KeyValuePair("Environment", Current.Environment?.Current.ToString(), Message.EContentType.Debug);

            Current.Log.Debug("___________________________________________________________________________________________________");
        }

        private static void ExecuteShutdownSequenceActions()
        {
            foreach (var sa in ShutdownSequence.Actions)
                try { sa(); } catch (Exception e) { Current.Log.Add(e); }
        }

        public static void End(string pReason = "(None)")
        {
            Current.Log.Add("Stack shutdown initiated: " + pReason, Message.EContentType.ShutdownSequence);

            if (Status.State == Status.EState.Shuttingdown) return;

            Status.SetState(Status.EState.Shuttingdown);

            Instances.ServiceData.EndTimeStamp = DateTime.Now;

            Current.Log.Debug($"    Session Start : {Instances.ServiceData.StartTimeStamp}");
            Current.Log.Debug($"      Session End : {Instances.ServiceData.EndTimeStamp}");
            Current.Log.Debug($" Session lifetime : {Instances.ServiceData.UpTime}");
            Current.Log.Add(@"  _|\_/|  ZZZzzz", Message.EContentType.Info);
            Current.Log.Add(@"c(_(-.-)", Message.EContentType.Info);

            ExecuteShutdownSequenceActions();

            //try { MediaTypeNames.Application.Exit(); }
            //catch { }
            try { Environment.Exit(0); } catch { }
        }

        public static void ScheduleShutdown(int seconds = 30)
        {
            _doShutdown = true;

            if (_workerThread != null) return;

            _workerThread = new Thread(() => Shutdown(seconds)) { IsBackground = false };
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