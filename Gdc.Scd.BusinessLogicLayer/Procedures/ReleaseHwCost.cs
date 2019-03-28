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

        public async Task ExecuteAsync( HwFilterDto filter )
        {
            var parameters = Prepare( filter );
            await _repo.ExecuteProcAsync(PROC, parameters);
        }
    
        private static DbParameter[] Prepare( HwFilterDto filter )
        {
            var pCnt = new DbParameterBuilder().WithName("@cnt");
            var pWg = new DbParameterBuilder().WithName("@wg");
            var pAv = new DbParameterBuilder().WithName("@av");
            var pDur = new DbParameterBuilder().WithName("@dur");
            var pReactiontime = new DbParameterBuilder().WithName("@reactiontime");
            var pReactiontype = new DbParameterBuilder().WithName("@reactiontype");
            var pLoc = new DbParameterBuilder().WithName("@loc");
            var pPro = new DbParameterBuilder().WithName("@pro");

            if (filter != null)
            {
                pCnt.WithListIdValue(filter.Country);
                pWg.WithListIdValue(filter.Wg);
                pAv.WithListIdValue(filter.Availability);
                pDur.WithListIdValue(filter.Duration);
                pReactiontype.WithListIdValue(filter.ReactionType);
                pReactiontime.WithListIdValue(filter.ReactionTime);
                pLoc.WithListIdValue(filter.ServiceLocation);
                pPro.WithListIdValue(filter.ProActive);
            }

            return new DbParameter[] {
                 pCnt.Build(),
                 pWg.Build(),
                 pAv.Build(),
                 pDur.Build(),
                 pReactiontime.Build(),
                 pReactiontype.Build(),
                 pLoc.Build(),
                 pPro.Build()
            };
        }
    }
}
