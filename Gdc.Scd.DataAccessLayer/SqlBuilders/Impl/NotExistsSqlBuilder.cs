using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class NotExistsSqlBuilder : ExistsSqlBuilder
    {
        public override string Build(SqlBuilderContext context)
        {
            return $"NOT {base.Build(context)}";
        }
    }
}
