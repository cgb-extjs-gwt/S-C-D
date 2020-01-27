using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;

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

        public IndexColumn(FieldMeta field, SortDirection sortDirection = SortDirection.Asc)
            : this(field.Name, sortDirection)
        {
        }
    }
}
