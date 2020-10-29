using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenId;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Zen.App.BaseAuth;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;

namespace Zen.Web.Auth.Extensions
{
    public static class Pipeline
    {
        public static OAuthEvents OAuthEventHandler = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                // Persist additional claims and tokens from external providers
                var tokens = context.Properties.GetTokens().ToList();

                tokens.Add(new AuthenticationToken
                {
                    Name = "TicketCreated",
                    Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                });

                context.Properties.StoreTokens(tokens);

                Base.Current.Log.Add(context.User.ToString());

                context.RunClaimActions(context.User);
                context.Identity.Stamp();

                context.Identity.ToLocalModel();

                await Task.FromResult(true);
            }
        };

        public static OpenIdAuthenticationEvents OpenIdEventHandler = new OpenIdAuthenticationEvents
        {
            OnAuthenticated = async context =>
            {
                Base.Current.Log.Add(context.User.ToString());

                context.RunClaimActions();
                context.Identity.Stamp();

                context.Identity.ToLocalModel();

                await Task.FromResult(true);
            }
        };

        public static string StampValue(this ClaimsIdentity source) => StampValue(source.AuthenticationType, source.Claim(ClaimTypes.NameIdentifier));

        public static string StampValue(this UserLoginInfo source) => StampValue(source.ProviderDisplayName, source.ProviderKey);

        public static string StampValue(string provider, string key)
        {
            var rawKey = $"{provider}::{key}";
            var stamp = rawKey.ToGuid().ToString();

            Log.KeyValuePair(stamp, rawKey, Message.EContentType.StartupSequence);

            return stamp;
        }

        public static void Stamp(this ClaimsIdentity source)
        {
            source.AddClaim(new Claim(ZenClaimTypes.Stamp, source.StampValue()));
        }

        public static Model.Identity ToLocalModel(this ClaimsIdentity ci)
        {
            var gid = ci.Claim(ZenClaimTypes.Stamp);

            var model = Model.Identity.Get(gid) ?? new Model.Identity {Id = gid};

            model.ProviderName = ci.AuthenticationType;
            model.ProviderKey = ci.Claim(ClaimTypes.NameIdentifier);

            model.Claims =
                ci.Claims
                    .ToList()
                    .GroupBy(i => i.Type)
                    .ToDictionary(j => j.Key, j => j.Select(k => k.Value).Distinct().Last());

            foreach (var (key, value) in model.Claims) Base.Current.Log.KeyValuePair(key, value);

            model.SyncWithLocalStore();

            model.IdentityUser = Shared.ToIdentityUser(ci, model);

            model.Save();

            return model.Save();
        }
    }
}