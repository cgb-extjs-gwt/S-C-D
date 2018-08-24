using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class MaxSqlBuilder : BaseQuerySqlBuilder
    {
        public override string Build(SqlBuilderContext context)
        {
            var query = this.Query.Build(context);

            return $"MAX({query})";
        }
    }
}
