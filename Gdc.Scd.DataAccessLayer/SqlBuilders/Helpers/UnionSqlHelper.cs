using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class UnionSqlHelper : OffsetFetchSqlHelper, IUnionSqlHelper<OffsetFetchSqlHelper>
    {
        public UnionSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public OffsetFetchSqlHelper Union(ISqlBuilder query, bool all = false)
        {
            return new OffsetFetchSqlHelper(new UnionSqlBuilder
            {
                All = all,
                Query1 = this.ToSqlBuilder(),
                Query2 = query
            });
        }

        public OffsetFetchSqlHelper Union(SqlHelper query, bool all = false)
        {
            return this.Union(query.ToSqlBuilder(), all);
        }
    }
}
