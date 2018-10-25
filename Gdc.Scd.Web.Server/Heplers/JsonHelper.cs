using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server
{
    public static class JsonHelper
    {
        public static string AsJson(this object o)
        {
            return JsonConvert.SerializeObject(o);
        }
    }
}