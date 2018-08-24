using System;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ConvertSqlBuilder : BaseQuerySqlBuilder
    {
        public TypeCode Type { get; set; }

        public override string Build(SqlBuilderContext context)
        {
            var typeBuilder = new TypeSqlBuilder
            {
                Type = this.Type
            };

            var sqlType = typeBuilder.Build(context);
            var query = this.Query.Build(context);

            return $"CONVERT({sqlType}, {query})";
        }
    }
}
