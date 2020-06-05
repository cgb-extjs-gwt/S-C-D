using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class ComputedColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<ComputedFieldMeta>
    {
        protected override string BuildType(SqlBuilderContext context)
        {
            return null;
        }

        public override string Build(SqlBuilderContext context)
        {
            var query = this.Field.Query.Build(context);

            return $"[{this.Field.Name}] AS ({query})";
        }
    }
}
