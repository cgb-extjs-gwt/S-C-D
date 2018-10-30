using Gdc.Scd.Import.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class InstallBaseDto
    {
        [ParseInfo(0)]
        public string CountryCode { get; set; }

        [ParseInfo(1)]
        public string Wg { get; set; }

        [ParseInfo(2)]
        public double? InstallBase { get; set; }
    }
}
