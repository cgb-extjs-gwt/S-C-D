using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gdc.Scd.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                var parentDirectoryInfo = Directory.GetParent(Directory.GetCurrentDirectory());

                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    ConfigFile = "webpack.config.asp.js",
                    ProjectPath = Path.Combine(parentDirectoryInfo.FullName, "Gdc.Scd.Web.Client")
                });
            }

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "DefaultApi", template: "api/{controller}/{action}");
                routes.MapSpaFallbackRoute("spa-fallback", new { controller = "Home", action = "Index" }); // 2
            });
        }
    }
}
