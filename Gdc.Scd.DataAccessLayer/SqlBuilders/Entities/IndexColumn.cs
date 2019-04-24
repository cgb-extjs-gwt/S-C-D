using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class IndexColumn
    {
        public string ColumnName { get; set; }

        public SortDirection SortDirection { get; set; }
    }
}
