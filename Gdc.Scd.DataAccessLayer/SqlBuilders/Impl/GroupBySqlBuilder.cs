using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class GroupBySqlBuilder : ISqlBuilder
    {
        public IEnumerable<ColumnInfo> Columns { get; set; }

        public ISqlBuilder SqlBuilder { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var columns =
                this.Columns.Select(column => new ColumnSqlBuilder { Name = column.Name, Table = column.TableName })
                            .Select(builder => builder.Build(context));

            var columnsStr = string.Join(", ", columns);
            var sql = this.SqlBuilder == null ? string.Empty : this.SqlBuilder.Build(context);

            return $"{sql} GROUP BY {columnsStr}";
        }
    }
}
