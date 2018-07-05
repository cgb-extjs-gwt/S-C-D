using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class IdColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<IdFieldMeta>
    {
        protected override string BuildType()
        {
            return "[bigint] IDENTITY(1,1)";
        }

        protected override bool IsNullOption()
        {
            return false;
        }
    }
}
