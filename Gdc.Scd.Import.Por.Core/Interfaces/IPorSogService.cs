using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSogService
    {
        bool UploadSogs(IEnumerable<SCD2_ServiceOfferingGroups> sogs, 
            IEnumerable<Pla> plas,
            DateTime modifiedDate, IEnumerable<string> softwareServiceTypes, 
            List<UpdateQueryOption> updateOptions, string solutionIdentifier);

        bool DeactivateSogs(IEnumerable<SCD2_ServiceOfferingGroups> sogs, DateTime modifiedDatetime);
    }
}
