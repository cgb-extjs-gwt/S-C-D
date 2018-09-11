using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class WithQuery
    {
        public string Name { get; set; }

        public IEnumerable<string> ColumnNames { get; set; }

        public ISqlBuilder Query { get; set; }
    }
}
