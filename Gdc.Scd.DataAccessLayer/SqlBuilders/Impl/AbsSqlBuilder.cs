using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AbsSqlBuilder : BaseQuerySqlBuilder
    {
        public override string Build(SqlBuilderContext context)
        {
            return $"ABS({this.Query.Build(context)})";
        }
    }
}
