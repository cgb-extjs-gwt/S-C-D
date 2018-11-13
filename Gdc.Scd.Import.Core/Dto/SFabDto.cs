using Gdc.Scd.Import.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class SFabDto
    {
        [ParseInfo(0)]
        public string WarrantyGroup { get; set; }

        [ParseInfo(1)]
        public string Sfab { get; set; }
    }
}
