using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Gdc.Scd.Web.Server
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static bool _firstRequest = true;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_EndRequest()
        {
            if (_firstRequest)
            {
                var handlers = DependencyResolver.Current.GetServices<IConfigureApplicationHandler>();
                foreach (var handler in handlers)
                {
                    handler.Handle();
                }
                _firstRequest = false;
            }
        }
    }
}
