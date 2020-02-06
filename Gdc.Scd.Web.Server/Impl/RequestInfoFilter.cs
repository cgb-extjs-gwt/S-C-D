using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Web.Server.Entities;
using System;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Server.Impl
{
    public class RequestInfoFilter : IActionFilter, IExceptionFilter
    {
        private const string RequestInfoContextKey = "RequestInfo";

        private readonly IPrincipalProvider principalProvider;

        private readonly IDomainService<RequestInfo> requestInfoService;

        public RequestInfoFilter(IPrincipalProvider principalProvider, IDomainService<RequestInfo> requestInfoService)
        {
            this.principalProvider = principalProvider;
            this.requestInfoService = requestInfoService;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var principal = this.principalProvider.GetCurrenctPrincipal();
            var requestInfo = new RequestInfo
            {
                DateTime = DateTime.UtcNow,
                RequestType = request.RequestType,
                Host = request.Url.Authority,
                Url = request.RawUrl,
                UserLogin = principal.Identity.Name
            };

            filterContext.HttpContext.Items.Add(RequestInfoContextKey, requestInfo);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var requestInfo = this.GetRequestInfo(filterContext);

            requestInfo.Duration = (long)(DateTime.UtcNow - requestInfo.DateTime).TotalMilliseconds;

            this.SaveRequestInfo(requestInfo);
        }

        public void OnException(ExceptionContext filterContext)
        {
            var requestInfo = this.GetRequestInfo(filterContext);
            var exception = GetLastLevelException(filterContext.Exception);

            requestInfo.Error = string.Concat(
                exception.Message,
                Environment.NewLine,
                Environment.NewLine,
                exception.StackTrace);

            this.SaveRequestInfo(requestInfo);

            Exception GetLastLevelException(Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                return ex;
            }
        }

        private RequestInfo GetRequestInfo(ControllerContext context)
        {
            return (RequestInfo)context.HttpContext.Items[RequestInfoContextKey];
        }

        private void SaveRequestInfo(RequestInfo requestInfo)
        {
            this.requestInfoService.Save(requestInfo);
        }
    }
}