using System.ComponentModel.DataAnnotations;

namespace Zen.Base.Module.Identity.Model
{
    public class Role : Data<Role>
    {
        public Role() { }

        public Role(string name)
        {
            Name = name;
            NormalizedName = name.ToUpperInvariant();
        }

        [Key]
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        [Display]
        public virtual string NormalizedName { get; set; }
        public virtual string ConcurrencyStamp { get; set; }

        public override string ToString() { return Name; }
    }
}