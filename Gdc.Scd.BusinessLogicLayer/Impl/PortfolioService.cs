using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class PortfolioService : IPortfolioService
    {
        private CacheDomainService cache;

        private readonly IRepositorySet repositorySet;

        private readonly IRepository<LocalPortfolio> localRepo;

        private readonly IRepository<PrincipalPortfolio> principalRepo;

        private readonly IRepository<PortfolioHistory> historyRepo;

        private readonly IUserService userService;

        private readonly IWgService wgService;

        private readonly IEmailService emailService;

        public PortfolioService(
                IRepositorySet repositorySet,
                IRepository<LocalPortfolio> localRepo,
                IRepository<PrincipalPortfolio> principalRepo,
                IRepository<PortfolioHistory> historyRepo,
                IUserService userService,
                IWgService wgService,
                IEmailService emailService)
        {
            this.repositorySet = repositorySet;
            this.localRepo = localRepo;
            this.principalRepo = principalRepo;
            this.historyRepo = historyRepo;
            this.userService = userService;
            this.wgService = wgService;
            this.emailService = emailService;
        }

        public bool CanEdit(User usr, PortfolioRuleSetDto m)
        {
            return m.IsLocalPortfolio() || userService.HasPermission(usr.Login, PermissionConstants.Portfolio);
        }

        public void Allow(PortfolioRuleSetDto m)
        {
            UpdatePortfolio(userService.GetCurrentUser(), m, false);
        }

        public Task Deny(PortfolioRuleSetDto m)
        {
            UpdatePortfolio(userService.GetCurrentUser(), m, true);

            return Task.CompletedTask;
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
                             .WhereIf(filter.ProActiveSla != null, x => filter.ProActiveSla.Contains(x.ProActiveSla.Id))
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
                             .WhereIf(filter.ProActiveSla != null, x => filter.ProActiveSla.Contains(x.ProActiveSla.Id));
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

        public async Task<(PortfolioHistoryDto[] items, int total)> GetHistory(int start, int limit)
        {
            var query = historyRepo.GetAll();

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new PortfolioHistoryDto
            {
                EditUser = x.EditUser.Name + "[" + x.EditUser.Email + "]",
                EditDate = x.EditDate,
                Deny = x.Deny,
                Country = x.Country == null ? null : x.Country.Name,
                Json = x.Rules

            })
            .OrderByDescending(x => x.EditDate)
            .PagingAsync(start, limit);

            return (result, count);
        }

        public void NotifyCountryUsers(long[] wgIds, string portfolioUrl)
        {
            var wgs = this.wgService.GetAll().Where(wg => wgIds.Contains(wg.Id)).ToArray();
            var users = this.userService.GetCountryKeyUsers().ToArray();

            this.emailService.SendPortfolioNotifications(wgs, users, portfolioUrl);

            foreach (var wg in wgs)
            {
                wg.IsNotified = true;
            }

            this.wgService.Save(wgs);
        }

        private void UpdatePortfolio(User changeUser, PortfolioRuleSetDto m, bool deny)
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
                new UpdateLocalPortfolio(repositorySet).Update(m, deny);
            }
            else
            {
                new UpdatePrincipalPortfolio(repositorySet).Update(m, deny);
            }
            //
            Log(changeUser, new PortfolioRuleSetDto[] { m }, deny);
        }

        private async Task DenyById(User changeUser, long[] ids)
        {
            var rules = await localRepo.GetAll()
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
            Log(changeUser, rules, true);
        }

        private void Log(User changeUser, PortfolioRuleSetDto[] rules, bool deny)
        {
            for (var i = 0; i < rules.Length; i++)
            {
                var r = rules[i];
                var p = AsHistoryRule(r);
                var h = new PortfolioHistory
                {
                    Deny = deny,
                    CountryId = r.CountryId,
                    EditDate = DateTime.Now,
                    EditUser = changeUser,
                    Rules = p.AsJson()
                };
                historyRepo.Save(h);
            }
            repositorySet.Sync();
        }

        private PortfolioHistroryRuleDto AsHistoryRule(PortfolioRuleSetDto dto)
        {
            if (cache == null)
            {
                cache = new CacheDomainService(repositorySet);
            }

            var p = new PortfolioHistroryRuleDto();

            if (dto.IsLocalPortfolio())
            {
                p.Country = cache.GetName<Country>(dto.CountryId.Value);
            }
            else
            {
                p.IsCorePortfolio = dto.IsCorePortfolio;
                p.IsGlobalPortfolio = dto.IsGlobalPortfolio;
                p.IsMasterPortfolio = dto.IsMasterPortfolio;
            }

            p.Wgs = cache.GetNames<Wg>(dto.Wgs);
            p.Availabilities = cache.GetNames<Availability>(dto.Availabilities);
            p.Durations = cache.GetNames<Duration>(dto.Durations);
            p.ReactionTimes = cache.GetNames<ReactionTime>(dto.ReactionTimes);
            p.ReactionTypes = cache.GetNames<ReactionType>(dto.ReactionTypes);
            p.ServiceLocations = cache.GetNames<ServiceLocation>(dto.ServiceLocations);
            p.ProActives = cache.GetExtNames<ProActiveSla>(dto.ProActives);

            return p;
        }
    }
}
