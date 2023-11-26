using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zen.Base.Extension;
using Zen.Base.Module.Service;
using Zen.Web.Common;
using Zen.Web.Model;

namespace Zen.Web.Middleware
{
    public static class Html5Router
    {
        private static readonly IZenWebCardRender cardRender = IoC.GetClassesByInterface<IZenWebCardRender>(false).FirstOrDefault()?.CreateInstance<IZenWebCardRender>();

        private static readonly List<string> _botSignatures = new() { "Discordbot/", "Twitterbot/", "Needle/", "node-fetch/", "Tumblr/", "OpenGraphNet/" };

        private static string Render(this ZenWebCardDetails source, HttpRequest request)
        {

            Uri baseUri = new(request.GetEncodedUrl());

            Uri imageUri = new(baseUri, source.Image);
            Uri UrlUri = new(baseUri, source.Url);

            var template = @$"<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""twitter:card"" content=""summary"" />
    <meta name=""twitter:title"" content=""{source.Title}"" />
    <meta name=""twitter:description"" content=""{source.Description}"" />
    <meta name=""twitter:image"" content=""{imageUri}"" />
    <meta name=""twitter:site"" content=""{source.TwitterSiteUser}"" />
    <meta name=""twitter:creator"" content=""{source.TwitterCreatorUser}"" />
    <meta property=""og:url"" content=""{UrlUri}"" />
    <meta property=""og:type"" content=""website"" />
    <meta property=""og:title"" content=""{source.Title}"" />
    <meta property=""og:description"" content=""{source.Description}"" />
    <meta property=""og:image"" content=""{imageUri}"" />
</head>
<body>
<body>
</html>"
;

            return template;
        }

        public static void UseHtml5Routing(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.ToString();
                var lowerPath = path.ToLower();

                if (lowerPath.StartsWith("/api") || lowerPath.EndsWith(".map")) { await next.Invoke(); return; }

                var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                pathParts.Insert(0, "wwwroot");

                var physicalPath = Path.Combine(pathParts.ToArray());

                //Base.Log.Add(context.Request.Path + context.Request.QueryString, Base.Module.Log.Message.EContentType.Debug);
                //Base.Log.Add(physicalPath, Base.Module.Log.Message.EContentType.Debug);

                await LogAnalytics(context);

                if (File.Exists(physicalPath)) { await next.Invoke(); return; }

                if (context.Request.Headers.ContainsKey("user-agent"))
                {
                    var sig = _botSignatures.FirstOrDefault(i => context.Request.Headers["user-agent"].ToString().Contains(i));

                    if (sig != null)
                    {
                        var result = cardRender.GetCardDetails(context.Request).Render(context.Request);
                        Base.Log.KeyValuePair("Card Generator", $"{sig}::{path}");
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(result);
                        return;
                    }
                }

                context.Request.Path = "/";

                await next.Invoke();
            });
        }

        private static async Task LogAnalytics(HttpContext context)
        {
            var url = context.Request.GetDisplayUrl();

            if (url.Contains(".js")) return;
            if (url.Contains(".css")) return;
            if (url.Contains(".html")) return;

            try
            {
                new Model.Analytics.Request()
                {
                    Url = context.Request.GetDisplayUrl(),
                    Headers = context.Request.Headers.ToDictionary(i => i.Key.ToString(), i => i.Value.ToString()),
                    Path = context.Request.Path + context.Request.QueryString,
                    Type = Model.Analytics.Request.EType.Html5Redirect
                }.Save();
            }
            catch (Exception) { }

        }
    }
}
