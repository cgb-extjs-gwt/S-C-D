using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class RawSqlBuilder : ISqlBuilder
    {
        private static ISqlBuilder[] EMPTY_CHILD = new ISqlBuilder[0];

        private IEnumerable<ISqlBuilder> children;

        public string RawSql { get; set; }

        public RawSqlBuilder() { }

        public RawSqlBuilder(string rawSql)
        {
            this.RawSql = rawSql;
        }

        public RawSqlBuilder(string rawSql, IEnumerable<ISqlBuilder> children)
        {
            this.RawSql = rawSql;
            this.children = children;
        }

        public string Build(SqlBuilderContext context)
        {
            return this.RawSql;
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            return children ?? EMPTY_CHILD;
        }
    }
}
