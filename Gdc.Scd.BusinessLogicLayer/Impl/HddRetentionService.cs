using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class HddRetentionService : IHddRetentionService
    {
        public HddRetentionService()
        {

        }

        public Task<(HddRetentionDto[] items, int total)> GetCost(bool approved, object filter, int start, int limit)
        {
            var items = new HddRetentionDto[] {
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                    new HddRetentionDto { Wg = "tst", ListPrice = 1005.0 },
                };

            return Task.FromResult((items, items.Length));
        }
    }
}
