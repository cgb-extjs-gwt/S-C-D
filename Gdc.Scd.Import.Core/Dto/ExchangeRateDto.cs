using Gdc.Scd.Import.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class ExchangeRateDto
    {
        [ParseInfo(2)]
        public string CurrencyCode { get; set; }

        [ParseInfo(3)]
        public double? ExchangeRate { get; set; } 
    }
}
