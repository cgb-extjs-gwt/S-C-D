using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class AbsSqlBuilder : BaseSqlBuilder
    {
        public override string Build(SqlBuilderContext context)
        {
            return $"ABS({this.SqlBuilder.Build(context)})";
        }
    }
}
