using System;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class IdColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<IdFieldMeta>
    {
        protected override string BuildType()
        {
            var typeBuilder = new TypeSqlBuilder
            {
                Type = TypeCode.Int64
            };

            var sqlType = typeBuilder.Build(null);

            return $"{sqlType} IDENTITY(1,1)";
        }
    }
}
