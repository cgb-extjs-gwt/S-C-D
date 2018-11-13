using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectIntoSqlHelper : SelectFromSqlHelper
    {
        public SelectIntoSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SelectFromSqlHelper Into(string tableName, string schema = null, string database = null)
        {
            return new SelectFromSqlHelper(new IntoSqlBuilder
            {
                DataBase = database,
                Schema = schema,
                Name = tableName,
                Query = this.ToSqlBuilder()
            });
        }

        public SelectFromSqlHelper Into(BaseEntityMeta meta)
        {
            return this.Into(meta.Name, meta.Schema);
        }
    }
}
