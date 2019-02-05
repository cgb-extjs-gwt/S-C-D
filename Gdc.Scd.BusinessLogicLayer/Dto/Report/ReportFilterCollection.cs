using System;
using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportFilterCollection : Dictionary<string, string>
    {
        public ReportFilterCollection() : base(StringComparer.OrdinalIgnoreCase) { }

        public ReportFilterCollection(IEnumerable<KeyValuePair<string, string>> items) : this()
        {
            if (items == null)
            {
                throw new ArgumentException("Invalid item collection");
            }

            foreach (var item in items)
            {
                if (!ContainsKey(item.Key))
                {
                    Add(item.Key, item.Value);
                }
            }
        }

        public bool TryGetVal(string key, out string value)
        {
            value = null;
            bool result = false;

            if (ContainsKey(key))
            {
                string v = this[key];
                if (!string.IsNullOrEmpty(v))
                {
                    result = true;
                    value = v;
                }
            }

            return result;
        }
    }
}
