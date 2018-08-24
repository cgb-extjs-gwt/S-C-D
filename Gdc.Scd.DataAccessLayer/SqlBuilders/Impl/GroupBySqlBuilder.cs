using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class GroupBySqlBuilder : BaseQuerySqlBuilder
    {
        public IEnumerable<ColumnInfo> Columns { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var columns =
                this.Columns.Select(column => new ColumnSqlBuilder { Name = column.Name, Table = column.TableName })
                            .Select(builder => builder.Build(context));

            var columnsStr = string.Join(", ", columns);
            var sql = this.Query == null ? string.Empty : this.Query.Build(context);

            return $"{sql} GROUP BY {columnsStr}";
        }
    }
}
