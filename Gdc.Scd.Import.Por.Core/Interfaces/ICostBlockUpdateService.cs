using Gdc.Scd.Core.Entities;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface ICostBlockUpdateService
    {
        void UpdateByPla(List<Wg> wgs);
    }
}
