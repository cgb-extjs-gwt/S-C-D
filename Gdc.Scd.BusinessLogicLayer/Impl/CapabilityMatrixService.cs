using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CapabilityMatrixService : ICapabilityMatrixService
    {
        private const string CAPABILITY_MATRIX_DENY_TBL = "CapabilityMatrixDeny";

        private readonly IRepositorySet repositorySet;

        private IRepository<CapabilityMatrixAllow> allowRepo;

        private IRepository<CapabilityMatrixDeny> denyRepo;

        public CapabilityMatrixService(
                IRepositorySet repositorySet,
                IRepository<CapabilityMatrixAllow> allowRepo,
                IRepository<CapabilityMatrixDeny> denyRepo
            )
        {
            this.repositorySet = repositorySet;
            this.allowRepo = allowRepo;
            this.denyRepo = denyRepo;
        }

        public Task AllowCombination(CapabilityMatrixEditDto m)
        {
            /*
            DELETE FROM CapabilityMatrixDeny WHERE Id IN (

                SELECT Id FROM CapabilityMatrixAllow
                    WHERE CountryId = @country

                      AND FujitsuGlobalPortfolio = @globalPortfolio
                      AND MasterPortfolio =        @masterPortfolio
                      AND CorePortfolio =          @corePortfolio

                      AND WgId               IN (@wg[]...)
                      AND AvailabilityId     IN (@availability[]...)
                      AND DurationId         IN (@duration[]...)
                      AND ReactionTypeId     IN (@reactionType[]...)
                      AND ReactionTimeId     IN (@reactionTime[]...)
                      AND ServiceLocationId  IN (@serviceLocation[]...)

            )*/

            var sql = new SqlStringBuilder()
                        .Append("DELETE FROM CapabilityMatrixDeny WHERE Id IN ( ")
                        .Append("  SELECT Id FROM CapabilityMatrixAllow ")
                        .Append("   WHERE CountryId ").AppendEqualsOrNull(m.CountryId)

                        .Append("         AND FujitsuGlobalPortfolio ").AppendEquals(m.IsGlobalPortfolio)
                        .Append("         AND MasterPortfolio ").AppendEquals(m.IsMasterPortfolio)
                        .Append("         AND CorePortfolio ").AppendEquals(m.IsCorePortfolio)

                        .Append("         AND WgId ").AppendInOrNull(m.Wgs)
                        .Append("         AND AvailabilityId ").AppendInOrNull(m.Availabilities)
                        .Append("         AND DurationId ").AppendInOrNull(m.Durations)
                        .Append("         AND ReactionTypeId ").AppendInOrNull(m.ReactionTypes)
                        .Append("         AND ReactionTimeId ").AppendInOrNull(m.ReactionTimes)
                        .Append("         AND ServiceLocationId ").AppendInOrNull(m.ServiceLocations)

                        .Append(")")

                        .AsSql();

            return repositorySet.ExecuteSqlAsync(sql);
        }

        public Task AllowCombinations(long[] items)
        {
            if (items.Length == 0)
            {
                throw new System.ArgumentException("Invalid items list");
            }

            //DELETE FROM CapabilityMatrixDeny WHERE Id IN (@items[]...)

            var cmd = Sql.Delete(CAPABILITY_MATRIX_DENY_TBL).WhereId(items);
            return repositorySet.ExecuteSqlAsync(cmd);
        }

        public Task DenyCombination(CapabilityMatrixEditDto m)
        {
            /*INSERT INTO CapabilityMatrixDeny (
                            Id, 
                            CountryId, 
                            WgId, 
                            AvailabilityId, 
                            DurationId, 
                            ReactionTypeId, 
                            ReactionTimeId, 
                            ServiceLocationId, 
                            FujitsuGlobalPortfolio,
                            MasterPortfolio, 
                            CorePortfolio) (
                    SELECT Id, 
                           CountryId, 
                           WgId, 
                           AvailabilityId, 
                           DurationId, 
                           ReactionTypeId, 
                           ReactionTimeId, 
                           ServiceLocationId, 
                           FujitsuGlobalPortfolio, 
                           MasterPortfolio, 
                           CorePortfolio FROM CapabilityMatrixAllow
                        WHERE Id NOT IN (SELECT Id FROM CapabilityMatrixDeny)

                              AND CountryId = @country
                 
                              AND FujitsuGlobalPortfolio = @globalPortfolio
                              AND MasterPortfolio =        @masterPortfolio
                              AND CorePortfolio =          @corePortfolio
                 
                              AND WgId               IN (@wg[]...)
                              AND AvailabilityId     IN (@availability[]...)
                              AND DurationId         IN (@duration[]...)
                              AND ReactionTypeId     IN (@reactionType[]...)
                              AND ReactionTimeId     IN (@reactionTime[]...)
                              AND ServiceLocationId  IN (@serviceLocation[]...)
                )*/


            var sql = new SqlStringBuilder()
                .Append(@"INSERT INTO CapabilityMatrixDeny(
                            Id,CountryId,WgId,AvailabilityId,DurationId,
                            ReactionTypeId,ReactionTimeId,ServiceLocationId,
                            FujitsuGlobalPortfolio,MasterPortfolio,CorePortfolio)( ")

                .Append(@" SELECT Id,CountryId,WgId,AvailabilityId,DurationId,
                                  ReactionTypeId,ReactionTimeId,ServiceLocationId,
                                  FujitsuGlobalPortfolio,MasterPortfolio,CorePortfolio")
                .Append("    FROM CapabilityMatrixAllow ")
                .Append("      WHERE Id NOT IN (SELECT Id FROM CapabilityMatrixDeny)")

                .Append("            AND CountryId ").AppendEqualsOrNull(m.CountryId)

                .Append("            AND FujitsuGlobalPortfolio ").AppendEquals(m.IsGlobalPortfolio)
                .Append("            AND MasterPortfolio ").AppendEquals(m.IsMasterPortfolio)
                .Append("            AND CorePortfolio ").AppendEquals(m.IsCorePortfolio)

                .Append("            AND WgId ").AppendInOrNull(m.Wgs)
                .Append("            AND AvailabilityId ").AppendInOrNull(m.Availabilities)
                .Append("            AND DurationId ").AppendInOrNull(m.Durations)
                .Append("            AND ReactionTypeId ").AppendInOrNull(m.ReactionTypes)
                .Append("            AND ReactionTimeId ").AppendInOrNull(m.ReactionTimes)
                .Append("            AND ServiceLocationId ").AppendInOrNull(m.ServiceLocations)

                .Append(")")

                .AsSql();

            return repositorySet.ExecuteSqlAsync(sql);
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations()
        {
            return GetAllowedCombinations(null);
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations(CapabilityMatrixFilterDto filter)
        {
            var query = allowRepo
                        .GetAll()
                        .Where(x => !denyRepo.GetAll().Any(y => y.Id == x.Id));

            return GetCombinations(query, filter);
        }

        public IEnumerable<CapabilityMatrixDto> GetDeniedCombinations()
        {
            return GetDeniedCombinations(null);
        }

        public IEnumerable<CapabilityMatrixDto> GetDeniedCombinations(CapabilityMatrixFilterDto filter)
        {
            var query = denyRepo.GetAll();
            return GetCombinations(query, filter);
        }

        public IEnumerable<CapabilityMatrixDto> GetCombinations(
                IQueryable<CapabilityMatrix> query,
                CapabilityMatrixFilterDto filter
            )
        {
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

            return GetCombinations(query);
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
    }
}
