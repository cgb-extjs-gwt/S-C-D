using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class IndexColumn
    {
        public string ColumnName { get; set; }

        public SortDirection SortDirection { get; set; }

        public IndexColumn()
        {
        }

        public IndexColumn(string columnName, SortDirection sortDirection = SortDirection.Asc)
        {
            this.ColumnName = columnName;
            this.SortDirection = sortDirection;
        }
    }
}
