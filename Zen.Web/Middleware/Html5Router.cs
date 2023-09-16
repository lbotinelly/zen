using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Core;
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
                var path = context.Request.Path.ToString().ToLower();
                var physicalPath = Path.Combine("wwwroot" + path);

                await LogRequest(context);

                if (File.Exists(physicalPath) ||
                path.StartsWith("/api") ||
                path.EndsWith(".map")
                )
                {
                    await next.Invoke();
                    return;
                }

                if (context.Request.Headers.ContainsKey("user-agent"))
                {
                    var sig = _botSignatures.FirstOrDefault(i => context.Request.Headers["user-agent"].ToString().Contains(i));

                    if (sig != null)
                    {
                        var result = cardRender.GetCardDetails(context.Request).Render(context.Request);
                        Base.Log.KeyValuePair("Card Generator", sig + "::" + path);
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(result);
                        return;
                    }
                }

                context.Request.Path = "/";

                await next.Invoke();
            });
        }

        private static async Task LogRequest(HttpContext context)
        {
            var url = context.Request.GetDisplayUrl();

            if (url.Contains(".js")) return;
            if (url.Contains(".css")) return;
            if (url.Contains(".html")) return;

            try
            {
                new RequestLog() { Referer = context.Request.Headers["referer"].ToString(), Url = context.Request.GetDisplayUrl(), }.Save();
            }
            catch (Exception) { }

        }
    }
}
