using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class InSqlBuilder : ISqlBuilder
    {
        public string Table { get; set; }

        public string Column { get; set; }

        public IEnumerable<ISqlBuilder> Values { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var columnBuilder = new ColumnSqlBuilder
            {
                Table = this.Table,
                Name = this.Column
            };
            var column = columnBuilder.Build(context);
            var values = string.Join(", ", this.Values.Select(builder => builder.Build(context)));

            return $"{column} IN ({values})";
        }
    }
}
