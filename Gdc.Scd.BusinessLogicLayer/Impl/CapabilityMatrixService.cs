using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities.CapabilityMatrix;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CapabilityMatrixService : ICapabilityMatrixService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<CapabilityMatrix> matrixRepo;

        private readonly IRepository<CapabilityMatrixMaster> matrixMasterRepo;

        private readonly IRepository<CapabilityMatrixRule> ruleRepo;

        public CapabilityMatrixService(
                IRepositorySet repositorySet,
                IRepository<CapabilityMatrixRule> ruleRepo,
                IRepository<CapabilityMatrix> matrixRepo,
                IRepository<CapabilityMatrixMaster> matrixMasterRepo
            )
        {
            this.repositorySet = repositorySet;
            this.ruleRepo = ruleRepo;
            this.matrixRepo = matrixRepo;
            this.matrixMasterRepo = matrixMasterRepo;
        }

        public Task AllowCombinations(long[] items)
        {
            return new DelMatrixRules(repositorySet).ExecuteAsync(items);
        }

        public Task DenyCombination(CapabilityMatrixRuleSetDto m)
        {
            var isValid = m.CountryId.HasValue || m.IsGlobalPortfolio || m.IsMasterPortfolio || m.IsCorePortfolio;

            if(!isValid)
            {
                throw new ArgumentException("No master or local portfolio specified!");
            }

            return new AddMatrixRules(repositorySet).ExecuteAsync(m);
        }

        public Task<Tuple<CapabilityMatrixDto[], int>> GetAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit)
        {
            if (filter != null && filter.Country.HasValue)
            {
                return GetCountryAllowedCombinations(filter.Country.Value, filter, start, limit);
            }
            else
            {
                return GetMasterAllowedCombinations(filter, start, limit);
            }
        }

        public async Task<Tuple<CapabilityMatrixDto[], int>> GetMasterAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit)
        {
            var query = matrixMasterRepo.GetAll().Where(x => !x.Denied);

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.Wg.Id == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.Duration.Id == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionType.Id == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTime.Id == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocation.Id == filter.ServiceLocation.Value)

                             .WhereIf(filter.IsGlobalPortfolio.HasValue && filter.IsGlobalPortfolio.Value, x => x.FujitsuGlobalPortfolio)
                             .WhereIf(filter.IsMasterPortfolio.HasValue && filter.IsMasterPortfolio.Value, x => x.MasterPortfolio)
                             .WhereIf(filter.IsCorePortfolio.HasValue   && filter.IsCorePortfolio.Value  , x => x.CorePortfolio);
            }

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new CapabilityMatrixDto
            {
                Id = x.Id,

                Wg = x.Wg.Name,
                Availability = x.Availability.Name,
                Duration = x.Duration.Name,
                ReactionType = x.ReactionType.Name,
                ReactionTime = x.ReactionTime.Name,
                ServiceLocation = x.ServiceLocation.Name,

                IsGlobalPortfolio = x.FujitsuGlobalPortfolio,
                IsMasterPortfolio = x.MasterPortfolio,
                IsCorePortfolio = x.CorePortfolio
            }).PagingAsync(start, limit);

            return new Tuple<CapabilityMatrixDto[], int>(result, count);
        }

        public async Task<Tuple<CapabilityMatrixDto[], int>> GetCountryAllowedCombinations(long country, CapabilityMatrixFilterDto filter, int start, int limit)
        {
            var query = matrixRepo.GetAll().Where(x => !x.Denied && x.Country.Id == country);

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.Wg.Id == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.Duration.Id == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionType.Id == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTime.Id == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocation.Id == filter.ServiceLocation.Value);
            }

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new CapabilityMatrixDto
            {
                Id = x.Id,

                Country = x.Country.Name,
                Wg = x.Wg.Name,
                Availability = x.Availability.Name,
                Duration = x.Duration.Name,
                ReactionType = x.ReactionType.Name,
                ReactionTime = x.ReactionTime.Name,
                ServiceLocation = x.ServiceLocation.Name,
            }).PagingAsync(start, limit);

            return new Tuple<CapabilityMatrixDto[], int>(result, count);
        }

        public async Task<Tuple<CapabilityMatrixRuleDto[], int>> GetDeniedCombinations(CapabilityMatrixFilterDto filter, int start, int limit)
        {
            var query = ruleRepo.GetAll();

            if (filter != null && filter.Country.HasValue)
            {
                query = query.Where(x => x.Country.Id == filter.Country.Value);
            }
            else
            {
                query = query.Where(x => x.Country == null);
            }

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.Wg.Id == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.Duration.Id == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionType.Id == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTime.Id == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocation.Id == filter.ServiceLocation.Value)
                             .WhereIf(filter.IsGlobalPortfolio.HasValue && filter.IsGlobalPortfolio.Value, x => x.FujitsuGlobalPortfolio)
                             .WhereIf(filter.IsMasterPortfolio.HasValue && filter.IsMasterPortfolio.Value, x => x.MasterPortfolio)
                             .WhereIf(filter.IsCorePortfolio.HasValue && filter.IsCorePortfolio.Value, x => x.CorePortfolio);
            }

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new CapabilityMatrixRuleDto
            {
                Id = x.Id,

                Country = x.Country == null ? null : x.Country.Name,
                Wg = x.Wg == null ? null : x.Wg.Name,
                Availability = x.Availability == null ? null : x.Availability.Name,
                Duration = x.Duration == null ? null : x.Duration.Name,
                ReactionType = x.ReactionType == null ? null : x.ReactionType.Name,
                ReactionTime = x.ReactionTime == null ? null : x.ReactionTime.Name,
                ServiceLocation = x.ServiceLocation == null ? null : x.ServiceLocation.Name,

                IsGlobalPortfolio = x.FujitsuGlobalPortfolio,
                IsMasterPortfolio = x.MasterPortfolio,
                IsCorePortfolio = x.CorePortfolio
            }).PagingAsync(start, limit);

            return new Tuple<CapabilityMatrixRuleDto[], int>(result, count);
        }
    }
}
