namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public abstract class BaseUpdateColumnInfo
    {
        public string Name { get; set; }

        public string TableName { get; set; }

        public BaseUpdateColumnInfo()
        {
        }

        public BaseUpdateColumnInfo(string name, string tableName = null)
        {
            this.Name = name;
            this.TableName = tableName;
        }
    }
}
