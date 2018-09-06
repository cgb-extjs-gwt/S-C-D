using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Ninject;
using Ninject.Modules;
using Owin;
using Web.Api.DI;

[assembly: OwinStartup(typeof(Web.Api.Startup))]
namespace Web.Api
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

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
        }

        public void Configuration(IAppBuilder app)
        {
            //NinjectModule registrations = new NinjectRegistrations();
            //var kernel = new StandardKernel(registrations);

            //var ninjectResolver = new NinjectScdDependencyResolver(kernel);

            //DependencyResolver.SetResolver(ninjectResolver); // MVC
            //GlobalConfiguration.Configuration.DependencyResolver = ninjectResolver; // Web API
        }

        public void Configure(IAppBuilder app, HostingEnvironment env, IServiceProvider serviceProvider)
        {

        }
    }
}