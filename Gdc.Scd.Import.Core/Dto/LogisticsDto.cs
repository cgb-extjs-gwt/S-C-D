using Gdc.Scd.Import.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class LogisticsDto
    {
        [ParseInfo(0)]
        public string WgCode { get; set; }

        [ParseInfo(1)]
        public string WgDescription { get; set; }

        [ParseInfo(2)]
        public double? CostPerKit { get; set; }

        [ParseInfo(3, Format = Enums.Format.Number)]
        public int? MaxQty { get; set; }

        [ParseInfo(4)]
        public string Action { get; set; }

        [ParseInfo(5)]
        public string WgType { get; set; }

        [ParseInfo(6)]
        public string Pla { get; set; }

        [ParseInfo(7)]
        public string IsJapanCostPerKit { get; set; }

        public bool IsMultiVendor
        {
            get
            {
                return WgType.Equals("s", StringComparison.OrdinalIgnoreCase);
            }
        }

        public long? PlaId { get; set; }
    }
}
