using System.ComponentModel.DataAnnotations;

namespace Zen.Web.Auth.Model.Account
{
    public class ExternalLoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}