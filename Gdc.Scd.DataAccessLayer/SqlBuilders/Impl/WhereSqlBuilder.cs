using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class WhereSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder SqlBuilder { get; set; }

        public ISqlBuilder Condition { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var sql = this.SqlBuilder == null ? string.Empty : this.SqlBuilder.Build(context);

            return $"{this.SqlBuilder.Build(context)} WHERE {this.Condition.Build(context)}";
        }
    }
}
