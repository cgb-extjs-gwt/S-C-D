using System.Collections.Generic;

namespace Gdc.Scd.Core.Helpers
{
    public static class LinqExtensions
    {
        public static IEnumerable<TItems> Concat<TItems, T1>(this IEnumerable<TItems> items, T1 addedItem)
            where T1 : TItems
        {
            foreach (var item in items)
            {
                yield return item;
            }

            yield return addedItem;
        }

        public static IEnumerable<TItems> Concat<TItems, T1, T2>(this IEnumerable<TItems> items, T1 addedItem1, T2 addedItem2)
            where T1 : TItems
            where T2 : TItems
        {
            foreach (var item in items)
            {
                yield return item;
            }

            yield return addedItem1;
            yield return addedItem2;
        }

        public static IEnumerable<TItems> Concat<TItems, T1, T2, T3>(
            this IEnumerable<TItems> items,
            T1 addedItem1,
            T2 addedItem2,
            T3 addedItem3)
            where T1 : TItems
            where T2 : TItems
            where T3 : TItems
        {
            foreach (var item in items)
            {
                yield return item;
            }

            yield return addedItem1;
            yield return addedItem2;
            yield return addedItem3;
        }

        public static IEnumerable<TItems> Concat<TItems, T1, T2, T3, T4>(
            this IEnumerable<TItems> items,
            T1 addedItem1,
            T2 addedItem2,
            T3 addedItem3,
            T4 addedItem4)
            where T1 : TItems
            where T2 : TItems
            where T3 : TItems
            where T4 : TItems
        {
            foreach (var item in items)
            {
                yield return item;
            }

            yield return addedItem1;
            yield return addedItem2;
            yield return addedItem3;
            yield return addedItem4;
        }
    }
}
