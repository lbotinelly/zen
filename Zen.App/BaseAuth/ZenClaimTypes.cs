namespace Zen.App.BaseAuth
{
    public static class ZenClaimTypes
    {
        internal const string ClaimTypeNamespace = "http://schemas.zen.code/ws/2020/01/identity/claims";

        public const string ProfilePicture = ClaimTypeNamespace + "/profilepicture";
        public static string Locale = ClaimTypeNamespace + "/locale";
        public static string EmailConfirmed = ClaimTypeNamespace + "/emailconfirmed";
        public static string Stamp = ClaimTypeNamespace + "/stamp";
    }
}