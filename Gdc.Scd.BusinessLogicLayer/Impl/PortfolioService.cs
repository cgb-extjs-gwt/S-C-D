using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities;
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
        private const string PORTFOLIO_ROLE = "Portfolio";

        private readonly IRepositorySet repositorySet;

        private readonly IRepository<LocalPortfolio> localRepo;

        private readonly IRepository<PrincipalPortfolio> principalRepo;

        private readonly IRepository<PortfolioHistory> historyRepo;

        private readonly IUserService userService;

        public PortfolioService(
                IRepositorySet repositorySet,
                IRepository<LocalPortfolio> localRepo,
                IRepository<PrincipalPortfolio> principalRepo,
                IRepository<PortfolioHistory> historyRepo,
                IUserService userService
            )
        {
            this.repositorySet = repositorySet;
            this.localRepo = localRepo;
            this.principalRepo = principalRepo;
            this.historyRepo = historyRepo;
            this.userService = userService;
        }

        public Task Allow(PortfolioRuleSetDto m)
        {
            return UpdatePortfolio(userService.GetCurrentUser(), m, false);
        }

        public Task Deny(PortfolioRuleSetDto m)
        {
            return UpdatePortfolio(userService.GetCurrentUser(), m, true);
        }

        public Task Deny(long[] countryId, long[] ids)
        {
            return DenyById(userService.GetCurrentUser(), ids);
        }

        public Task<(PortfolioDto[] items, int total)> GetAllowed(PortfolioFilterDto filter, int start, int limit)
        {
            var userCountriesIds = this.userService.GetCurrentUserCountries().Select(country => country.Id).ToArray();

            if (filter != null && filter.Country != null)
            {
                return GetLocalAllowed(filter.Country, filter, start, limit);
            }
            else if (userCountriesIds.Length > 0)
            {
                return GetLocalAllowed(userCountriesIds, filter, start, limit);
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
                query = query.WhereIf(filter.Wg != null, x => filter.Wg.Contains(x.Wg.Id))
                             .WhereIf(filter.Availability != null, x => filter.Availability.Contains(x.Availability.Id))
                             .WhereIf(filter.Duration != null, x => filter.Duration.Contains(x.Duration.Id))
                             .WhereIf(filter.ReactionType != null, x => filter.ReactionType.Contains(x.ReactionType.Id))
                             .WhereIf(filter.ReactionTime != null, x => filter.ReactionTime.Contains(x.ReactionTime.Id))
                             .WhereIf(filter.ServiceLocation != null, x => filter.ServiceLocation.Contains(x.ServiceLocation.Id))
                             .WhereIf(filter.ProActive != null, x => filter.ProActive.Contains(x.ProActiveSla.Id))
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

        public async Task<(PortfolioDto[] items, int total)> GetLocalAllowed(long[] countries, PortfolioFilterDto filter, int start, int limit)
        {
            var query = localRepo.GetAll().Where(x => countries.Contains(x.Country.Id));

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg != null, x => filter.Wg.Contains(x.Wg.Id))
                             .WhereIf(filter.Availability != null, x => filter.Availability.Contains(x.Availability.Id))
                             .WhereIf(filter.Duration != null, x => filter.Duration.Contains(x.Duration.Id))
                             .WhereIf(filter.ReactionType != null, x => filter.ReactionType.Contains(x.ReactionType.Id))
                             .WhereIf(filter.ReactionTime != null, x => filter.ReactionTime.Contains(x.ReactionTime.Id))
                             .WhereIf(filter.ServiceLocation != null, x => filter.ServiceLocation.Contains(x.ServiceLocation.Id))
                             .WhereIf(filter.ProActive != null, x => filter.ProActive.Contains(x.ProActiveSla.Id));
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

        private async Task UpdatePortfolio(User changeUser, PortfolioRuleSetDto m, bool deny)
        {
            if (m == null)
            {
                throw new ArgumentNullException("Null portfolio!");
            }

            if (!m.IsValid())
            {
                throw new ArgumentException("No portfolio or SLA specified!");
            }

            if (!CanEdit(changeUser, m))
            {
                throw new ArgumentException("Illegal access. User does not have <portfolio> role");
            }

            if (m.IsLocalPortfolio())
            {
                await new UpdateLocalPortfolio(repositorySet).UpdateAsync(m, deny);
            }
            else
            {
                await new UpdatePrincipalPortfolio(repositorySet).UpdateAsync(m, deny);
            }
            //
            Log(changeUser, new PortfolioRuleSetDto[] { m }, deny);
        }

        private async Task DenyById(User changeUser, long[] ids)
        {
            var ruleSet = await localRepo.GetAll()
                                       .Where(x => ids.Contains(x.Id))
                                       .Select(x => new PortfolioRuleSetDto
                                       {
                                           CountryId = x.Country.Id,
                                           Wgs = new long[] { x.Wg.Id },
                                           Availabilities = new long[] { x.Availability.Id },
                                           Durations = new long[] { x.Duration.Id },
                                           ReactionTypes = new long[] { x.ReactionType.Id },
                                           ReactionTimes = new long[] { x.ReactionTime.Id },
                                           ServiceLocations = new long[] { x.ServiceLocation.Id },
                                           ProActives = new long[] { x.ProActiveSla.Id }
                                       })
                                       .GetAsync();

            await new UpdateLocalPortfolio(repositorySet).DenyAsync(ids);
            //
            Log(changeUser, ruleSet, true);
        }

        private void Log(User changeUser, PortfolioRuleSetDto[] ruleSet, bool deny)
        {
            for (var i = 0; i < ruleSet.Length; i++)
            {
                var r = ruleSet[i];
                var h = new PortfolioHistory
                {
                    Deny = deny,
                    CountryId = r.CountryId,
                    EditDate = DateTime.Now,
                    EditUser = changeUser,
                    RuleSet = r.AsJson()
                };
                historyRepo.Save(h);
            }
            repositorySet.Sync();
        }

        public bool CanEdit(User usr, PortfolioRuleSetDto m)
        {
            return m.IsLocalPortfolio() || userService.HasRole(usr.Login, PORTFOLIO_ROLE);
        }
    }
}
