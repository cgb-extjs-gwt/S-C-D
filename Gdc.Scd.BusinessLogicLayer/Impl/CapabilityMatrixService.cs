using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CapabilityMatrixService : ICapabilityMatrixService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<CapabilityMatrixRule> ruleRepo;

        public CapabilityMatrixService(
                IRepositorySet repositorySet,
                IRepository<CapabilityMatrixRule> ruleRepo
            )
        {
            this.repositorySet = repositorySet;
            this.ruleRepo = ruleRepo;
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

        public Task DenyCombination(CapabilityMatrixRuleSetDto m)
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

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations()
        {
            return GetAllowedCombinations(null);
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations(CapabilityMatrixFilterDto filter)
        {
            return repositorySet.ReadBySql("select top(100) ID, WG_NAME, AVAIL_NAME, DUR_NAME, REACT_TYPE_NAME, REACT_TIME_NAME, GLOBAL_PORTFOLIO, MASTER_PORTFOLIO, CORE_PORTFOLIO from MatrixAllowView", Read).Result;
        }

        public IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations()
        {
            return GetDeniedCombinations(null);
        }

        public IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations(CapabilityMatrixFilterDto filter)
        {
            var query = ruleRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Country.HasValue, x => x.Country.Id == filter.Country.Value)
                             .WhereIf(filter.Wg.HasValue, x => x.Wg.Id == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.Duration.Id == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionType.Id == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTime.Id == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocation.Id == filter.ServiceLocation.Value)
                             .WhereIf(filter.IsGlobalPortfolio.HasValue && filter.IsGlobalPortfolio.Value, x => x.FujitsuGlobalPortfolio)
                             .WhereIf(filter.IsMasterPortfolio.HasValue && filter.IsMasterPortfolio.Value, x => x.MasterPortfolio)
                             .WhereIf(filter.IsCorePortfolio.HasValue && filter.IsCorePortfolio.Value, x => x.CorePortfolio);
            }

            if (!filter.Country.HasValue)
            {
                query = query.OrderBy(x => x.Country.Name);
            }

            return query.Select(x => new CapabilityMatrixRuleDto
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
            })
            .ToList();
        }

        public IEnumerable<CapabilityMatrixDto> GetCombinations(IQueryable<CapabilityMatrix> query)
        {
            return query.Select(x => new CapabilityMatrixDto
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
            })
            .ToList();
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

        private CapabilityMatrixDto Read(IDataReader reader)
        {
            int ID = 0,
                WG_NAME = 1,
                AVAIL_NAME = 2,
                DUR_NAME = 3,
                REACT_TYPE_NAME = 4,
                REACT_TIME_NAME = 5,
                GLOBAL_PORTFOLIO = 6,
                MASTER_PORTFOLIO = 7,
                CORE_PORTFOLIO = 8;

            var result =  new CapabilityMatrixDto();

            result.Id = reader.GetInt64(ID);
            result.Wg = reader.GetString(WG_NAME);
            result.Availability = reader.GetString(AVAIL_NAME);

            return result;
        }
    }
}
