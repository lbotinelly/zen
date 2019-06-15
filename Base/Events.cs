using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zen.Base.Common;
using Zen.Base.Extension;
using Zen.Base.Internal;
using Zen.Base.Module.Log;

namespace Zen.Base
{
    public static class Events
    {
        public static ActionQueue StartupSequence = new ActionQueue();
        public static ActionQueue ShutdownSequence = new ActionQueue();


        private static bool _doShutdown = true;
        private static Thread _workerThread;


        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            End("Process Exit");
        }

        public static void Start()
        {
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
        }

        private static void DumpStartInfo()
        {
            Current.Log.Info(@"Zen " + System.Reflection.Assembly.GetCallingAssembly().GetName().Version);
            Current.Log.Debug("________________________________________________________________________________");
            Current.Log.Debug("");
            Current.Log.Debug($"            Cache : {(Current.Cache == null ? "(none)" : Current.Cache.ToString())}");
            Current.Log.Debug($"      Environment : {(Current.Environment == null ? "(none)" : Current.Environment.ToString())}");
            Current.Log.Debug($"              Log : {(Current.Log == null ? "(none)" : Current.Log.ToString())}");
            Current.Log.Debug($"       Encryption : {(Current.Encryption == null ? "(none)" : Current.Encryption.ToString())}");
            Current.Log.Debug($"    Authorization : {(Current.Authorization == null ? "(none)" : Current.Authorization.ToString())}");
            Current.Log.Debug($"Global BundleType : {(Current.GlobalConnectionBundleType == null ? "(none)" : Current.GlobalConnectionBundleType.ToString())}");
            Current.Log.Debug($"      Application : {Configuration.ApplicationAssemblyName}");
            Current.Log.Debug($"     App Location : {Configuration.BaseDirectory}");
            Current.Log.Debug($"         App Data : {Configuration.DataDirectory}");
            Current.Log.Debug("________________________________________________________________________________");
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
            Current.Log.Add("Stack shutdown initiated: " + pReason, Message.EContentType.ShutdownSequence);

            if (Status.State == Status.EState.Shuttingdown) return;

            Status.ChangeState(Status.EState.Shuttingdown);

            Instances.ServiceData.EndTimeStamp = DateTime.Now;

            Current.Log.Debug($"    Session Start : {(Instances.ServiceData.StartTimeStamp)}");
            Current.Log.Debug($"      Session End : {(Instances.ServiceData.EndTimeStamp)}");
            Current.Log.Debug($" Session lifetime : {(Instances.ServiceData.UpTime)}");
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

        public static void InitializeServices()
        {
            var providers = Instances.Services.Where(i => typeof(IZenProvider).IsAssignableFrom(i.ServiceType));

            foreach (var zenService in providers)
                ((IZenProvider) Instances.ServiceProvider.GetService(zenService.ServiceType)).Initialize();
        }


        public class ActionQueue
        {
            public List<Action> Actions = new List<Action>();
        }
    }
}