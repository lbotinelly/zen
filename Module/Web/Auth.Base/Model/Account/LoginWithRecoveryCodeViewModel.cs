using System.ComponentModel.DataAnnotations;

namespace Zen.Module.Web.Auth.Base.Model.Account
{
    public class LoginWithRecoveryCodeViewModel
    {
        [Required, DataType(DataType.Text), Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; }
    }
}