using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings serializerSettings =
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static string AsJson(this object o)
        {
            return JsonConvert.SerializeObject(o, serializerSettings);
        }

        public static T AsObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }
    }
}