using System;
using System.Threading;

namespace Zen.Base.Process
{
    public static class ThreadContext
    {
        public static void Set(string field, object value) { Thread.SetData(Thread.GetNamedDataSlot(field), value); }

        public static T Get<T>(string field)
        {
            try
            {
                var ret = (T)Thread.GetData(Thread.GetNamedDataSlot(field));
                return ret;
            }
            catch (Exception) { return default(T); }
        }
    }

    public static class EnvironmentVariables
    {
        public static string Get(string key)
        {
            var content =

                System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process) ??
                System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User) ??
                System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);

            return content;
        }
    }
}