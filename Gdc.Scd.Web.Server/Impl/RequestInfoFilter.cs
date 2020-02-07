using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Web.Server.Entities;
using Ninject;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Gdc.Scd.Web.Server.Impl
{
    public class RequestInfoFilter : System.Web.Mvc.IActionFilter, System.Web.Mvc.IExceptionFilter, IActionFilter, IExceptionFilter
    {
        private const string RequestInfoContextKey = "RequestInfo";

        private readonly IPrincipalProvider principalProvider;

        private readonly IKernel ioc;

        bool IFilter.AllowMultiple => false;

        public RequestInfoFilter(IPrincipalProvider principalProvider, IKernel ioc)
        {
            this.principalProvider = principalProvider;
            this.ioc = ioc;
        }

        void System.Web.Mvc.IActionFilter.OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            try
            {
                var request = filterContext.HttpContext.Request;
                var requestInfo = this.BuildRequestInfo(request.RequestType, request.Url);

                filterContext.HttpContext.Items.Add(RequestInfoContextKey, requestInfo);
            }
            catch
            { 
            }
        }

        void System.Web.Mvc.IActionFilter.OnActionExecuted(System.Web.Mvc.ActionExecutedContext filterContext)
        {
            try
            {
                var requestInfo = this.GetRequestInfo(filterContext);

                this.SaveRequestInfo(requestInfo);
            }
            catch
            { 
            }
        }

        void System.Web.Mvc.IExceptionFilter.OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            try
            {
                var requestInfo = this.GetRequestInfo(filterContext);

                this.SaveRequestInfo(requestInfo, filterContext.Exception);
            }
            catch
            {
            }
        }

        async Task<HttpResponseMessage> IActionFilter.ExecuteActionFilterAsync(
            HttpActionContext actionContext, 
            CancellationToken cancellationToken, 
            Func<Task<HttpResponseMessage>> continuation)
        {
            RequestInfo requestInfo = null;

            try
            {
                var request = actionContext.Request;
                
                requestInfo = this.BuildRequestInfo(request.Method.Method, request.RequestUri);

                actionContext.Request.Properties[RequestInfoContextKey] = requestInfo;
            }
            catch
            {
            }

            var result = await continuation();

            try
            {
                this.SaveRequestInfo(requestInfo);
            }
            catch
            {
            }

            return result;
        }

        async Task IExceptionFilter.ExecuteExceptionFilterAsync(
            HttpActionExecutedContext actionExecutedContext, 
            CancellationToken cancellationToken)
        {
            try
            {
                var requestInfo = (RequestInfo)actionExecutedContext.ActionContext.Request.Properties[RequestInfoContextKey];

                this.SaveRequestInfo(requestInfo, actionExecutedContext.Exception);
            }
            catch
            {
            }

            await Task.CompletedTask;
        }

        private RequestInfo GetRequestInfo(System.Web.Mvc.ControllerContext context)
        {
            return (RequestInfo)context.HttpContext.Items[RequestInfoContextKey];
        }

        private void SaveRequestInfo(RequestInfo requestInfo, Exception exception)
        {
            exception = GetLastLevelException(exception);

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

        private void SaveRequestInfo(RequestInfo requestInfo)
        {
            requestInfo.Duration = (long)(DateTime.UtcNow - requestInfo.DateTime).TotalMilliseconds;

            this.ioc.Get<IDomainService<RequestInfo>>().Save(requestInfo);
        }

        private RequestInfo BuildRequestInfo(string requestType, Uri uri)
        {
            var principal = this.principalProvider.GetCurrenctPrincipal();

            return new RequestInfo
            {
                DateTime = DateTime.UtcNow,
                RequestType = requestType,
                Host = uri.Authority,
                QueryPath = uri.AbsolutePath,
                QueryParams = string.IsNullOrWhiteSpace(uri.Query) ? null : uri.Query,
                UserLogin = principal.Identity.Name
            };
        }
    }
}