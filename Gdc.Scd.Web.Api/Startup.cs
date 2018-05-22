using System.IO;
using Gdc.Scd.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            this.InitModules(services);
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
                    ProjectPath = Path.Combine(parentDirectoryInfo.FullName, "Gdc.Scd.Web.Client"),
                });
            }

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "DefaultApi", template: "api/{controller}/{action}");
                routes.MapSpaFallbackRoute("spa-fallback", new { controller = "Home", action = "Index" }); // 2
            });
        }

        private void InitModules(IServiceCollection services)
        {
            this.InitModule<Scd.DataAccessLayer.Module>(services);
            this.InitModule<Scd.BusinessLogicLayer.Module>(services);
        }

        private void InitModule<T>(IServiceCollection services) where T : IModule, new()
        {
            var module = new T();

            module.Init(services);
        }
    }
}
