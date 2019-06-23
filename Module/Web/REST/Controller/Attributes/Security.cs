using System;

namespace Zen.Module.Web.REST.Controller.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Security : System.Attribute
    {
        public string Read { get; set; }
        public string Write { get; set; }
        public string Remove { get; set; }
    }
}