using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities.CapabilityMatrix;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CapabilityMatrixService : ICapabilityMatrixService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<CapabilityMatrixRule> ruleRepo;

        private readonly IRepository<CapabilityMatrixAllowView> allowRepo;

        private readonly IRepository<CapabilityMatrixCountryAllowView> countryAllowRepo;

        public CapabilityMatrixService(
                IRepositorySet repositorySet,
                IRepository<CapabilityMatrixRule> ruleRepo,
                IRepository<CapabilityMatrixAllowView> allowRepo,
                IRepository<CapabilityMatrixCountryAllowView> countryAllowRepo
            )
        {
            this.repositorySet = repositorySet;
            this.ruleRepo = ruleRepo;
            this.allowRepo = allowRepo;
            this.countryAllowRepo = countryAllowRepo;
        }

        public Task AllowCombinations(long[] items)
        {
            return new DelMatrixRules(repositorySet).ExecuteAsync(items);
        }

        public Task DenyCombination(CapabilityMatrixRuleSetDto m)
        {
            return new AddMatrixRules(repositorySet).ExecuteAsync(m);
        }

        public Task<Tuple<CapabilityMatrixDto[], int>> GetAllowedCombinations(int start, int limit)
        {
            return GetAllowedCombinations(null, start, limit);
        }

        public Task<Tuple<CapabilityMatrixDto[], int>> GetAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit)
        {
            if (filter != null && filter.Country.HasValue)
            {
                return GetCountryAllowedCombinations(filter, start, limit);
            }

            var query = allowRepo.GetAll();

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

            var result = query.Select(x => new CapabilityMatrixDto
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
            });

            return result.PagingWithCountAsync(start, limit);
        }

        public Task<Tuple<CapabilityMatrixDto[], int>> GetCountryAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit, out int count)
        {
            var query = countryAllowRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Country.HasValue, x => x.CountryId == filter.Country.Value)
                             .WhereIf(filter.Wg.HasValue, x => x.WgId == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.AvailabilityId == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.DurationId == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionTypeId == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTimeId == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocationId == filter.ServiceLocation.Value);
            }

            query = query.OrderBy(x => x.Country);

            var result = query.Select(x => new CapabilityMatrixDto
            {
                Id = x.Id,

                Country = x.Country,
                Wg = x.Wg,
                Availability = x.Availability,
                Duration = x.Duration,
                ReactionType = x.ReactionType,
                ReactionTime = x.ReactionTime,
                ServiceLocation = x.ServiceLocation,
            });

            return result.PagingWithCount(start, limit, out count);
        }

        public IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations(int start, int limit, out int count)
        {
            return GetDeniedCombinations(null, start, limit, out count);
        }

        public IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations(CapabilityMatrixFilterDto filter, int start, int limit, out int count)
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

            var result = query.Select(x => new CapabilityMatrixRuleDto
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
            });

            return result.PagingWithCount(start, limit, out count);
        }
    }
}
