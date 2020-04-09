using Gdc.Scd.Core.Dto;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetHwCost
    {
        private const string PROC = "Hardware.SpGetCosts";

        private readonly IRepositorySet _repo;

        public GetHwCost(IRepositorySet repo)
        {
            _repo = repo;
        }

        public Task<(string json, int total, bool hasMore)> ExecuteJsonAsync(bool approved, HwFilterDto filter, int start, int limit)
        {
            var parameters = Prepare(approved, filter, start, limit + 1); //get one row over limit for correct paging
            return _repo.ExecuteProcAsJsonAsync(PROC, limit, parameters);
        }

        private static DbParameter[] Prepare(bool approved, HwFilterDto filter, int lastid, int limit)
        {
            var pApproved = new DbParameterBuilder().WithName("approved").WithValue(approved);
            var pLocal = new DbParameterBuilder().WithName("local").WithValue(true);
            var pCnt = new DbParameterBuilder().WithName("cnt");
            var pFsp = new DbParameterBuilder().WithName("fsp");
            var pHasFsp = new DbParameterBuilder().WithName("hasFsp");
            var pWg = new DbParameterBuilder().WithName("wg");
            var pAv = new DbParameterBuilder().WithName("av");
            var pDur = new DbParameterBuilder().WithName("dur");
            var pReactiontime = new DbParameterBuilder().WithName("reactiontime");
            var pReactiontype = new DbParameterBuilder().WithName("reactiontype");
            var pLoc = new DbParameterBuilder().WithName("loc");
            var pPro = new DbParameterBuilder().WithName("pro");
            var pLastid = new DbParameterBuilder().WithName("lastid").WithValue(lastid);
            var pLimit = new DbParameterBuilder().WithName("limit").WithValue(limit);

            if (filter != null)
            {
                pCnt.WithListIdValue(filter.Country);
                pFsp.WithValue(filter.Fsp);
                pHasFsp.WithValue(filter.HasFsp);
                pWg.WithListIdValue(filter.Wg);
                pAv.WithListIdValue(filter.Availability);
                pDur.WithListIdValue(filter.Duration);
                pReactiontype.WithListIdValue(filter.ReactionType);
                pReactiontime.WithListIdValue(filter.ReactionTime);
                pLoc.WithListIdValue(filter.ServiceLocation);
                pPro.WithListIdValue(filter.ProActive);
            }

            return new DbParameter[] {
                 pApproved.Build(),
                 pLocal.Build(),
                 pCnt.Build(),
                 pFsp.Build(),
                 pHasFsp.Build(),
                 pWg.Build(),
                 pAv.Build(),
                 pDur.Build(),
                 pReactiontime.Build(),
                 pReactiontype.Build(),
                 pLoc.Build(),
                 pPro.Build(),
                 pLastid.Build(),
                 pLimit.Build()
            };
        }
    }
}
