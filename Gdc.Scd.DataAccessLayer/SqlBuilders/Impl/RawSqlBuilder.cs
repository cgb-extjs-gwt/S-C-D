using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class RawSqlBuilder : ISqlBuilder
    {
        public string RawSql { get; set; }

        public RawSqlBuilder()
        {
        }

        public RawSqlBuilder(string rawSql)
        {
            this.RawSql = rawSql;
        }

        public string Build(SqlBuilderContext context)
        {
            return this.RawSql;
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return Enumerable.Empty<ISqlBuilder>();
        }
    }
}
