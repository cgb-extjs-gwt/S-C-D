using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class ReferenceColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<ReferenceFieldMeta>
    {
        protected override string BuildType()
        {
            return "[bigint]";
        }

        protected override bool IsNullOption()
        {
            return this.Field.IsNullOption;
        }
    }
}
