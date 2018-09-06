using System;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class ReferenceColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<ReferenceFieldMeta>
    {
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
