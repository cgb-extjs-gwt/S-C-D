using System;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class SimpleColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<SimpleFieldMeta>
    {
        protected override string BuildType()
        {
            var typeBuilder = new TypeSqlBuilder
            {
                Type = this.Field.Type
            };

            return typeBuilder.Build(null);
        }
    }
}
