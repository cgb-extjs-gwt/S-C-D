using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectFromSqlHelper : SqlHelper
    {
        private readonly WhereSqlHelper whereHelper;

        private readonly GroupBySqlHelper groupByHelper;

        public SelectFromSqlHelper(ISqlBuilder sqlBuilder) 
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
            return new SelectWhereSqlHelper(this.whereHelper.Where(condition));
        }

        public SelectWhereSqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SelectWhereSqlHelper(this.whereHelper.Where(filter, tableName));
        }

        public SelectGroupBySqlHelper GroupBy(params ColumnInfo[] columns)
        {
            return this.groupByHelper.GroupBy(columns);
        }

        public SelectJoinSqlHelper Join()
        {
            throw new NotImplementedException();
        }

        public SqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
