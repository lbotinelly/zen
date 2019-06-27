using System.ComponentModel.DataAnnotations;

namespace Zen.Web.Auth.Model.Account
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}