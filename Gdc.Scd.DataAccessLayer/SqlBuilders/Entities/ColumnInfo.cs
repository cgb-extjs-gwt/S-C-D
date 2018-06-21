using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class ColumnInfo : BaseColumnInfo
    {
        public string Name { get; set; }

        public string TableName { get; set; }

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
