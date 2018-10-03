using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System.Collections.Generic;


namespace Gdc.Scd.Import.Por.Models
{
    public class SwHelperModel
    {
        public IDictionary<string, SCD2_SW_Overview> SwDigits { get; set; }
        public IDictionary<string, SCD2_SW_Overview> SwLicenses { get; set; }

        public SwHelperModel(IDictionary<string, SCD2_SW_Overview> swDigits, 
            IDictionary<string, SCD2_SW_Overview> swLicenses)
        {
            this.SwDigits = swDigits;
            this.SwLicenses = swLicenses;
        }
    }
}
