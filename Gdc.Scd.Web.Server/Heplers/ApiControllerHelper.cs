using Gdc.Scd.Web.Server.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace Gdc.Scd.Web.Server
{
    public static class ApiControllerHelper
    {
        private static readonly string DATAINFO_ITEMS = nameof(DataInfo<int>.Items).ToLower();
        private static readonly string DATAINFO_TOTAL = nameof(DataInfo<int>.Total).ToLower();

        public static HttpResponseMessage ExcelContent(
                this ApiController ctrl,
                Stream data,
                string fileName
            )
        {
            return MediaContent(ctrl, data, fileName, MimeTypes.APPLICATION_VND_MS_EXCEL);
        }

        public static HttpResponseMessage MediaContent(
                this ApiController ctrl,
                Stream data,
                string fileName,
                string mediaType
            )
        {
            var content = new StreamContent(data);
            var headers = content.Headers;
            headers.ContentType = new MediaTypeHeaderValue(mediaType);
            headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(data);
            return response;
        }

        public static HttpResponseMessage JsonContent(this ApiController ctrl, string json)
        {
            CheckJson(json);
            //
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, MimeTypes.APPLICATION_JSON);
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