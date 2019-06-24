namespace Zen.Base.Identity.Model {
    public class TwoFactorRecoveryCode
    {
        public string Code { get; set; }
        public bool Redeemed { get; set; }
    }
}