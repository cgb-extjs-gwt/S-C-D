using Gdc.Scd.Import.Core.Attributes;
using Gdc.Scd.Import.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class MaterialCostDto
    {
        [ParseInfo(0)]
        public string Period { get; set; }
        [ParseInfo(1, Format = Format.Number)]
        public int? Year { get; set; }
        [ParseInfo(2)]
        public double? MaterialCost { get; set; }
        [ParseInfo(3)]
        public string Currency { get; set; }
        [ParseInfo(4)]
        public string Wg { get; set; }
    }
}
