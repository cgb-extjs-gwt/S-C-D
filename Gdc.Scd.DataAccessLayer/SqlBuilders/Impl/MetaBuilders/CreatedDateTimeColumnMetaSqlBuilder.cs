using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class CreatedDateTimeColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>
    {
        protected override string BuildType(SqlBuilderContext context)
        {
            return "[DATETIME]";
        }

        protected override string GetDefaultExpresstion(SqlBuilderContext context)
        {
            return "GETUTCDATE()";
        }
    }
}
