using System;

namespace Zen.App.Data.Log
{
    public class Header : IHeader
    {
        public string Action { get; set; }
        public string Type { get; set; }
        public string AuthorLocator { get; set; }
        public string ReferenceId { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
    }

}
