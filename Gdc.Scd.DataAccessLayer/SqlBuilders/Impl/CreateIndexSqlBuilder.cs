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

        public IEnumerable<string> IncludeColumns { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var type = this.Type.ToString().ToUpper();
            var tableSqlBuilder = new TableSqlBuilder
            {
                Schema = this.Schema,
                Name = this.Table
            };

            var name = this.GetSqlName(this.Name);
            var columns = string.Join(", ", this.Columns.Select(column => this.BuildColumn(column, context)));
            var table = tableSqlBuilder.Build(context);

            var query = $"CREATE {type} INDEX {name} ON {Environment.NewLine}{table} {Environment.NewLine}({columns})";

            if (this.IncludeColumns != null)
            {
                var includeColumns =
                    this.IncludeColumns.Select(column => new ColumnSqlBuilder(column).Build(context));

                query += $"{Environment.NewLine}INCLUDE({string.Join(", ", includeColumns)})";
            }

            return query;
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
