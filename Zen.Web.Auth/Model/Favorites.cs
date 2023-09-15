using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Web.Auth.Model
{
    public class Favorites : Data<Favorites>, IDataId
    {
        [Key]
        public string Id { get; set; }
        [Display]
        public string Alias { get; set; }
        public string Locator { get; set; }
        public string Scope { get; set; }
        public string User { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
