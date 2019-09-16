using System.Collections.Generic;
using System.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

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

        public static IDictionary<string, long[]> Convert(this IDictionary<string, long> filter)
        {
            return filter.ToDictionary(keyValuePair => keyValuePair.Key, 
                keyValuePair => new long[] { keyValuePair.Value});
        }
     }
}
