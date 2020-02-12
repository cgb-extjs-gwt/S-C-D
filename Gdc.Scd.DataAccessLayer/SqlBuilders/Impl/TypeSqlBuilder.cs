using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class TypeSqlBuilder : ISqlBuilder
    {
        public TypeCode Type { get; set; }

        public string Build(SqlBuilderContext context)
        {
            string result = null;

            switch (this.Type)
            {
                case TypeCode.Double:
                    result = "[float]";
                    break;

                case TypeCode.String:
                    result = "[nvarchar](30)";
                    break;

                case TypeCode.Int32:
                    result = "[int]";
                    break;

                case TypeCode.UInt64:
                case TypeCode.Int64:
                    result = "[bigint]";
                    break;

                case TypeCode.DateTime:
                    result = "[DATETIME]";
                    break;

                case TypeCode.Boolean:
                    result = "[BIT]";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
