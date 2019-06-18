using System.ComponentModel.DataAnnotations;
using Bogus.DataSets;
using Zen.Base.Module;

namespace SimpleWebApp.Models
{
    public class SampleModel : Data<SampleModel>
    {
        [Key]
        public string Id { get; set; }
        public Name.Gender Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Display]
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}