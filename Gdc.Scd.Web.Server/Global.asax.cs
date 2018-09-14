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

#if DEBUG
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
#endif
        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    Exception ex = Server.GetLastError();
        //    string to = "nikita.zotov.gdc@ts.fujitsu.com";
        //    string from = "nikita.zotov.gdc@ts.fujitsu.com";
        //    MailMessage message = new MailMessage(from, to);
        //    message.Subject = "Using the new SMTP client.";
        //    message.Body = ex.ToString();
        //    SmtpClient client = new SmtpClient("mail.fsc.net");
        //    // Credentials are necessary if the server requires the client 
        //    // to authenticate before it will send e-mail on the client's behalf.
        //    client.Send(message);          
        //}
    }
}
