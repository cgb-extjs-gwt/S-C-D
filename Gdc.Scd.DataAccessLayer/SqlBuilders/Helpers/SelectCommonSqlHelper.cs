using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class SelectCommonSqlHelper : SqlHelper, 
        IWhereSqlHelper<SelectWhereSqlHelper>, 
        IGroupBySqlHelper<SelectGroupBySqlHelper>, 
        IJoinSqlHelper<SelectCommonSqlHelper>,
        IOrderBySqlHelper<SqlHelper>
    {
        private readonly WhereSqlHelper whereHelper;

        private readonly GroupBySqlHelper groupByHelper;

        private readonly JoinSqlHelper joinSqlHelper;

        private readonly OrderBySqlHelper orderBySqlHelper;

        public SelectCommonSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
            this.whereHelper = new WhereSqlHelper(sqlBuilder);
            this.groupByHelper = new GroupBySqlHelper(sqlBuilder);
            this.joinSqlHelper = new JoinSqlHelper(sqlBuilder);
            this.orderBySqlHelper = new OrderBySqlHelper(sqlBuilder);
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

        public SelectCommonSqlHelper Join(ISqlBuilder table, ISqlBuilder condition, JoinType type = JoinType.Inner)
        {
            return new SelectCommonSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public SelectCommonSqlHelper Join(ISqlBuilder table, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return new SelectCommonSqlHelper(this.joinSqlHelper.Join(table, condition, type));
        }

        public SelectCommonSqlHelper Join(string schemaName, string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new SelectCommonSqlHelper(this.joinSqlHelper.Join(schemaName, tableName, condition, type, alias));
        }

        public SelectCommonSqlHelper Join(string tableName, ConditionHelper condition, JoinType type = JoinType.Inner, string alias = null)
        {
            return new SelectCommonSqlHelper(this.joinSqlHelper.Join(tableName, condition, type, alias));
        }

        public SelectCommonSqlHelper Join(BaseEntityMeta meta, string referenceFieldName, string aliasMetaTable = null)
        {
            return new SelectCommonSqlHelper(this.joinSqlHelper.Join(meta, referenceFieldName, aliasMetaTable));
        }

        public SelectCommonSqlHelper Join(BaseEntityMeta meta, ConditionHelper condition, JoinType type = JoinType.Inner, string aliasMetaTable = null)
        {
            return new SelectCommonSqlHelper(this.joinSqlHelper.Join(meta, condition, type, aliasMetaTable));
        }

        public SqlHelper OrderBy(params OrderByInfo[] infos)
        {
            return this.orderBySqlHelper.OrderBy(infos);
        }

        public SqlHelper OrderBy(OrderByDirection direction, params ColumnInfo[] columns)
        {
            return this.orderBySqlHelper.OrderBy(direction, columns);
        }
    }
}
