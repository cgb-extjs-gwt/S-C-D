using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class NotInSqlBuilder : ISqlBuilder
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

            return $"{column} NOT IN ({values})";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return this.Values;
        }
    }
}
