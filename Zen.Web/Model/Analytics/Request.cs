using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Web.Model.Analytics
{
    internal class Request : Data<Request>, IDataId
    {
        public enum EType
        {
            Regular,
            Html5Redirect
        }


        [Key]
        public string Id { get; set; }
        [Display]
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Path { get; set; }
        public EType Type { get; set; } = EType.Regular;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
