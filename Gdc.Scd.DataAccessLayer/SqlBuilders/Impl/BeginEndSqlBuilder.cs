using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class BeginEndSqlBuilder : BaseQuerySqlBuilder
    {
        public BeginEndSqlBuilder()
        {
        }

        public BeginEndSqlBuilder(ISqlBuilder query)
            : base(query)
        {
        }

        public override string Build(SqlBuilderContext context)
        {
            var query = this.Query.Build(context);

            return $"{Environment.NewLine}BEGIN{Environment.NewLine}{query}{Environment.NewLine}END{Environment.NewLine}";
        }
    }
}
