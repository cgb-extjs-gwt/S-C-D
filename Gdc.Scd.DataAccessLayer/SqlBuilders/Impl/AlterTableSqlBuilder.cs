using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AlterTableSqlBuilder : TableSqlBuilder
    {
        public ISqlBuilder Query { get; set; }

        public AlterTableSqlBuilder()
        {
        }

        public AlterTableSqlBuilder(string tableName, string schema = null, string database = null)
            : base(tableName, schema, database)
        {
        }

        public AlterTableSqlBuilder(BaseEntityMeta meta, string database = null)
            : base(meta, database)
        {
        }

        public override string Build(SqlBuilderContext context)
        {
            return $"ALTER TABLE {base.Build(context)}{Environment.NewLine}{Query.Build(context)} ";
        }

        public override IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            foreach (var sqlBuilder in base.GetChildrenBuilders())
            {
                yield return sqlBuilder;
            }

            yield return this.Query;
        }
    }
}
