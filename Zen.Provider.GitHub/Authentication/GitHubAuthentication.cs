﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Zen.Provider.GitHub.Authentication
{
    public static class GitHubAuthentication
    {
        internal static IServiceCollection AddAuthenticationProvider(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "GitHub";
            })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Unauthorized/";
                    options.AccessDeniedPath = "/Account/Forbidden/";
                })
                .AddJwtBearer(options =>
                {
                    options.Audience = "http://localhost:5001/";
                    options.Authority = "http://localhost:5000/";
                })
                .AddOAuth("GitHub", options =>
                {
                    //options.ClientId = Configuration["GitHub:ClientId"];
                    //options.ClientSecret = Configuration["GitHub:ClientSecret"];
                    options.CallbackPath = new PathString("/signin-github");

                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey("urn:github:login", "login");
                    options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                    options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request =
                                new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var response = await context.Backchannel.SendAsync(request,
                                HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                            context.RunClaimActions(user);
                        }
                    };
                })
                .AddOAuth("GitHub", options =>
                {
                    options.ClientId =
                        "****************"; //https://github.com/settings/developers üzerinden aldığınız ClientID
                    options.ClientSecret =
                        "************"; //https://github.com/settings/developers üzerinden aldığınız ClientSecret
                    options.CallbackPath = new PathString("/signin-github");

                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey("urn:github:blog", "blog");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async ctx =>
                        {
                            // Console.WriteLine("OnCreatingTicket Event");

                            var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

                            var response = await ctx.Backchannel.SendAsync(request,
                                HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var userInfo = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                            ctx.RunClaimActions(userInfo);
                            // Console.WriteLine($"User Info:\n{userInfo.ToString()}");
                        }
                    };
                });


            return services;
        }
    }
}