using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Common;
using Zen.Base.Extension;

namespace Zen.Base.Module
{
    public class Audit : Data<Audit>
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Event { get; set; }
        public string Type { get; set; }
        public DataReference Subject { get; set; }
        public DataReference Target { get; set; }
        [Display]
        public string Description { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public object Payload { get; set; }

        #region Overrides of Data<Audit>

        public override void AfterInsert(string newKey)
        {
            Current.Log.Info<Audit>(this.ToJson());
        }

        #endregion
    }
}