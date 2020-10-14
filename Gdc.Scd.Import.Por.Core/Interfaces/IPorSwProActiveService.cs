using Gdc.Scd.Core.Entities;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwProActiveService
    {
        bool UpdateProActiveSw(IEnumerable<SwDigit> digits, IEnumerable<ProActiveSw> proActiveSw);
    }
}
