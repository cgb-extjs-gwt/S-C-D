using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class UnionSqlHelper : SqlHelper, IUnionSqlHelper<UnionSqlHelper>
    {
        public UnionSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public UnionSqlHelper Union(ISqlBuilder query, bool all = false)
        {
            return new UnionSqlHelper(new UnionSqlBuilder
            {
                All = all,
                Query1 = this.ToSqlBuilder(),
                Query2 = query
            });
        }

        public UnionSqlHelper Union(SqlHelper query, bool all = false)
        {
            return this.Union(query.ToSqlBuilder(), all);
        }
    }
}
