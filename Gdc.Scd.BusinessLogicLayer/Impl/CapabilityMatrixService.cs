using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
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

        public CapabilityMatrixService(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
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

            return repositorySet.ExecuteSqlAsync(new SqlHelper(new RawSqlBuilder { RawSql = sql }));
        }

        public Task AllowCombinations(long[] items)
        {
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

            return repositorySet.ExecuteSqlAsync(new SqlHelper(new RawSqlBuilder { RawSql = sql }));
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations()
        {
            var query = AllowRepo()
                        .GetAll()
                        .Where(x => !DenyRepo().GetAll().Any(y => y.Id == x.Id));

            return GetCombinations(query);
        }

        public IEnumerable<CapabilityMatrixDto> GetDeniedCombinations()
        {
            var query = DenyRepo().GetAll();
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

        protected virtual IRepository<CapabilityMatrixAllow> AllowRepo()
        {
            if (allowRepo == null)
            {
                allowRepo = repositorySet.GetRepository<CapabilityMatrixAllow>();
            }
            return allowRepo;
        }

        protected virtual IRepository<CapabilityMatrixDeny> DenyRepo()
        {
            if (denyRepo == null)
            {
                denyRepo = repositorySet.GetRepository<CapabilityMatrixDeny>();
            }
            return denyRepo;
        }
    }
}
