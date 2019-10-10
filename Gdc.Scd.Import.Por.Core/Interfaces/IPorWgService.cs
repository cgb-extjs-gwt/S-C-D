using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorWgService
    {
        bool UploadWgs(IEnumerable<SCD2_WarrantyGroups> wgs,
            IEnumerable<Sog> sogs,
            IEnumerable<Pla> plas,
            DateTime modifiedDateTime, IEnumerable<string> softwareServiceTypes, List<UpdateQueryOption> updateOptions);

        bool DeactivateWgs(IEnumerable<SCD2_WarrantyGroups> sogs, DateTime modifiedDatetime);
    }
}
