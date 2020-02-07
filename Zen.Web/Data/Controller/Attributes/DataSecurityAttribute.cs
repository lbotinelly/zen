using System;

namespace Zen.Web.Data.Controller.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataSecurityAttribute : Attribute
    {
        public string ReadPermission { get; set; }
        public string WritePermission { get; set; }
        public string RemovePermission { get; set; }
    }
}