using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class FromSqlHelper : BaseSqlHelper
    {
        private readonly WhereSqlHelper whereHelper;

        private readonly GroupBySqlHelper groupByHelper;

        public FromSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
            this.groupByHelper = new GroupBySqlHelper(sqlBuilder);
        }

        public SelectWhereSqlHelper Where(ISqlBuilder condition)
        {
            return new SelectWhereSqlHelper(this.whereHelper.Where(condition));
        }

        public SelectWhereSqlHelper Where(ConditionHelper condition)
        {
            return this.Where(condition.ToSqlBuilder());
        }

        public SelectGroupBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.groupByHelper.GroupBy(columns);
        }

        public SelectJoinSqlHelper Join()
        {
            throw new NotImplementedException();
        }

        public BaseSqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
