using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectFromSqlHelper : SqlHelper, 
        IWhereSqlHelper<SelectWhereSqlHelper>, 
        IGroupBySqlHelper<SelectGroupBySqlHelper>, 
        IJoinSqlHelper<SelectJoinSqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        private readonly GroupBySqlHelper groupByHelper;

        private readonly JoinSqlHelper joinSqlHelper;

        public SelectFromSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
            this.groupByHelper = new GroupBySqlHelper(sqlBuilder);
            this.joinSqlHelper = new JoinSqlHelper(sqlBuilder);
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

        public SelectJoinSqlHelper Join(ISqlBuilder table, ISqlBuilder condition, JoinType type = JoinType.Inner)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public SelectJoinSqlHelper Join(ISqlBuilder table, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public SelectJoinSqlHelper Join(string schemaName, string tableName, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(schemaName, tableName, condition, type));
        }

        public SelectJoinSqlHelper Join(string tableName, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(tableName, condition, type));
        }

        public SelectJoinSqlHelper Join(EntityMeta meta, string referenceFieldName)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(meta, referenceFieldName));
        }

        public SqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
