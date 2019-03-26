using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class CostBlockSelectQueryData
    {
        public CostBlockEntityMeta CostBlock { get; set; }

        public CostBlockSelectCostElementInfo[] CostElementInfos { get; set; }

        public string[] GroupedFields { get; set; }

        public bool IsGroupedFieldsNameSelected { get; set; }

        public IDictionary<string, IEnumerable<object>> Filter { get; set; }

        public string IntoTable { get; set; }

        public string Alias { get; set; }
    }
}
