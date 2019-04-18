using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwSpMaintenaceService
    {
        bool Update2ndLevelSupportCosts(IQueryable<SwDigit> digits, IQueryable<SwSpMaintenance> swSpMaintenance);
    }
}
