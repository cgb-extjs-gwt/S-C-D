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

        private readonly IRepository<CapabilityMatrixRule> ruleRepo;

        public CapabilityMatrixService(
                IRepositorySet repositorySet,
                IRepository<CapabilityMatrixRule> ruleRepo,
                IRepository<CapabilityMatrix> matrixRepo
            )
        {
            this.repositorySet = repositorySet;
            this.ruleRepo = ruleRepo;
            this.matrixRepo = matrixRepo;
        }

        public Task AllowCombinations(long[] items)
        {
            return new DelMatrixRules(repositorySet).ExecuteAsync(items);
        }

        public Task DenyCombination(CapabilityMatrixRuleSetDto m)
        {
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
            var query = GetAllowed().Where(x => x.CountryId == null);

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.WgId == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.AvailabilityId == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.DurationId == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionTypeId == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTimeId == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocationId == filter.ServiceLocation.Value)
                             .WhereIf(filter.IsGlobalPortfolio.HasValue && filter.IsGlobalPortfolio.Value, x => x.FujitsuGlobalPortfolio)
                             .WhereIf(filter.IsMasterPortfolio.HasValue && filter.IsMasterPortfolio.Value, x => x.MasterPortfolio)
                             .WhereIf(filter.IsCorePortfolio.HasValue && filter.IsCorePortfolio.Value, x => x.CorePortfolio);
            }

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new CapabilityMatrixDto
            {
                Id = x.Id,

                Wg = x.Wg,
                Availability = x.Availability,
                Duration = x.Duration,
                ReactionType = x.ReactionType,
                ReactionTime = x.ReactionTime,
                ServiceLocation = x.ServiceLocation,

                IsGlobalPortfolio = x.FujitsuGlobalPortfolio,
                IsMasterPortfolio = x.MasterPortfolio,
                IsCorePortfolio = x.CorePortfolio
            }).PagingAsync(start, limit);

            return new Tuple<CapabilityMatrixDto[], int>(result, count);
        }

        public async Task<Tuple<CapabilityMatrixDto[], int>> GetCountryAllowedCombinations(long country, CapabilityMatrixFilterDto filter, int start, int limit)
        {
            var query = GetAllowed().Where(x => x.CountryId == country);

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.WgId == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.AvailabilityId == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.DurationId == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionTypeId == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTimeId == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocationId == filter.ServiceLocation.Value);
            }

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new CapabilityMatrixDto
            {
                Id = x.Id,

                Country = x.Country,
                Wg = x.Wg,
                Availability = x.Availability,
                Duration = x.Duration,
                ReactionType = x.ReactionType,
                ReactionTime = x.ReactionTime,
                ServiceLocation = x.ServiceLocation,
            }).PagingAsync(start, limit);

            return new Tuple<CapabilityMatrixDto[], int>(result, count);
        }

        public async Task<Tuple<CapabilityMatrixRuleDto[], int>> GetDeniedCombinations(CapabilityMatrixFilterDto filter, int start, int limit)
        {
            var query = ruleRepo.GetAll();

            if (filter != null && filter.Country.HasValue)
            {
                query = query.Where(x => x.Country.Id == filter.Country.Value);
                query = query.OrderBy(x => x.Country.Name);
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

        private IQueryable<CapabilityMatrix> GetAllowed()
        {
            return matrixRepo.GetAll().Where(x => !x.Denied);
        }
    }
}
