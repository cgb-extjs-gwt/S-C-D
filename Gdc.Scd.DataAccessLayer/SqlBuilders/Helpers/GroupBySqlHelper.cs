using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class GroupBySqlHelper : SqlHelper
    {
        public GroupBySqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public SelectGroupBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return new SelectGroupBySqlHelper(new GroupBySqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                Columns = columns
            });
        }
    }
}
