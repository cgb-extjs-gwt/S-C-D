using System;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class IdColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<IdFieldMeta>
    {
        protected override string BuildType(SqlBuilderContext context)
        {
            var typeBuilder = new TypeSqlBuilder
            {
                Type = TypeCode.Int64
            };

            var sqlType = typeBuilder.Build(context);

            return $"{sqlType} IDENTITY(1,1)";
        }
    }
}
