using Gdc.Scd.Web.Server.Entities;
using Gdc.Scd.Web.Server.Impl;
using System;
using System.Configuration;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Server
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection mvcFilters, HttpFilterCollection apiFilters)
        {
            var requestLoggingStr = ConfigurationManager.AppSettings[nameof(RequestLogging)];
            if (requestLoggingStr != null)
            {
                var requestLogging = (RequestLogging)Enum.Parse(typeof(RequestLogging), requestLoggingStr, true);
                if (requestLogging == RequestLogging.Enable)
                {
                    apiFilters.Add(DependencyResolver.Current.GetService<RequestInfoFilter>());
                }
            }
            
            mvcFilters.Add(new HandleErrorAttribute());
        }
    }
}
