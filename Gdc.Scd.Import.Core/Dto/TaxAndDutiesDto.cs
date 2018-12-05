using Gdc.Scd.Import.Core.Attributes;
using Gdc.Scd.Import.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class TaxAndDutiesDto
    {
        [ParseInfo(0)]
        public string Region { get; set; }

        [ParseInfo(1)]
        public string CountryGroup { get; set; }

        [ParseInfo(3)]
        public string Country { get; set; }

        [ParseInfo(2)]
        public string ISO3Code { get; set; }

        [ParseInfo(4, Format = Format.Percentage)]
        public double? AverageSumDutiesAndTaxes { get; set; }
    }
}
