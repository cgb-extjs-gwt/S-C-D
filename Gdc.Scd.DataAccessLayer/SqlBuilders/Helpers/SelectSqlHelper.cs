using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectSqlHelper : SqlHelper, IFromSqlHelper<SelectCommonSqlHelper>
    {
        private readonly FromSqlHepler fromSqlHelper;

        public SelectSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.fromSqlHelper = new FromSqlHepler(sqlBuilder);
        }

        public SelectCommonSqlHelper From(string tabeName, string schemaName = null, string dataBaseName = null, string alias = null)
        {
            return new SelectCommonSqlHelper(this.fromSqlHelper.From(tabeName, schemaName, dataBaseName, alias));
        }

        public SelectCommonSqlHelper From(BaseEntityMeta meta, string alias = null)
        {
            return new SelectCommonSqlHelper(this.fromSqlHelper.From(meta, alias));
        }

        public SelectCommonSqlHelper FromQuery(ISqlBuilder query)
        {
            return new SelectCommonSqlHelper(this.fromSqlHelper.FromQuery(query));
        }

        public SelectCommonSqlHelper FromQuery(SqlHelper sqlHelper)
        {
            return new SelectCommonSqlHelper(this.fromSqlHelper.FromQuery(sqlHelper));
        }
    }
}
