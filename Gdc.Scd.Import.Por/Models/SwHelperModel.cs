using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Models
{
    public class SwHelperModel
    {
        public IDictionary<string, string> SwDigits { get; set; }
        public IDictionary<string, SCD_SW_Overview> SwLicenses { get; set; }

        public SwHelperModel(IDictionary<string, string> swDigits, 
            IDictionary<string, SCD_SW_Overview> swLicenses)
        {
            this.SwDigits = swDigits;
            this.SwLicenses = swLicenses;
        }
    }
}
