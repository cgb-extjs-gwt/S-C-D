using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class CreateIndexSqlBuilder : NameSqlBuilder
    {
        public IndexType Type { get; set; }

        public string Schema { get; set; }

        public string Table { get; set; }

        public IEnumerable<IndexColumn> Columns { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var type = this.Type.ToString().ToUpper();
            var tableSqlBuilder = new TableSqlBuilder
            {
                Schema = this.Schema,
                Name = this.Table
            };

            var name = this.GetSqlName(this.Name);
            var columns = this.Columns.Select(column => this.BuildColumn(column, context));

            return $"CREATE {type} INDEX {name} ON {Environment.NewLine}{tableSqlBuilder.Build(context)} {Environment.NewLine}({string.Join(", ", columns)})";
        }

        private string BuildColumn(IndexColumn column, SqlBuilderContext context)
        {
            var columnSqlBuilder = new ColumnSqlBuilder
            {
                Name = column.ColumnName
            };

            var sort = column.SortDirection.ToString().ToUpper();

            return $"{columnSqlBuilder.Build(context)} {sort}";
        }
    }
}
