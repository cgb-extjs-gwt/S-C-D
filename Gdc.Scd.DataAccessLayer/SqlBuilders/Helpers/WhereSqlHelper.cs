using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class WhereSqlHelper : SqlHelper, IWhereSqlHelper<ISqlBuilder>
    {
        public WhereSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public ISqlBuilder Where(ISqlBuilder condition)
        {
            return new WhereSqlBuilder
            {
                Condition = condition,
                Query = this.ToSqlBuilder()
            };
        }

        public ISqlBuilder Where(ConditionHelper condition)
        {
            return this.Where(condition.ToSqlBuilder());
        }

        public ISqlBuilder Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            var columnFilter = filter.ToDictionary(
                keyValue => new ColumnInfo(keyValue.Key, tableName),
                keyValue => keyValue.Value);

            return this.Where(columnFilter);
        }

        public ISqlBuilder Where(IDictionary<ColumnInfo, IEnumerable<object>> filter) 
        {
            ISqlBuilder result;

            if (filter == null || filter.Count == 0)
            {
                result = this.ToSqlBuilder();
            }
            else
            {
                result = new WhereSqlBuilder
                {
                    Query = this.ToSqlBuilder(),
                    Condition = ConditionHelper.AndStatic(filter).ToSqlBuilder()
                };
            }

            return result;
        }
    }
}
