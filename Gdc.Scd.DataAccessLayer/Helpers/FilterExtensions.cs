using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class FilterExtensions
    {
        public static IDictionary<string, IEnumerable<object>> Convert<TCollection, TItem>(this IDictionary<string, TCollection> filter)
            where TCollection : IEnumerable<TItem>
        {
            return filter.ToDictionary(
                keyValue => keyValue.Key, 
                keyValue => (IEnumerable<object>)keyValue.Value.Cast<object>().ToArray());
        }

        public static IDictionary<string, IEnumerable<object>> Convert(this IDictionary<string, long[]> filter)
        {
            return filter.Convert<long[], long>();
        }
    }
}
