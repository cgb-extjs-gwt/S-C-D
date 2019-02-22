using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DisableEnableTriggersSqlBuilder : ISqlBuilder
    {
        public string Schema { get; set; }

        public string Table { get; set; }

        public DispabelEnableType Type { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var table = new TableSqlBuilder
            {
                Schema = this.Schema,
                Name = this.Table
            };

            var type = this.Type.ToString().ToUpper();

            return $"{type} TRIGGER ALL ON {table.Build(context)}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
