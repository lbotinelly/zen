using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Common;
using Zen.Base.Module.Service;

namespace Zen.Base
{
    public static class Status
    {
        public enum EState
        {
            Unknown,
            Starting,
            Running,
            ShuttingDown
        }
        public static EState State { get; internal set; }

        internal static void SetState(EState newState) { State = newState; }

        public static string InstanceId = Guid.NewGuid().ToString();

        public static List<Type> Providers = new List<Type>();

        public static IZenProvider Service<T>(this T source) where T : Type => (IZenProvider)Instances.ServiceProvider.GetService(source);
    }
}