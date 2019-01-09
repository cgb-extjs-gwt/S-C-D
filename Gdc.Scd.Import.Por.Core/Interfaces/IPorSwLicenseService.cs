using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwLicenseService
    {
        bool UploadSwLicense(IEnumerable<SCD2_SW_Overview> swInfo,
            DateTime modifiedDateTime, List<UpdateQueryOption> updateOptions);

        bool Deactivate(IEnumerable<SCD2_SW_Overview> swInfo, DateTime modifiedDateTime);
    }
}
