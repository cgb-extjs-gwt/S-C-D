using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class ColumnInfo : BaseColumnInfo
    {
        public string Name { get; set; }

        public string TableName { get; set; }

        public ColumnInfo(FieldMeta field, BaseEntityMeta meta, string alias = null)
            : this(field, meta.Name, alias)
        {
        }

        public ColumnInfo(FieldMeta field, string tableName, string alias = null)
            : this(field.Name, tableName, alias)
        {
        }

        public ColumnInfo(FieldMeta field)
            : this(field, (string)null)
        {
        }

        public ColumnInfo(string columnName, string tableName = null, string alias = null)
            : base(alias)
        {
            this.Name = columnName;
            this.TableName = tableName;
        }

        public ColumnInfo()
        {
        }
    }
}
