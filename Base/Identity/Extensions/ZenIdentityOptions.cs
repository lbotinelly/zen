namespace Zen.Base.Identity.Extensions {
    public class ZenIdentityOptions
    {
        public string ConnectionString { get; set; } = "mongodb://localhost/default";

        public string UsersCollection { get; set; } = "Users";

        public string RolesCollection { get; set; } = "Roles";

        public bool UseDefaultIdentity { get; set; } = true;
    }
}