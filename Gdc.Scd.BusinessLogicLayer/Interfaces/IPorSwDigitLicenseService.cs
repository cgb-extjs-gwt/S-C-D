using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPorSwDigitLicenseService
    {
        bool UploadSwDigitAndLicenseRelation(IEnumerable<SwLicense> licenses,
            IEnumerable<SwDigit> digits,
            IEnumerable<SCD2_SW_Overview> swInfo, DateTime created);
    }
}
