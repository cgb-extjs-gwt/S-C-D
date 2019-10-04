using Gdc.Scd.Core.Entities;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwSpMaintenaceService
    {
        bool Update2ndLevelSupportCosts(IEnumerable<SwDigit> digits, IEnumerable<SwSpMaintenance> swSpMaintenance);
    }
}
