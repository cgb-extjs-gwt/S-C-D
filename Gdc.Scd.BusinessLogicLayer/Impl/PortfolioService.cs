using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<LocalPortfolio> localRepo;

        private readonly IRepository<PrincipalPortfolio> principalRepo;

        private readonly IUserService userService;

        public PortfolioService(
                IRepositorySet repositorySet,
                IRepository<LocalPortfolio> localRepo,
                IRepository<PrincipalPortfolio> principalRepo,
                IUserService userService
            )
        {
            this.repositorySet = repositorySet;
            this.localRepo = localRepo;
            this.principalRepo = principalRepo;
            this.userService = userService;
        }

        public Task Allow(PortfolioRuleSetDto m)
        {
            return UpdatePortfolio(m, false);
        }

        public Task Deny(PortfolioRuleSetDto m)
        {
            return UpdatePortfolio(m, true);
        }

        public Task Deny(long countryId, long[] ids)
        {
            return new UpdateLocalPortfolio(repositorySet).DenyAsync(countryId, ids);
        }

        public Task<(PortfolioDto[] items, int total)> GetAllowed(PortfolioFilterDto filter, int start, int limit)
        {
            var userCountriesIds = this.userService.GetCurrentUserCountries().Select(country => country.Id).ToArray();

            if (filter != null && filter.Country.HasValue)
            {
                return GetLocalAllowed(filter.Country.Value, filter, start, limit);
            }
            else if (userCountriesIds.Length > 0)
            {
                return GetLocalAllowed(userCountriesIds.FirstOrDefault(), filter, start, limit);
            }
            else
            {
                return GetPrincipalAllowed(filter, start, limit);
            }
        }

        public async Task<(PortfolioDto[] items, int total)> GetPrincipalAllowed(PortfolioFilterDto filter, int start, int limit)
        {
            var query = principalRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.Wg.Id == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.Duration.Id == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionType.Id == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTime.Id == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocation.Id == filter.ServiceLocation.Value)
                             .WhereIf(filter.ProActive.HasValue, x => x.ProActiveSla.Id == filter.ProActive.Value)
                             .WhereIf(filter.IsGlobalPortfolio.HasValue && filter.IsGlobalPortfolio.Value, x => x.IsGlobalPortfolio)
                             .WhereIf(filter.IsMasterPortfolio.HasValue && filter.IsMasterPortfolio.Value, x => x.IsMasterPortfolio)
                             .WhereIf(filter.IsCorePortfolio.HasValue && filter.IsCorePortfolio.Value, x => x.IsCorePortfolio);
            }

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new PortfolioDto
            {
                Id = x.Id,

                Wg = x.Wg.Name,
                Availability = x.Availability.Name,
                Duration = x.Duration.Name,
                ReactionType = x.ReactionType.Name,
                ReactionTime = x.ReactionTime.Name,
                ServiceLocation = x.ServiceLocation.Name,
                ProActive = x.ProActiveSla.ExternalName,

                IsGlobalPortfolio = x.IsGlobalPortfolio,
                IsMasterPortfolio = x.IsMasterPortfolio,
                IsCorePortfolio = x.IsCorePortfolio
            }).PagingAsync(start, limit);

            return (result, count);
        }

        public async Task<(PortfolioDto[] items, int total)> GetLocalAllowed(long country, PortfolioFilterDto filter, int start, int limit)
        {
            var query = localRepo.GetAll().Where(x => x.Country.Id == country);

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.Wg.Id == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.Duration.Id == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionType.Id == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTime.Id == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocation.Id == filter.ServiceLocation.Value)
                             .WhereIf(filter.ProActive.HasValue, x => x.ProActiveSla.Id == filter.ProActive.Value);
            }

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new PortfolioDto
            {
                Id = x.Id,

                Country = x.Country.Name,
                Wg = x.Wg.Name,
                Availability = x.Availability.Name,
                Duration = x.Duration.Name,
                ReactionType = x.ReactionType.Name,
                ReactionTime = x.ReactionTime.Name,
                ServiceLocation = x.ServiceLocation.Name,
                ProActive = x.ProActiveSla.ExternalName
            }).PagingAsync(start, limit);

            return (result, count);
        }

        private Task UpdatePortfolio(PortfolioRuleSetDto m, bool deny)
        {
            if (m == null)
            {
                throw new ArgumentNullException("Null portfolio!");
            }

            if (!m.IsValid())
            {
                throw new ArgumentException("No portfolio or SLA specified!");
            }

            if (m.IsLocalPortfolio())
            {
                return new UpdateLocalPortfolio(repositorySet).UpdateAsync(m, deny);
            }
            else
            {
                return new UpdatePrincipalPortfolio(repositorySet).UpdateAsync(m, deny);
            }
        }
    }
}
