namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class ValueUpdateColumnInfo : BaseUpdateColumnInfo
    {
        public object Value { get; set; }

        public ValueUpdateColumnInfo()
        {
        }

        public ValueUpdateColumnInfo(string name, object value)
            : base(name)
        {
            this.Value = value;
        }
    }
}
