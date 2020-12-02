using System;
using System.Collections.Generic;
using Zen.Web.Configuration;

namespace Zen.Web
{
    public static class Instances
    {
        public static List<Action> BeforeUseEndpoints = new List<Action>();
        public static Options Options { get; set; }
    }
}