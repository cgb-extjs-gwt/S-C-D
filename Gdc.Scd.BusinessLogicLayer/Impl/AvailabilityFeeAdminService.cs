using Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class AvailabilityFeeAdminService : IAvailabilityFeeAdminService
    {
        private const string GET_AVAILABILITY_FEE_PROCEDURE = "GetAvailabilityFeeCoverageCombination";

        private readonly IRepositorySet _repositorySet;

        private readonly IRepository<AdminAvailabilityFee> _availabilityFeeAdminRepo;

        public AvailabilityFeeAdminService(IRepositorySet repositorySet, 
            IRepository<AdminAvailabilityFee> availabilityFeeAdminRepo)
        {
            _repositorySet = repositorySet;
            _availabilityFeeAdminRepo = availabilityFeeAdminRepo;
        }

        public void ApplyAvailabilityFeeForSelectedCombination(AdminAvailabilityFeeDto model)
        {
            var newAvailabilityFee = new AdminAvailabilityFee
            {
                Country = new Country { Id = model.CountryId },
                ReactionTime = new ReactionTime { Id = model.ReactionTimeId },
                ReactionType = new ReactionType { Id = model.ReactionTypeId },
                ServiceLocation = new ServiceLocation { Id = model.ServiceLocatorId }
            };

            using (var transaction = _repositorySet.GetTransaction())
            {
                try
                {
                    _availabilityFeeAdminRepo.Save(newAvailabilityFee);
                    _repositorySet.Sync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    throw ex;
                }
            }
        }

        public Task<List<AdminAvailabilityFeeDto>> GetAllCombinations()
        {
            return _repositorySet.ExecuteProcAsync<AdminAvailabilityFeeDto>(GET_AVAILABILITY_FEE_PROCEDURE);
        }

        public void RemoveCombination(long id)
        {
            using (var transaction = _repositorySet.GetTransaction())
            {
                try
                {
                    _availabilityFeeAdminRepo.Delete(id);
                    _repositorySet.Sync();
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();

                    throw ex;
                }
            }
                  
        }
    }
}
