using System;
using System.Threading;
using Zen.Base.Extension;

namespace Zen.Base.Diagnostics
{
    public class ThreadHelper
    {
        private static readonly ThreadLocal<string> _Uid = new ThreadLocal<string>(() => Guid.NewGuid().ToShortGuid().ToString());
        public static string Uid => _Uid.Value;
    }
}