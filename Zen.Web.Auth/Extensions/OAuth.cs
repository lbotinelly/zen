using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Zen.App.BaseAuth;

namespace Zen.Web.Auth.Extensions
{
    public static class OAuth
    {
        public static async Task<string> GetIdentityPayload(this OAuthCreatingTicketContext context)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadAsStringAsync();

            return payload;
        }

        public static string Claim(this ClaimsIdentity source, string key) => source.FindFirst(key)?.Value;

        public static bool SyncWithLocalStore(this Model.Identity model)
        {
            var localModel = App.Current.Orchestrator.GetPersonByClaims(model.Claims);
            model.InternalId = localModel.Id;

            return true;
        }

        public static IdentityUser ToIdentityUser(this ClaimsIdentity source, Model.Identity user)
        {
            var model = new IdentityUser {Id = user.InternalId, Email = source.Claim(ClaimTypes.Email)};
            model.NormalizedEmail = model.Email.ToUpperInvariant();
            model.UserName = source.Claim(ClaimTypes.GivenName);
            model.NormalizedUserName = model.UserName.ToUpperInvariant();
            model.EmailConfirmed = source.Claim(ZenClaimTypes.EmailConfirmed) == "true";
            return model;
        }
    }
}