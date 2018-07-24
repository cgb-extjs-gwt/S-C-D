using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CapabilityMatrixService : ICapabilityMatrixService
    {
        public Task AllowCombination()
        {
            throw new NotImplementedException();
        }

        public Task DenyCombination()
        {
            throw new NotImplementedException();
        }

        public Task<object> GetAllowedCombinations()
        {
            throw new NotImplementedException();
        }

        public Task<object> GetDenyedCombinations()
        {
            throw new NotImplementedException();
        }
    }
}
