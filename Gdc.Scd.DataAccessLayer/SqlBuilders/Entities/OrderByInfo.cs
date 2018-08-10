using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class OrderByInfo
    {
        public ISqlBuilder SqlBuilder { get; set; }

        public SortDirection Direction { get; set; }
    }
}
