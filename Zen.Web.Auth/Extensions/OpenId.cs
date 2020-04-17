using System.Collections.Generic;
using System.Security.Claims;
using AspNet.Security.OpenId;
using Newtonsoft.Json.Linq;

namespace Zen.Web.Auth.Extensions
{
    public static class OpenId
    {
        internal static Dictionary<OpenIdAuthenticationOptions, List<Claim>> ClaimMap = new Dictionary<OpenIdAuthenticationOptions, List<Claim>>();

        public static void ClearClaimMap(this OpenIdAuthenticationOptions source)
        {
            if (source == null) return;
            ClaimMap[source] = new List<Claim>();
        }

        public static void MapClaimToJsonKey(this OpenIdAuthenticationOptions source, string claimType, string jsonKey, string valueType = null, string issuer = null)
        {
            if (source == null) return;
            if (!ClaimMap.ContainsKey(source)) ClaimMap[source] = new List<Claim>();
            ClaimMap[source].Add(new Claim(claimType, jsonKey, valueType, issuer));
        }

        public static void RunClaimActions(this OpenIdAuthenticatedContext source, JObject model = null)
        {
            if (!ClaimMap.ContainsKey(source.Options)) return;

            if (model == null) model = source.User;

            foreach (var claim in ClaimMap[source.Options])
            {
                var targetValue = model.SelectToken(claim.Value).ToString();
                if (targetValue != null) source.Identity.AddClaim(new Claim(claim.Type, targetValue, claim.ValueType,claim.Issuer));
            }
        }
    }
}