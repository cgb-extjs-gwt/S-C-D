using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectWhereSqlHelper : BaseSqlHelper
    {
        private readonly GroupBySqlHelper groupByHelper;

        public SelectWhereSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.groupByHelper = new GroupBySqlHelper(sqlBuilder);
        }

        public SelectGroupBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.groupByHelper.GroupBy(columns);
        }
    }
}
