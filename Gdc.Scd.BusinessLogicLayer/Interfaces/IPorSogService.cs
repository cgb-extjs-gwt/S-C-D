using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.External.Por;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPorSogService
    {
        bool UploadSogs(IEnumerable<SCD2_ServiceOfferingGroups> sogs, 
            IEnumerable<Pla> plas,
            IEnumerable<SFab> sFabs,
            DateTime modifiedDate);

        bool DeactivateSogs(IEnumerable<SCD2_ServiceOfferingGroups> sogs, DateTime modifiedDatetime);
    }
}
