using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;

namespace SimpleWebApp.Models
{
    public class SampleModel : Data<SampleModel>
    {
        [Key]
        public string Id { get; set; }
        [Display]
        public string Name { get; set; }
    }
}