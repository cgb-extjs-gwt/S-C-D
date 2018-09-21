using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPorSwLicenseService
    {
        bool UploadSwLicense(IEnumerable<SCD_SW_Overview> swInfo, IEnumerable<SwDigit> digits,
            DateTime modifiedDateTime);

        bool Deactivate(IEnumerable<SCD_SW_Overview> swInfo, DateTime modifiedDateTime);
    }
}
