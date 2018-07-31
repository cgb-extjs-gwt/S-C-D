using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
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

        private const string ID_COL = "Id";

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

            var filter = new Dictionary<string, IEnumerable<object>>();

            var cmd = Sql.Delete(null, CAPABILITY_MATRIX_DENY_TBL).Where(filter);

            return repositorySet.ExecuteSqlAsync(cmd);
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


            return Task.FromResult(0);
        }

        public IEnumerable<CapabilityMatrixDto> GetAllowedCombinations()
        {
            return AllowRepo()
                .GetAll()
                .Where(x => !DenyRepo().GetAll().Any(y => y.Id == x.Id))
                .Select(x => new CapabilityMatrixDto
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

        public IEnumerable<CapabilityMatrixDto> GetDeniedCombinations()
        {
            return DenyRepo()
                .GetAll()
                .Select(x => new CapabilityMatrixAllow
                {
                    Id = x.Id,

                    Country = x.CapabilityMatrixAllow.Country,
                    Wg = x.CapabilityMatrixAllow.Wg,
                    Availability = x.CapabilityMatrixAllow.Availability,
                    Duration = x.CapabilityMatrixAllow.Duration,
                    ReactionType = x.CapabilityMatrixAllow.ReactionType,
                    ReactionTime = x.CapabilityMatrixAllow.ReactionTime,
                    ServiceLocation = x.CapabilityMatrixAllow.ServiceLocation,

                    FujitsuGlobalPortfolio = x.CapabilityMatrixAllow.FujitsuGlobalPortfolio,
                    MasterPortfolio = x.CapabilityMatrixAllow.MasterPortfolio,
                    CorePortfolio = x.CapabilityMatrixAllow.CorePortfolio
                })
                .Select(x => new CapabilityMatrixDto
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

        private SqlHelper BuildDeleteValueQuery()
        {
            throw new System.NotImplementedException();
        }

        //private SqlHelper BuildDeleteValueQuery(
        //    EditItem editItem,
        //    EditItemInfo editItemInfo,
        //    int index,
        //    object value,
        //    IDictionary<string, IEnumerable<object>> filter = null)
        //{
        //    var updateColumn = new ValueUpdateColumnInfo(
        //        editItemInfo.ValueField,
        //        editItem.Value,
        //        $"{editItemInfo.ValueField}_{index}");

        //    filter = new Dictionary<string, IEnumerable<object>>(filter ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<object>>>())
        //    {
        //        [editItemInfo.NameField] = new object[]
        //        {
        //            new CommandParameterInfo
        //            {
        //                Name = $"{editItemInfo.NameField}_{index}",
        //                Value = value
        //            }
        //        }
        //    };

        //    return Sql.Update(editItemInfo.Schema, editItemInfo.EntityName, updateColumn)
        //              .Where(filter);
        //}
    }
}
