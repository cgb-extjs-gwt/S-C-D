using System.Collections.Generic;

namespace Gdc.Scd.Web.Server.Entities
{
    public class DataInfo<T>
    {
        public IEnumerable<T> Items { get; set; }

        public int Total { get; set; }
    }
}
