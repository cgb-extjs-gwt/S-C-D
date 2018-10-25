using System.Web;

namespace Gdc.Scd.Web.Server
{
    public static class HttpContextHelper
    {
        public static string Username(this HttpContext ctx)
        {
            return ctx.User.Identity.Name;
        }
    }
}