using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectJoinSqlHelper : SqlHelper, IWhereSqlHelper<SelectWhereSqlHelper>, IJoinSqlHelper<SelectJoinSqlHelper>
    {
        private readonly JoinSqlHelper joinSqlHelper;

        private readonly WhereSqlHelper whereHelper;

        public SelectJoinSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.joinSqlHelper = new JoinSqlHelper(sqlBuilder);
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
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

        public SelectJoinSqlHelper Join(BaseEntityMeta meta, string referenceFieldName)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(meta, referenceFieldName));
        }

        public SelectJoinSqlHelper Join(BaseEntityMeta meta, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(meta, condition, type));
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

        public SqlHelper Union()
        {
            throw new NotImplementedException();
        }
    }
}
