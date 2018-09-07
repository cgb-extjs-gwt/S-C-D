using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using Gdc.Scd.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Mvc;
using Owin;
using Web.Api;

[assembly: OwinStartupAttribute(typeof(Gdc.Scd.Web.Startup))]
namespace Gdc.Scd.Web
{
    public class Startup
    {
        private readonly HostingEnvironment env;

        public Startup()
        {
        }

        public Startup(HostingEnvironment env)
        {
            this.env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            this.InitModules(services);
            var types = typeof(Startup).Assembly.GetExportedTypes();
            List<Type> controllerTypes=new List<Type>();
            foreach (var t in types)
            {
                if(!t.IsAbstract && !t.IsGenericTypeDefinition && (typeof(IController).IsAssignableFrom(t)
                || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)))
                {
                    controllerTypes.Add(t);
                }
            }
            services.AddControllersAsServices(controllerTypes);
        }

        public void Configuration(IAppBuilder app)
        {

            var services = new ServiceCollection();
            ConfigureServices(services);
            var resolver = new DefaultDependencyResolver(services.BuildServiceProvider());
            DependencyResolver.SetResolver(resolver);
            GlobalConfiguration.Configuration.DependencyResolver = new DefaultDependencyResolver(services.BuildServiceProvider());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IAppBuilder app, HostingEnvironment env, IServiceProvider serviceProvider)
        {
#if DEBUG
            var parentDirectoryInfo = Directory.GetParent(Directory.GetCurrentDirectory());

            //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
            //{
            //    ConfigFile = "webpack.config.asp.js",
            //    ProjectPath = Path.Combine(parentDirectoryInfo.FullName, "Gdc.Scd.Web.Client"),
            //});
#endif
            foreach (var handler in serviceProvider.GetServices<IConfigureApplicationHandler>())
            {
                handler.Handle();
            }
        }

        private void InitModules(IServiceCollection services)
        {
            this.InitModule<Scd.Core.Module>(services);
            this.InitModule<Scd.DataAccessLayer.Module>(services);
            this.InitModule<Scd.BusinessLogicLayer.Module>(services);
#if DEBUG

                this.InitModule<Gdc.Scd.DataAccessLayer.TestData.Module>(services);
#endif
        }

        private void InitModule<T>(IServiceCollection services) where T : IModule, new()
        {
            var module = new T();

            module.Init(services);
        }
    }

    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddControllersAsServices(this IServiceCollection services,
           IEnumerable<Type> controllerTypes)
        {
            foreach (var type in controllerTypes)
            {
                services.AddTransient(type);
            }

            return services;
        }
    }

  

}
