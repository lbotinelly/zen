using System;
using System.ComponentModel.DataAnnotations;
using Zen.Base.Module;

namespace Zen.Module.Web.Auth.Zen
{
    public class ZenUser : Data<ZenUser>
    {
        public ZenUser() { }
        public ZenUser(string userName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }
        [Key]
        public string Id { get; internal set; }
        [Display]
        public string UserName { get; internal set; }
        public string NormalizedUserName { get; internal set; }
    }
}
