using System;
using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportFilterCollection
    {
        Dictionary<string, object> map;

        public ReportFilterCollection(IEnumerable<KeyValuePair<string, object>> items)
        {
            if (items == null)
            {
                throw new ArgumentException("Invalid item collection");
            }

            map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (!map.ContainsKey(item.Key))
                {
                    map.Add(item.Key, item.Value);
                }
            }
        }

        public bool TryGetVal(string key, out object value)
        {
            value = null;
            bool result = false;

            if (map.ContainsKey(key))
            {
                var v = map[key];
                if (v != null)
                {
                    result = true;
                    value = v;
                }
            }

            return result;
        }

        public bool TryGetVal(string key, out long[] value)
        {
            value = null;
            bool result = false;

            if (map.ContainsKey(key))
            {
                var v = map[key] as long[];
                if (v != null)
                {
                    result = true;
                    value = v;
                }
            }

            return result;
        }
    }
}
