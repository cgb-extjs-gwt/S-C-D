using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.External.Por;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPorWgService
    {
        bool UploadWgs(IEnumerable<SCD2_WarrantyGroups> wgs,
            IEnumerable<SFab> sFabs,
            IEnumerable<Sog> sogs,
            IEnumerable<Pla> plas,
            DateTime modifiedDateTime, IEnumerable<string> softwareServiceTypes);

        bool DeactivateWgs(IEnumerable<SCD2_WarrantyGroups> sogs, DateTime modifiedDatetime);
    }
}
