using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    class ServiceCostDto
    {
        public string Country { get; set; }
        public string FspCode { get; set; }
        public double ServiceTC { get; set; }
        public double ServiceTP { get; set; }
        public double ServiceTP_MonthlyYear1 { get; set; }
        public double ServiceTP_MonthlyYear2 { get; set; }
        public double ServiceTP_MonthlyYear3 { get; set; }
        public double ServiceTP_MonthlyYear4 { get; set; }
        public double ServiceTP_MonthlyYear5 { get; set; }
    }
}
