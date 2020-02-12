using System;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class ReferenceColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<ReferenceFieldMeta>
    {
        public ReferenceColumnMetaSqlBuilder()
        {
        }

        public ReferenceColumnMetaSqlBuilder(ReferenceFieldMeta field)
            : base(field)
        {
        }

        protected override string BuildType()
        {
            var typeBuilder = new TypeSqlBuilder
            {
                Type = TypeCode.Int64
            };

            return typeBuilder.Build(null);
        }
    }
}
