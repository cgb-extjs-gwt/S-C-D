using System;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders
{
    public class SimpleColumnMetaSqlBuilder : BaseColumnMetaSqlBuilder<SimpleFieldMeta>
    {
        protected override string BuildType()
        {
            string result;

            switch (this.Field.Type)
            {
                case TypeCode.Double:
                    result = "[float]";
                    break;

                case TypeCode.String:
                    result = "[nvarchar](30)";
                    break;

                case TypeCode.Int64:
                    result = "[bigint]";
                    break;

                case TypeCode.DateTime:
                    result = "[DATETIME]";
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            return result;
        }
    }
}
