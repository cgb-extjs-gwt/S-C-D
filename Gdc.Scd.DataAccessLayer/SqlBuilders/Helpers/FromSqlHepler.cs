using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class FromSqlHepler : SqlHelper, IFromSqlHelper<ISqlBuilder>
    {
        public FromSqlHepler(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public ISqlBuilder From(string tabeName, string schemaName = null, string dataBaseName = null, string alias = null)
        {
            var tableBuilder = new TableSqlBuilder
            {
                DataBase = dataBaseName,
                Schema = schemaName,
                Name = tabeName
            };

            var fromBuilder =
                alias == null ?
                    (ISqlBuilder)tableBuilder
                    : new AliasSqlBuilder
                    {
                        Alias = alias,
                        SqlBuilder = tableBuilder
                    };

            return new FromSqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                From = fromBuilder
            };
        }

        public ISqlBuilder From(BaseEntityMeta meta, string alias = null)
        {
            return this.From(meta.Name, meta.Schema, alias: alias);
        }

        public ISqlBuilder FromQuery(ISqlBuilder query)
        {
            return new FromSqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                From = new BracketsSqlBuilder
                {
                    SqlBuilder = query
                }
            };
        }

        public ISqlBuilder FromQuery(SqlHelper sqlHelper)
        {
            return this.FromQuery(sqlHelper.ToSqlBuilder());
        }
    }
}
