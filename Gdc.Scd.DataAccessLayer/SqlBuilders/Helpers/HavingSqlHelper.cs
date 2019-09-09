using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class HavingSqlHelper : OrderBySqlHelper
    {
        public HavingSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public OrderBySqlHelper Having(ConditionHelper condition)
        {
            return this.Having(condition.ToSqlBuilder());
        }

        public OrderBySqlHelper Having(ISqlBuilder condition)
        {
            return new OrderBySqlHelper(new HavingSqlBuilder
            {
                Query = this.ToSqlBuilder(),
                Condition = condition
            });
        }
    }
}
