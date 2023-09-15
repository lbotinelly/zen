using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Web.Model
{
    public class RequestLog : Data<RequestLog>, IDataId
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Display]
        public string Url { get; set; }

        public string Referer { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
