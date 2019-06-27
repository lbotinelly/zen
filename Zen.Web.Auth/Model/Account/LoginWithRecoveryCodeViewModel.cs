using System.ComponentModel.DataAnnotations;

namespace Zen.Web.Auth.Model.Account
{
    public class LoginWithRecoveryCodeViewModel
    {
        [Required, DataType(DataType.Text), Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; }
    }
}