using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectFromSqlHelper : SqlHelper, IFromSqlHelper<SelectJoinSqlHelper>
    {
        private readonly FromSqlHepler fromSqlHelper;

        public SelectFromSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.fromSqlHelper = new FromSqlHepler(sqlBuilder);
        }

        public SelectJoinSqlHelper From(string tabeName, string schemaName = null, string dataBaseName = null, string alias = null)
        {
            return new SelectJoinSqlHelper(this.fromSqlHelper.From(tabeName, schemaName, dataBaseName, alias));
        }

        public SelectJoinSqlHelper From(BaseEntityMeta meta, string alias = null)
        {
            return new SelectJoinSqlHelper(this.fromSqlHelper.From(meta, alias));
        }

        public SelectJoinSqlHelper FromQuery(ISqlBuilder query, string alias = null)
        {
            return new SelectJoinSqlHelper(this.fromSqlHelper.FromQuery(query, alias));
        }

        public SelectJoinSqlHelper FromQuery(SqlHelper sqlHelper, string alias = null)
        {
            return new SelectJoinSqlHelper(this.fromSqlHelper.FromQuery(sqlHelper, alias));
        }
    }
}
