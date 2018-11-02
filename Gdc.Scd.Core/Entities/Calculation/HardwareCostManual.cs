using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Entities.Calculation
{
    class HardwareCostManual
    {
        public long Id { get; set; }

        public double? ServiceTC { get; set; }

        public double? ServiceTP { get; set; }

        public double? ListPrice { get; set; }

        public double? DealerDiscount { get; set; }
    }
}
