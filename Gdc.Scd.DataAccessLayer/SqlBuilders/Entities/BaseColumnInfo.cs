namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public abstract class BaseColumnInfo
    {
        public string Alias { get; set; }

        protected BaseColumnInfo(string alias)
        {
            this.Alias = alias;
        }

        protected BaseColumnInfo()
        {
        }
    }
}
