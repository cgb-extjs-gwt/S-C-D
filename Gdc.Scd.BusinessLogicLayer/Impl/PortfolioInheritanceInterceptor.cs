using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class PortfolioInheritanceInterceptor : IAfterAddingInterceptor<Wg>
    {
        private readonly IPortfolioRepository<PrincipalPortfolio, PrincipalPortfolioInheritance> principalPortfolioRepository;

        private readonly IPortfolioRepository<LocalPortfolio, LocalPortfolioInheritance> localPortfolioRepository;

        private readonly IPortfolioService portfolioService;

        public PortfolioInheritanceInterceptor(
            IPortfolioRepository<PrincipalPortfolio, PrincipalPortfolioInheritance> principalPortfolioRepository,
            IPortfolioRepository<LocalPortfolio, LocalPortfolioInheritance> localPortfolioRepository,
            IPortfolioService portfolioService)
        {
            this.principalPortfolioRepository = principalPortfolioRepository;
            this.localPortfolioRepository = localPortfolioRepository;
            this.portfolioService = portfolioService;
        }

        public void Handle(Wg[] wgs)
        {
            var task = this.HandlAsync(wgs);

            task.Wait();
        }

        private async Task HandlAsync(Wg[] wgs)
        {
            var wgGroups = 
                wgs.GroupBy(wg => wg.PlaId)
                   .ToDictionary(
                        group => group.Key, 
                        group => group.Select(wg => wg.Id).ToArray());

            var plaIds = wgGroups.Keys.ToArray();

            var principalInheritances = await this.principalPortfolioRepository.GetInheritanceItems(plaIds);
            var localInheritances = await this.localPortfolioRepository.GetInheritanceItems(plaIds);

            var rules =
                principalInheritances.Select(BuildPrincipalRule)
                                     .Concat(localInheritances.Select(BuildLocalRule))
                                     .GroupBy(rule => new
                                     {
                                         rule.CountryId,
                                         rule.IsCorePortfolio,
                                         rule.IsGlobalPortfolio,
                                         rule.IsMasterPortfolio,
                                         Availability = rule.Availabilities[0],
                                         Duration = rule.Durations[0],
                                         ProActive = rule.ProActives[0],
                                         ReactionTime = rule.ReactionTimes[0],
                                         ReactionType = rule.ReactionTypes[0],
                                         Wgs = string.Join("_", rule.Wgs)
                                     })
                                     .Select(group => new PortfolioRuleSetDto
                                     {
                                         Availabilities = new[] { group.Key.Availability },
                                         CountryId = group.Key.CountryId,
                                         Durations = new[] { group.Key.Duration },
                                         IsCorePortfolio = group.Key.IsCorePortfolio,
                                         IsGlobalPortfolio = group.Key.IsGlobalPortfolio,
                                         IsMasterPortfolio = group.Key.IsMasterPortfolio,
                                         ProActives = new[] { group.Key.ProActive },
                                         ReactionTimes = new[] { group.Key.ReactionTime },
                                         ReactionTypes = new[] { group.Key.ReactionType },
                                         Wgs = group.Key.Wgs.Split('_').Select(value => long.Parse(value)).ToArray(),
                                         ServiceLocations = group.SelectMany(rule => rule.ServiceLocations).ToArray()
                                     })
                                     .ToArray();

            foreach (var rule in rules)
            {
                await this.portfolioService.Allow(rule);
            }

            PortfolioRuleSetDto BuildRule(BasePortfolioInheritance item)
            {
                return new PortfolioRuleSetDto
                {
                    Availabilities = new[] { item.AvailabilityId },
                    Durations = new[] { item.DurationId },
                    ProActives = new[] { item.ProActiveSlaId },
                    ReactionTimes = new[] { item.ReactionTimeId },
                    ReactionTypes = new[] { item.ReactionTypeId },
                    ServiceLocations = new[] { item.ServiceLocationId },
                    Wgs = wgGroups[item.PlaId]
                };
            }

            PortfolioRuleSetDto BuildPrincipalRule(PrincipalPortfolioInheritance item)
            {
                var rule = BuildRule(item);

                rule.IsCorePortfolio = item.IsCorePortfolio;
                rule.IsGlobalPortfolio = item.IsGlobalPortfolio;
                rule.IsMasterPortfolio = item.IsMasterPortfolio;

                return rule;
            }

            PortfolioRuleSetDto BuildLocalRule(LocalPortfolioInheritance item)
            {
                var rule = BuildRule(item);

                rule.CountryId = item.CountryId;

                return rule;
            }
        }
    }
}
