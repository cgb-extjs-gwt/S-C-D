using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class OffsetFetchSqlHelper : SqlHelper, IOffsetFetchSqlHelper<SqlHelper>
    {
        public OffsetFetchSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SqlHelper OffsetFetch(int offset, int? fetch = null)
        {
            return new SqlHelper(new OffsetFetchSqlBuilder
            {
                Offset = offset,
                Fetch = fetch,
                SqlBuilder = this.ToSqlBuilder()
            });
        }
    }
}
