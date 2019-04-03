using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
using System.Data.Common;
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

        public async Task ExecuteAsync(long userId, HwFilterDto filter )
        {
            var parameters = Prepare(userId, filter);
            await _repo.ExecuteProcAsync(PROC, parameters);
        }
    
        private static DbParameter[] Prepare(long userId, HwFilterDto filter )
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
                 pPro.Build()
            };
        }
    }
}
