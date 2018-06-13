using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class MaxSqlBuilder : ISqlBuilder
    {
        public string ColumnName { get; set; }

        public string TableName { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var columnBuilder = new ColumnSqlBuilder { Table = this.TableName, Name = this.ColumnName };
            var column = columnBuilder.Build(context);

            return $"MAX({column})";
        }
    }
}
