using System;

namespace Zen.Base
{
    public static class Status
    {
        public enum EState
        {
            Unknown,
            Starting,
            Running,
            Shuttingdown
        }

        public static EState State { get; internal set; }

        internal static void SetState(EState newState) { State = newState; }

        public static string InstanceId = Guid.NewGuid().ToString();
    }
}