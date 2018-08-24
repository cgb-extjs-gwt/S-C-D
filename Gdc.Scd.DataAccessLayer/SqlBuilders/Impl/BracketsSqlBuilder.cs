using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class BracketsSqlBuilder : BaseQuerySqlBuilder
    {
        public override string Build(SqlBuilderContext context)
        {
            return $"({this.Query.Build(context)})";
        }
    }
}
