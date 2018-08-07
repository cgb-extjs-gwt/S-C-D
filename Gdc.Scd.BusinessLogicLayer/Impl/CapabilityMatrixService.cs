using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
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
            if (items.Length == 0)
            {
                throw new System.ArgumentException("Invalid items list");
            }

            //DELETE FROM MatrixRule WHERE Id IN (@items[]...)

            var sql = new SqlStringBuilder()
                         .Append("DELETE FROM MatrixRule WHERE Id ")
                         .AppendInOrNull(items)

                         .AsSql();

            return repositorySet.ExecuteSqlAsync(sql);
        }

        public async Task DenyCombination(CapabilityMatrixRuleSetDto m)
        {
            //Add deny combinations to MatrixRule
            await AddMatrixRules(m);

            //Update Matrix, set rows denied where match to rule
            await DenyMatrixRows(m);
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations(int start, int limit, out int count)
        {
            return GetAllowedCombinations(null, start, limit, out count);
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit, out int count)
        {
            if (filter != null && filter.Country.HasValue)
            {
                return GetCountryAllowedCombinations(filter, start, limit, out count);
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

            return Paging(result, start, limit, out count);
        }

        public IEnumerable<CapabilityMatrixDto> GetCountryAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit, out int count)
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

            return Paging(result, start, limit, out count);
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

            return Paging(result, start, limit, out count);
        }

        private Task AddMatrixRules(CapabilityMatrixRuleSetDto m)
        {
            //Add all posible combinations from user multi array input
            //except those which exists in db

            /*INSERT INTO MatrixRule(
                    CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, FujitsuGlobalPortfolio,MasterPortfolio, CorePortfolio) 

            SELECT @country, wg, av, dur, rtype, rtime, loc, @globalPortfolio, @masterPortfolio, @corePortfolio
            FROM (VALUES @wg[]...) a(wg)
            CROSS JOIN (VALUES @availability[]...) b(av)
            CROSS JOIN (VALUES @duration[]...) c(dur)
            CROSS JOIN (VALUES @reactionType[]...) d(rtype)
            CROSS JOIN (VALUES @reactionTime[]...) e(rtime)
            CROSS JOIN (VALUES @serviceLocation[]...) f(loc)

            EXCEPT

            SELECT CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, FujitsuGlobalPortfolio, MasterPortfolio, CorePortfolio
            FROM MatrixRule  */

            var sql = new SqlStringBuilder()
                .Append(@"INSERT INTO MatrixRule(
                            CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, FujitsuGlobalPortfolio,MasterPortfolio, CorePortfolio) ")

                .Append(CrossJoin(m))

                .Append(@"EXCEPT

                        SELECT CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, FujitsuGlobalPortfolio, MasterPortfolio, CorePortfolio
                        FROM MatrixRule")

                .AsSql();

            return repositorySet.ExecuteSqlAsync(sql);
        }

        private Task DenyMatrixRows(CapabilityMatrixRuleSetDto m)
        {
            /*UPDATE Matrix SET Denied = 1
                WHERE CountryId = @country

                      AND FujitsuGlobalPortfolio = @globport
                      AND MasterPortfolio = @masterPort
                      AND CorePortfolio = @corePort

                      AND WgId in (@wg[]...)
                      AND AvailabilityId in (@availability[]...)
                      AND DurationId in (@duration[]...)
                      AND ReactionTimeId in (@reactTime[]...)
                      AND ReactionTypeId in (@reactType[]...)
                      AND ServiceLocationId in (@serviceLoc[]...)*/

            var sql = new SqlStringBuilder();

            sql.Append("UPDATE Matrix SET Denied = 1 WHERE CountryId ").AppendEqualsOrNull(m.CountryId);

            if(!m.CountryId.HasValue)
            {
                sql.Append(" AND FujitsuGlobalPortfolio ").AppendEquals(m.IsGlobalPortfolio)
                   .Append(" AND MasterPortfolio ").AppendEquals(m.IsMasterPortfolio)
                   .Append(" AND CorePortfolio ").AppendEquals(m.IsCorePortfolio);
            }

            if (m.Wgs.Any())
            {
                sql.Append(" AND WgId ").AppendInOrNull(m.Wgs);
            }

            if (m.Availabilities.Any())
            {
                sql.Append(" AND AvailabilityId ").AppendInOrNull(m.Availabilities);
            }

            if (m.Durations.Any())
            {
                sql.Append(" AND DurationId ").AppendInOrNull(m.Durations);
            }

            if (m.ReactionTimes.Any())
            {
                sql.Append(" AND ReactionTimeId ").AppendInOrNull(m.ReactionTimes);
            }

            if (m.ReactionTypes.Any())
            {
                sql.Append( "AND ReactionTypeId ").AppendInOrNull(m.ReactionTypes);
            }

            if (m.ServiceLocations.Any())
            {
                sql.Append(" AND ServiceLocationId ").AppendInOrNull(m.ServiceLocations);
            }

            return repositorySet.ExecuteSqlAsync(sql.AsSql());
        }

        private string CrossJoin(CapabilityMatrixRuleSetDto m)
        {
            /*SELECT @country, wg, av, dur, rtype, rtime, loc, @globalPortfolio, @masterPortfolio, @corePortfolio
            FROM (VALUES @wg[]...) a(wg)
            CROSS JOIN (VALUES @availability[]...) b(av)
            CROSS JOIN (VALUES @duration[]...) c(dur)
            CROSS JOIN (VALUES @reactionType[]...) d(rtype)
            CROSS JOIN (VALUES @reactionTime[]...) e(rtime)
            CROSS JOIN (VALUES @serviceLocation[]...) f(loc)*/

            return new SqlStringBuilder()

                .Append("SELECT ")
                            .AppendValue(m.CountryId)
                            .Append(", wg")
                            .Append(", av")
                            .Append(", dur")
                            .Append(", rtype")
                            .Append(", rtime")
                            .Append(", loc")
                            .Append(", ").AppendValue(m.IsGlobalPortfolio)
                            .Append(", ").AppendValue(m.IsMasterPortfolio)
                            .Append(", ").AppendValue(m.IsCorePortfolio)

                .Append("FROM ").AppendValues(m.Wgs).Append(" a(wg)")
                .Append("CROSS JOIN ").AppendValues(m.Availabilities).Append(" b(av)")
                .Append("CROSS JOIN ").AppendValues(m.Durations).Append(" c(dur)")
                .Append("CROSS JOIN ").AppendValues(m.ReactionTypes).Append(" d(rtype)")
                .Append("CROSS JOIN ").AppendValues(m.ReactionTimes).Append(" e(rtime)")
                .Append("CROSS JOIN ").AppendValues(m.ServiceLocations).Append(" f(loc)")

                .AsSql();
        }

        private IEnumerable<T> Paging<T>(IQueryable<T> query, int start, int limit, out int count)
        {
            count = query.Count();
            return query.Skip(start).Take(limit).ToList();
        }
    }
}
