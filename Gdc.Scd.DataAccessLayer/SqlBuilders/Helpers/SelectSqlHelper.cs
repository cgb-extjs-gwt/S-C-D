using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectSqlHelper : SqlHelper, IFromSqlHelper<SelectFromSqlHelper>
    {
        private readonly FromSqlHepler fromSqlHelper;

        public SelectSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.fromSqlHelper = new FromSqlHepler(sqlBuilder);
        }

        public SelectFromSqlHelper From(string tabeName, string schemaName = null, string dataBaseName = null, string alias = null)
        {
            return new SelectFromSqlHelper(this.fromSqlHelper.From(tabeName, schemaName, dataBaseName, alias));
        }

        public SelectFromSqlHelper From(BaseEntityMeta meta, string alias = null)
        {
            return new SelectFromSqlHelper(this.fromSqlHelper.From(meta, alias));
        }

        public SelectFromSqlHelper FromQuery(ISqlBuilder query)
        {
            return new SelectFromSqlHelper(this.fromSqlHelper.FromQuery(query));
        }

        public SelectFromSqlHelper FromQuery(SqlHelper sqlHelper)
        {
            return new SelectFromSqlHelper(this.fromSqlHelper.FromQuery(sqlHelper));
        }
    }
}
