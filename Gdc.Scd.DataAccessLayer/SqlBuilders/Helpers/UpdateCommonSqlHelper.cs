using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class UpdateCommonSqlHelper : SqlHelper, IWhereSqlHelper<SqlHelper>, IJoinSqlHelper<UpdateCommonSqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        private readonly JoinSqlHelper joinSqlHelper;

        public UpdateCommonSqlHelper(ISqlBuilder sqlBuilder)
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
            this.joinSqlHelper = new JoinSqlHelper(sqlBuilder);
        }

        public SqlHelper Where(ISqlBuilder condition)
        {
            return new SqlHelper(this.whereHelper.Where(condition));
        }

        public SqlHelper Where(ConditionHelper condition)
        {
            return this.Where(condition.ToSqlBuilder());
        }

        public SqlHelper Where(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            return new SqlHelper(this.whereHelper.Where(filter, tableName));
        }

        public SqlHelper Where(IDictionary<ColumnInfo, IEnumerable<object>> filter)
        {
            return new SqlHelper(this.whereHelper.Where(filter));
        }

        public SqlHelper Where(IEnumerable<ConditionHelper> conditions)
        {
            return new SqlHelper(this.whereHelper.Where(conditions));
        }

        public UpdateCommonSqlHelper Join(ISqlBuilder table, ISqlBuilder condition, JoinType type = JoinType.Inner)
        {
            return new UpdateCommonSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public UpdateCommonSqlHelper Join(ISqlBuilder table, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new UpdateCommonSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public UpdateCommonSqlHelper Join(string schemaName, string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new UpdateCommonSqlHelper(this.joinSqlHelper.Join(schemaName, tableName, condition, type, alias));
        }

        public UpdateCommonSqlHelper Join(string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new UpdateCommonSqlHelper(this.joinSqlHelper.Join(tableName, condition, type, alias));
        }

        public UpdateCommonSqlHelper Join(BaseEntityMeta meta, string referenceFieldName, string joinedTableAlias = null, string metaTableAlias = null, JoinType joinType = JoinType.Inner)
        {
            return new UpdateCommonSqlHelper(this.joinSqlHelper.Join(meta, referenceFieldName, joinedTableAlias, metaTableAlias, joinType));
        }

        public UpdateCommonSqlHelper Join(BaseEntityMeta meta, ConditionHelper condition, JoinType type = JoinType.Inner, string aliasMetaTable = null)
        {
            return new UpdateCommonSqlHelper(this.joinSqlHelper.Join(meta, condition, type, aliasMetaTable));
        }

        public UpdateCommonSqlHelper Join(IEnumerable<JoinInfo> joinInfos)
        {
            return new UpdateCommonSqlHelper(this.joinSqlHelper.Join(joinInfos));
        }
    }
}
