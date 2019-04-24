using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class ReleaseHwCost
    {
        private const string PROC = "Hardware.SpReleaseCosts";

        private readonly IRepositorySet _repo;

        public ReleaseHwCost(IRepositorySet repo)
        {
            _repo = repo;
        }

        public async Task ExecuteAsync(long userId, HwFilterDto filter, HwCostDto[] items = null)
        {
            var parameters = Prepare(userId, filter, items);
            await _repo.ExecuteProcAsync(PROC, parameters);
        }
    
        private static DbParameter[] Prepare(long userId, HwFilterDto filter, HwCostDto[] items)
        {
            var pUsr = new DbParameterBuilder().WithName("@usr");
            var pCnt = new DbParameterBuilder().WithName("@cnt");
            var pWg = new DbParameterBuilder().WithName("@wg");
            var pAv = new DbParameterBuilder().WithName("@av");
            var pDur = new DbParameterBuilder().WithName("@dur");
            var pReactionTime = new DbParameterBuilder().WithName("@reactiontime");
            var pReactionType = new DbParameterBuilder().WithName("@reactiontype");
            var pLoc = new DbParameterBuilder().WithName("@loc");
            var pPro = new DbParameterBuilder().WithName("@pro");
            var pPortfolio = new DbParameterBuilder().WithName("@portfolioIds");

            if (filter != null)
            {
                pUsr.WithValue(userId);
                pCnt.WithListIdValue(filter.Country);
                pWg.WithListIdValue(filter.Wg);
                pAv.WithListIdValue(filter.Availability);
                pDur.WithListIdValue(filter.Duration);
                pReactionType.WithListIdValue(filter.ReactionType);
                pReactionTime.WithListIdValue(filter.ReactionTime);
                pLoc.WithListIdValue(filter.ServiceLocation);
                pPro.WithListIdValue(filter.ProActive);
                pPortfolio.WithListIdValue(items?.Select(x=>x.Id).ToArray());
            }

            return new DbParameter[] {
                 pUsr.Build(),
                 pCnt.Build(),
                 pWg.Build(),
                 pAv.Build(),
                 pDur.Build(),
                 pReactionTime.Build(),
                 pReactionType.Build(),
                 pLoc.Build(),
                 pPro.Build(),
                 pPortfolio.Build()
            };
        }
    }
}
