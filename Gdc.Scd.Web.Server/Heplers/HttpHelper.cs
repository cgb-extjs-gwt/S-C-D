using System.Web;

namespace Gdc.Scd.Web.Server
{
    public static class HttpHelper
    {
        public static string Username(this HttpContext ctx)
        {
            return ctx.User.Identity.Name;
        }

        public static void SendNow(this HttpResponse resp, object value)
        {
            resp.Write(value.AsJson());
            resp.Flush();
        }
    }
}