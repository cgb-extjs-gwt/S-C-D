using System.Collections.Generic;
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
                    Condition = ConditionHelper.AndStatic(filter, tableName).ToSqlBuilder()
                };
            }

            return result;
        }
    }
}
