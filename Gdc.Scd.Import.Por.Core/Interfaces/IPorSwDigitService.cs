using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwDigitService
    {
        (bool ok, List<SwDigit> added) UploadSwDigits(IDictionary<string, SCD2_SW_Overview> swInfo, IEnumerable<Sog> sogs, 
            DateTime modifiedDateTime, List<UpdateQueryOption> updateOptions);

        bool Deactivate(IDictionary<string, SCD2_SW_Overview> swInfo, DateTime modifiedDateTime);
    }
}
