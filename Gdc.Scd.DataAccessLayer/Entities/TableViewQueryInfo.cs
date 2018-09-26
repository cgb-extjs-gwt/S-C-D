using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class TableViewQueryInfo
    {
        public CostBlockEntityMeta Meta { get; set; }

        public IEnumerable<string> FieldNames { get; set; }
    }
}
