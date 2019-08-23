using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;

namespace Zen.Base.Distributed {
    public class TicketLog : Data<TicketLog>
    {
        [Key]
        public string Id { get; set; }
        [Display]
        public string ServiceDescriptor { get; set; }
        public string TicketId { get; set; }

        public string InstanceId { get; set; }
        public string State { get; set; }
        public string Comments { get; set; }
        public DateTime TimeOut { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}