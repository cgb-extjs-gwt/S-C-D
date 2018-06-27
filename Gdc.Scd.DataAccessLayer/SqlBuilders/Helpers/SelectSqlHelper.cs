using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectSqlHelper : SqlHelper
    {
        public SelectSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SelectFromSqlHelper From(string tabeName, string schemaName = null, string dataBaseName = null)
        {
            return new SelectFromSqlHelper(new FromSqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                From = new TableSqlBuilder
                {
                    DataBase = dataBaseName,
                    Schema = schemaName,
                    Name = tabeName
                }
            });
        }

        public SelectFromSqlHelper From(EntityMeta meta)
        {
            return this.From(meta.Name, meta.Shema);
        }

        public SelectFromSqlHelper FromQuery(ISqlBuilder query)
        {
            return new SelectFromSqlHelper(new FromSqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                From = new BracketsSqlBuilder
                {
                    SqlBuilder = query
                }
            });
        }

        public SelectFromSqlHelper FromQuery(SqlHelper sqlHelper)
        {
            return this.FromQuery(sqlHelper.ToSqlBuilder());
        }
    }
}
