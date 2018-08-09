using System;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectGroupBySqlHelper : SqlHelper, IOrderBySqlHelper<SqlHelper>
    {
        private readonly OrderBySqlHelper orderBySqlHelper;

        public SelectGroupBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.orderBySqlHelper = new OrderBySqlHelper(sqlBuilder);
        }

        public SqlHelper OrderBy(params OrderByInfo[] infos)
        {
            return this.orderBySqlHelper.OrderBy(infos);
        }

        public SqlHelper OrderBy(OrderByDirection direction, params ColumnInfo[] columns)
        {
            return this.orderBySqlHelper.OrderBy(direction, columns);
        }

        public SqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
