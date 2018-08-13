using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class CreatedDateTimeColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<CreatedDateTimeFieldMeta>
    {
        protected override string BuildType()
        {
            return "[DATETIME]";
        }

        protected override string GetDefaultExpresstion()
        {
            return "GETUTCDATE()";
        }
    }
}
