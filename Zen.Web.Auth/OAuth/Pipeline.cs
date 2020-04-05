using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AspNet.Security.OpenId;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Zen.Base.Extension;
using Zen.Web.Auth.Model;

namespace Zen.Web.Auth.OAuth
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

                var payload = await context.GetIdentityPayload();
                var userData = JsonDocument.Parse(payload).RootElement;
                context.RunClaimActions(userData);

                ClaimsIdentity ci = context.Identity;

                var provider = ci.AuthenticationType;
                var id = ci.Claim(ClaimTypes.NameIdentifier);
                var firstName = ci.Claim(ClaimTypes.GivenName);
                var lastName = ci.Claim(ClaimTypes.Surname);
                var email = ci.Claim(ClaimTypes.Email);
                var webpage = ci.Claim(ClaimTypes.Webpage);

                var modelKey = $"{provider}-{id}".StringToGuid().ToString();

                var model = ProviderIdentityUser.Get(modelKey) ?? new ProviderIdentityUser { Id = modelKey };

                model.ProviderName = provider;
                model.ProviderKey = id;
                model.Claims = context.Identity.Claims.ToList()
                    .GroupBy(i => i.Type)
                    .ToDictionary(j => j.Key, j => j.Select(k => k.Value).Distinct().Last());

                model.SyncWithLocalStore();

                model.IdentityUser = ci.ToIdentityUser(model);

                model.Save();
                
                await Task.FromResult(true);
            }
        };

        public static OpenIdAuthenticationEvents OpenIdEventHandler = new OpenIdAuthenticationEvents()
        {

            OnAuthenticated = async context =>
            {

            }
        };
    }
}