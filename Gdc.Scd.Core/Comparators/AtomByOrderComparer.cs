using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Comparators
{
    public class AtomByOrderComparer : IComparer<ISortable>
    {
        public int Compare(ISortable x, ISortable y)
        {
            return x.Order.CompareTo(y.Order);
        }
    }
}
