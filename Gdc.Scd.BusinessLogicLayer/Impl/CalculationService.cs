using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CalculationService : ICalculationService
    {
        private readonly IRepositorySet repositorySet;

        private IRepository<HardwareCalculationResult> hwRepo;

        private IRepository<SoftwareCalculationResult> swRepo;

        public CalculationService(
                IRepositorySet repositorySet,
                IRepository<HardwareCalculationResult> hwRepo,
                IRepository<SoftwareCalculationResult> swRepo
            )
        {
            this.repositorySet = repositorySet;
            this.hwRepo = hwRepo;
            this.swRepo = swRepo;
        }

        public IEnumerable<HwCostDto> GetHardwareCost(HwFilterDto filter, int start, int limit, out int count)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SwCostDto> GetSoftwareCost(SwFilterDto filter, int start, int limit, out int count)
        {
            throw new System.NotImplementedException();
        }

        public void SaveHardwareCost(IEnumerable<HwCostManualDto> records)
        {
            var entities = hwRepo.GetAll()
                                 .Where(x => records.Any(y => y.Id == x.Id) &&
                                             x.Matrix.Country.CanOverrideTransferCostAndPrice)
                                 .ToDictionary(x => x.Id, y => y);

            ITransaction transaction = null;
            try
            {
                transaction = repositorySet.GetTransaction();

                foreach (var rec in records)
                {
                    if (entities.ContainsKey(rec.Id))
                    {
                        var e = entities[rec.Id];

                        e.ServiceTCManual = rec.ServiceTC;
                        e.ServiceTPManual = rec.ServiceTP;

                        hwRepo.Save(e);
                    }
                }

                repositorySet.Sync();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
