using System.ComponentModel.DataAnnotations;
using Bogus.DataSets;
using Zen.Base.Module;

namespace SimpleWebApp.Models
{
#pragma warning disable IDE1006 // Naming Styles: POCOs are allowed to use camelCase naming.
    public class sampleModel : Data<sampleModel>
    // ReSharper restore InconsistentNaming
    {
        [Key] public string id { get; set;}
        [Display] public string userName;
        public string name;
        public Name.Gender gender;
        public string firstName;
        public string lastName;
        public string email;
    }
    // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles
}