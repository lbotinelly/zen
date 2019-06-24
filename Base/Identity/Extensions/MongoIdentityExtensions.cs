using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base.Identity.Collections;
using Zen.Base.Identity.Model;
using Zen.Base.Identity.Store;

namespace Zen.Base.Identity.Extensions
{
    public static class MongoIdentityExtensions
    {
        public static IdentityBuilder AddZenIdentityProvider<TUser>(this IServiceCollection services)
            where TUser : ZenUser
        {
            return AddZenIdentityProvider<TUser, ZenRole>(services, x => { });
        }

        public static IdentityBuilder AddZenIdentityProvider<TUser>(this IServiceCollection services, Action<ZenIdentityOptions> setupDatabaseAction)
            where TUser : ZenUser
        {
            return AddZenIdentityProvider<TUser, ZenRole>(services, setupDatabaseAction);
        }

        public static IdentityBuilder AddZenIdentityProvider<TUser, TRole>(this IServiceCollection services, Action<ZenIdentityOptions> setupDatabaseAction)
            where TUser : ZenUser
            where TRole : ZenRole
        { return AddZenIdentityProvider<TUser, TRole>(services, x => { }, setupDatabaseAction); }

        public static IdentityBuilder AddZenIdentityProvider(this IServiceCollection services, Action<IdentityOptions> setupIdentityAction, Action<ZenIdentityOptions> setupDatabaseAction)
        {
            return AddZenIdentityProvider<ZenUser, ZenRole>(services, setupIdentityAction, setupDatabaseAction);
        }

        public static IdentityBuilder AddZenIdentityProvider<TUser>(this IServiceCollection services, Action<IdentityOptions> setupIdentityAction, Action<ZenIdentityOptions> setupDatabaseAction)
            where TUser : ZenUser
        {
            return AddZenIdentityProvider<TUser, ZenRole>(services, setupIdentityAction, setupDatabaseAction);
        }

        public static IdentityBuilder AddZenIdentityProvider<TUser, TRole>(this IServiceCollection services, Action<IdentityOptions> setupIdentityAction, Action<ZenIdentityOptions> setupDatabaseAction) where TUser : ZenUser
            where TRole : ZenRole
        {
            var dbOptions = new ZenIdentityOptions();
            setupDatabaseAction(dbOptions);

            var builder = services.AddIdentity<TUser, TRole>(setupIdentityAction ?? (x => { }));

            builder.AddRoleStore<RoleStore<TRole>>()
                .AddUserStore<UserStore<TUser, TRole>>()
                .AddUserManager<UserManager<TUser>>()
                .AddRoleManager<RoleManager<TRole>>()
                .AddDefaultTokenProviders();

            var userCollection = new IdentityUserCollection<TUser>(dbOptions.ConnectionString, dbOptions.UsersCollection);
            var roleCollection = new IdentityRoleCollection<TRole>(dbOptions.ConnectionString, dbOptions.RolesCollection);

            services.AddTransient<IIdentityUserCollection<TUser>>(x => userCollection);
            services.AddTransient<IIdentityRoleCollection<TRole>>(x => roleCollection);

            // Identity Services
            services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser, TRole>(userCollection, roleCollection, x.GetService<ILookupNormalizer>()));
            services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole>(roleCollection));

            return builder;
        }
    }
}