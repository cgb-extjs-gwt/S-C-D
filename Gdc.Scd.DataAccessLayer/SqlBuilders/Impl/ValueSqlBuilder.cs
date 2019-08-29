using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class ValueSqlBuilder : ISqlBuilder
    {
        private static readonly NumberFormatInfo numberFormat = new NumberFormatInfo
        {
            NumberDecimalSeparator = ".",
            NumberGroupSeparator = string.Empty
        };

        public ValueSqlBuilder(object value)
        {
            this.Value = value;
        }

        public object Value { get; set; }

        public string Build(SqlBuilderContext context)
        {
            string result = null;

            if (this.Value == null)
            {
                result = "NULL";
            }
            else if (this.Value is string)
            {
                result = $"'{this.Value}'";
            }
            else if (this.Value is int || this.Value is long)
            {
                result = this.Value.ToString();
            }
            else if (this.Value is bool booValue)
            {
                result = booValue ? "1" : "0";
            }
            else if (this.Value is double doubleValue)
            {
                result = doubleValue.ToString(numberFormat);
            }
            else if (this.Value is decimal decimalValue)
            {
                result = decimalValue.ToString(numberFormat);
            }
            else
            {
                throw new NotSupportedException();
            }

            return result;
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
