using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gdc.Scd.Tests.Util
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings serializerSettings =
                new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.Indented };

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