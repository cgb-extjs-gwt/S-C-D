using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class UpdateJoinSqlHelper : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<UpdateJoinSqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        private readonly JoinSqlHelper joinSqlHelper;

        public UpdateJoinSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);

            this.joinSqlHelper = new JoinSqlHelper(sqlBuilder);
        }

        public UpdateJoinSqlHelper Join(ISqlBuilder table, ISqlBuilder condition, JoinType type = JoinType.Inner)
        {
            return new UpdateJoinSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public UpdateJoinSqlHelper Join(ISqlBuilder table, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new UpdateJoinSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public UpdateJoinSqlHelper Join(string schemaName, string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new UpdateJoinSqlHelper(this.joinSqlHelper.Join(schemaName, tableName, condition, type, alias));
        }

        public UpdateJoinSqlHelper Join(string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new UpdateJoinSqlHelper(this.joinSqlHelper.Join(tableName, condition, type, alias));
        }

        public UpdateJoinSqlHelper Join(BaseEntityMeta meta, string referenceFieldName, string aliasMetaTable = null)
        {
            return new UpdateJoinSqlHelper(this.joinSqlHelper.Join(meta, referenceFieldName, aliasMetaTable));
        }

        public UpdateJoinSqlHelper Join(BaseEntityMeta meta, ConditionHelper condition, JoinType type = JoinType.Inner, string aliasMetaTable = null)
        {
            return new UpdateJoinSqlHelper(this.joinSqlHelper.Join(meta, condition, type, aliasMetaTable));
        }

        public SqlHelper Where(ISqlBuilder condition)
        {
            return new SqlHelper(this.whereHelper.Where(condition));
        }

        public SqlHelper Where(ConditionHelper condition)
        {
            return new SqlHelper(this.whereHelper.Where(condition));
        }

        public SqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SqlHelper(this.whereHelper.Where(filter, tableName));
        }
    }
}
