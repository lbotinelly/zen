using System.ComponentModel.DataAnnotations;

namespace Zen.Module.Web.Auth.Base.Model.Account
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}