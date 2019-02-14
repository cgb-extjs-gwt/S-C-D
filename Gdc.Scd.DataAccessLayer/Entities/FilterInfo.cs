using System.Collections.Generic;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class FilterInfo
    {
        public IDictionary<string, IEnumerable<object>> Filter { get; set; }

        public string TableName { get; set; }

        public FilterInfo()
        {
        }

        public FilterInfo(IDictionary<string, IEnumerable<object>> filter, string tableName = null)
        {
            this.Filter = filter;
            this.TableName = tableName;
        }
    }
}
