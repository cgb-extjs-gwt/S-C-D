using System;
using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class CreateViewSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder Query { get; set; }

        public string Name { get; set; }

        public string Shema { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var tableBuilder = new TableSqlBuilder
            {
                Schema = this.Shema,
                Name = this.Name
            };

            var table = tableBuilder.Build(context);
            var query = this.Query.Build(context);

            return $"CREATE VIEW {table} AS{Environment.NewLine}{query}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            yield return this.Query;
        }
    }
}
