using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;
using Zen.Base.Module.Data.CommonAttributes;

namespace Zen.Web.App.Model.Metrics
{
    public class Datapoint : Data<Datapoint>, IDataId
    {
        [Key]
        public string Id { get; set; }
        [Display]
        public string Locator { get; set; }
    }
}
