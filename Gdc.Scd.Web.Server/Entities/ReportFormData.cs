using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Gdc.Scd.Web.Server.Entities
{
    public class ReportFormData
    {
        public long Id { get; set; }

        public int Start { get; set; }

        public int Limit { get; set; }

        public string Report { get; set; }

        public string Filter { get; set; }

        public ReportFilterCollection AsFilterCollection()
        {
            IList<KeyValuePair<string, object>> pairs = new List<KeyValuePair<string, object>>();

            var jo = JObject.Parse(Filter);

            foreach (var o in jo)
            {
                object val = null;

                var token = o.Value;

                switch (token.Type)
                {
                    case JTokenType.Array:
                        val = token.ToObject<long[]>();
                        break;

                    case JTokenType.Integer:
                        val = token.ToObject<long>();
                        break;

                    case JTokenType.Float:
                        val = token.ToObject<double>();
                        break;

                    case JTokenType.String:
                        var str = token.ToObject<string>();
                        if (!string.IsNullOrEmpty(str))
                        {
                            val = str;
                        }
                        break;

                    case JTokenType.Boolean:
                        val = token.ToObject<bool>();
                        break;
                }

                if (val != null)
                {
                    pairs.Add(new KeyValuePair<string, object>(o.Key, val));
                }
            }

            return new ReportFilterCollection(pairs);
        }
    }
}