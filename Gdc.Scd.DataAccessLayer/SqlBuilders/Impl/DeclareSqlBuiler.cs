using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DeclareSqlBuiler : ISqlBuilder
    {
        public string Name { get; set; }

        public TypeCode? TypeCode { get; set; }

        public object DefaultValue { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var type = this.GetType(context);
            var value = this.GetDefaultValue(context);

            return $"DECLARE {this.Name} {type} = {value}";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }

        private string GetType(SqlBuilderContext context)
        {
            TypeCode typeCode;

            if (this.TypeCode.HasValue)
            {
                typeCode = this.TypeCode.Value;
            }
            else if (this.DefaultValue == null)
            {
                throw new Exception(
                    $"'{nameof(this.TypeCode)}' or '{nameof(this.DefaultValue)}' must have non null value");
            }
            else
            {
                typeCode = Type.GetTypeCode(this.DefaultValue.GetType());
            }

            var typeSqlBuilder = new TypeSqlBuilder
            {
                Type = typeCode
            };

            return typeSqlBuilder.Build(context);
        }

        private string GetDefaultValue(SqlBuilderContext context)
        {
            var valueSqlBuilder = new ValueSqlBuilder(this.DefaultValue);

            return valueSqlBuilder.Build(context);
        }
    }
}
