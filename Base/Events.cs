using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
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

            Stop();

            Current.Log.Add("Shutting down " + Current.Authorization.GetType().Name, Message.EContentType.MoreInfo);
            Current.Authorization.Shutdown();

            Current.Log.Add("Shutting down " + Current.Cache.GetType().Name, Message.EContentType.MoreInfo);
            Current.Cache.Shutdown();

            Current.Log.Add("Shutting down " + Current.Environment.GetType().Name, Message.EContentType.MoreInfo);
            Current.Environment.Shutdown();

            Current.Log.Add("Shutting down " + Current.Encryption.GetType().Name, Message.EContentType.MoreInfo);
            Current.Encryption.Shutdown();

            Current.Log.Add(@"  _|\_/|  ZZZzzz", Message.EContentType.Info);
            Current.Log.Add(@"c(_(-.-)", Message.EContentType.Info);

            Current.Log.Add(@"Stack shutdown concluded.", Message.EContentType.ShutdownSequence);

            Current.Log.Shutdown();

            try { MediaTypeNames.Application.Exit(); }
            catch { }
            try { Environment.Exit(0); }
            catch { }
        }


    }
}
