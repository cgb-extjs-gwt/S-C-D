using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwDigitLicenseService
    {
        bool UploadSwDigitAndLicenseRelation(IEnumerable<SwLicense> licenses,
            IEnumerable<SwDigit> digits,
            IEnumerable<SCD2_SW_Overview> swInfo, DateTime created);
    }
}
