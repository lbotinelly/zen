﻿using System.Text;

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

        internal static void ChangeState(EState newState) { State = newState; }
    }
}
