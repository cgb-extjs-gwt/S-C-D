using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers
{
    public class JoinSqlHelper : SqlHelper, IJoinSqlHelper<ISqlBuilder>
    {
        public JoinSqlHelper(ISqlBuilder sqlBuilder) 
            : base(sqlBuilder)
        {
        }

        public ISqlBuilder Join(ISqlBuilder table, ISqlBuilder condition, JoinType type = JoinType.Inner)
        {
            return new JoinSqlBuilder
            {
                SqlBuilder = this.ToSqlBuilder(),
                Table = table,
                Condition = condition,
                Type = type
            };
        }

        public ISqlBuilder Join(ISqlBuilder table, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return this.Join(table, condition.ToSqlBuilder(), type);
        }

        public ISqlBuilder Join(string schemaName, string tableName, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            var table = new TableSqlBuilder
            {
                Schema = schemaName,
                Name = tableName
            };

            return this.Join(table, condition, type);
        }

        public ISqlBuilder Join(string tableName, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return this.Join(null, tableName, condition, type);
        }

        public ISqlBuilder Join(BaseEntityMeta meta, string referenceFieldName)
        {
            var referenceField = (ReferenceFieldMeta)meta.GetField(referenceFieldName);

            return this.Join(
                    referenceField.ReferenceMeta.Schema,
                    referenceField.ReferenceMeta.Name,
                    SqlOperators.Equals(
                        new ColumnInfo(referenceField.ForeignField.Name, meta.Name),
                        new ColumnInfo(referenceField.ReferenceValueField, referenceField.Name)));
        }

        public ISqlBuilder Join(BaseEntityMeta meta, ConditionHelper condition, JoinType type = JoinType.Inner)
        {
            return this.Join(meta.Schema, meta.Name, condition, type);
        }
    }
}
