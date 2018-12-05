using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorWgService
    {
        bool UploadWgs(IEnumerable<SCD2_WarrantyGroups> wgs,
            IEnumerable<Sog> sogs,
            IEnumerable<Pla> plas,
            DateTime modifiedDateTime, IEnumerable<string> softwareServiceTypes);

        bool DeactivateWgs(IEnumerable<SCD2_WarrantyGroups> sogs, DateTime modifiedDatetime);
    }
}
