using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Zen.Base;
using Zen.Web.Auth;

namespace Zen.Provider.GitHub.Authentication
{
    public static class Settings
    {
        internal static IServiceCollection Configure(this IServiceCollection services)
        {
            var probe = Configuration.Options["Authentication:GitHub:ClientId"];

            Instances.AuthenticationBuilder.AddGitHub("Github", options =>
             {
                 options.ClientId = Configuration.Options["Authentication:GitHub:ClientId"];
                 options.ClientSecret = Configuration.Options["Authentication:GitHub:ClientSecret"];

                 options.Scope.Add("user:email");

                 options.CallbackPath = "/auth/signin/github";

                 options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                 options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                 options.UserInformationEndpoint = "https://api.github.com/user";

                 options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                 options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                 options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                 options.ClaimActions.MapJsonKey("urn:github:login", "login");
                 options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                 options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
                 options.ClaimActions.MapJsonKey("urn:github:blog", "blog");

                 options.Events = new OAuthEvents
                 {
                     OnCreatingTicket = async context =>
                     {

                         var fullName = context.Identity.FindFirst("urn:github:name").Value;
                         var email = context.Identity.FindFirst(ClaimTypes.Email).Value;


                         var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                         request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                         request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                         var response = await context.Backchannel.SendAsync(request,
                             HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                         response.EnsureSuccessStatusCode();

                         var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                         context.RunClaimActions(user);
                     }
                 };
             });


            return services;
        }
    }
}