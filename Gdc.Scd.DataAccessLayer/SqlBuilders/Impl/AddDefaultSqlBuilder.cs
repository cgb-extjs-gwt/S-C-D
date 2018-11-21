using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AddDefaultSqlBuilder :  ISqlBuilder
    {
        public string DefaultValue { get; set; }
        public string ColumnName { get; set; }

        public string Build(SqlBuilderContext context)
        {
            return $"ADD DEFAULT ({this.DefaultValue}) FOR [{this.ColumnName}]";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
