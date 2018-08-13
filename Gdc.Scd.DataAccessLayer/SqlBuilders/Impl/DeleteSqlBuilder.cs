using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class DeleteSqlBuilder : ISqlBuilder
    {
        public string Build(SqlBuilderContext context)
        {
            return $"DELETE";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return new ISqlBuilder[0];
        }
    }
}
