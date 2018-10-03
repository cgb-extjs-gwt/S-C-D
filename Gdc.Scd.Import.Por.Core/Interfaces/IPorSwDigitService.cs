using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwDigitService
    {
        bool UploadSwDigits(IDictionary<string, SCD2_SW_Overview> swInfo, IEnumerable<Sog> sogs, 
            DateTime modifiedDateTime);

        bool Deactivate(IDictionary<string, SCD2_SW_Overview> swInfo, DateTime modifiedDateTime);
    }
}
