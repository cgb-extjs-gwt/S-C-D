using Gdc.Scd.Import.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class CentralContractGroupDto
    {
        [ParseInfo(0)]
        public string WgName { get; set; }

        [ParseInfo(2)]
        public string CentralContractGroupCode { get; set; }

        [ParseInfo(3)]
        public string CentralContractGroupName { get; set; }
    }
}
