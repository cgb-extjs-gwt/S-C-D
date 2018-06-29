using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class InsertSqlBuilder : ISqlBuilder
    {
        public string Schema { get; set; }

        public string Table { get; set; }

        public IEnumerable<string> Columns { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var tableBuilder = new TableSqlBuilder
            {
                Schema = this.Schema,
                Name = this.Table
            };
            var table = tableBuilder.Build(context);
            var columns =
                this.Columns.Select(column => new ColumnSqlBuilder { Name = column })
                            .Select(builder => builder.Build(context));

            return $"INSERT INTO {table} ({string.Join(",", columns)})";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
