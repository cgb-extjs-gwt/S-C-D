using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockService
    {
        Task UpdateByCoordinates(IEnumerable<CostBlockEntityMeta> costBlockMetas);

        Task UpdateByCoordinates();
    }
}
