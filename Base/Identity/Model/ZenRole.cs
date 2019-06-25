using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;

namespace Zen.Base.Identity.Model
{
    public class ZenRole : Data<ZenRole>
    {
        public ZenRole() { }

        public ZenRole(string name)
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