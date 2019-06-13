using System;
using System.Diagnostics;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Base.Module.Default {
    public class NullLogProvider : LogProvider
    {
        public new void Dispatch(Message payload)
        {
            Debug.Write("[{0}] {1}".format(payload.Type.ToString(), payload.Content));
            Console.WriteLine("[{0}] {1}".format(payload.Type.ToString(), payload.Content));
        }
    }
}