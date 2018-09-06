using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectJoinSqlHelper : SelectWhereSqlHelper, IJoinSqlHelper<SelectJoinSqlHelper>        
    {
        private readonly JoinSqlHelper joinSqlHelper;

        public SelectJoinSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.joinSqlHelper = new JoinSqlHelper(sqlBuilder);
        }

        public SelectJoinSqlHelper Join(ISqlBuilder table, ISqlBuilder condition, JoinType type = JoinType.Inner)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public SelectJoinSqlHelper Join(ISqlBuilder table, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public SelectJoinSqlHelper Join(string schemaName, string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(schemaName, tableName, condition, type, alias));
        }

        public SelectJoinSqlHelper Join(string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(tableName, condition, type, alias));
        }

        public SelectJoinSqlHelper Join(BaseEntityMeta meta, string referenceFieldName, string joinedTableAlias = null, string metaTableAlias = null)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(meta, referenceFieldName, joinedTableAlias, metaTableAlias));
        }

        public SelectJoinSqlHelper Join(BaseEntityMeta meta, ConditionHelper condition, JoinType type = JoinType.Inner, string aliasMetaTable = null)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(meta, condition, type, aliasMetaTable));
        }

        public SelectJoinSqlHelper Join(IEnumerable<JoinInfo> joinInfos)
        {
            return new SelectJoinSqlHelper(this.joinSqlHelper.Join(joinInfos));
        }
    }
}
