using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sample02_REST.Model;
using Zen.Base.Service.Extensions;

namespace Sample02_REST
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddZen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseZen();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/Sample02-REST/", async context =>
                {
                    var rc = Person.Count();

                    if (rc == 0)
                        for (var i = 1; i < 10; i++)
                        {
                            var r = Person.Random();
                            r.Id = i.ToString();
                            r.Save();
                        }

                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}