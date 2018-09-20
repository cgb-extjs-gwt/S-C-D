using Gdc.Scd.Web.Server.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Gdc.Scd.Web.Server
{
    public static class ApiControllerHelper
    {
        private const string APPLICATION_JSON = "application/json";
        private static readonly string DATAINFO_ITEMS = nameof(DataInfo<int>.Items).ToLower();
        private static readonly string DATAINFO_TOTAL = nameof(DataInfo<int>.Total).ToLower();

        public static HttpResponseMessage JsonContent(this ApiController ctrl, string json)
        {
            CheckJson(json);
            //
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, APPLICATION_JSON);
            return response;
        }

        public static HttpResponseMessage JsonContent(this ApiController ctrl, string jsonArray, int total)
        {
            return JsonContent(ctrl, WithDataInfo(jsonArray, total));
        }

        private static string WithDataInfo(string jsonArray, int total)
        {
            CheckJson(jsonArray);

            var sb = new StringBuilder();

            using (JsonWriter writer = new JsonTextWriter(new StringWriter(sb)))
            {
                writer.WriteStartObject();

                writer.WritePropertyName(DATAINFO_ITEMS);
                writer.WriteRawValue(jsonArray);

                writer.WritePropertyName(DATAINFO_TOTAL);
                writer.WriteValue(total);

                writer.WriteEndObject();
            }

            return sb.ToString();
        }

        private static void CheckJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("Invalid json");
            }
        }
    }
}