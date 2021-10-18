using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Zen.Web.Auth.Model;

namespace Zen.Web.Auth.Extensions
{
    public static class Shared
    {
        public static IdentityUser ToIdentityUser(this ClaimsIdentity source, Model.Identity user)
        {
            var model = new IdentityUser
            {
                Id = source.Claim(ZenClaimTypes.Stamp),
                Email = source.Claim(ClaimTypes.Email),
                UserName = source.Claim(ClaimTypes.GivenName),
                EmailConfirmed = source.Claim(ZenClaimTypes.EmailConfirmed) == "true"
            };

            model.NormalizedEmail = model.Email?.ToUpperInvariant();
            model.NormalizedUserName = model.UserName?.ToUpperInvariant();
            return model;
        }
    }
}