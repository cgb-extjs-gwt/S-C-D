﻿using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
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

        public async Task<(string json, int total)> ExecuteJsonAsync(bool approved, HwFilterDto filter, int lastid, int limit)
        {
            var parameters = Prepare(approved, filter, lastid, limit);

            var json = await _repo.ExecuteProcAsJsonAsync(PROC, parameters);
            var total = GetTotal(parameters);

            return (json, total);
        }

        private static DbParameter[] Prepare(bool approved, HwFilterDto filter, int lastid, int limit)
        {
            var pApproved = new DbParameterBuilder().WithName("approved").WithValue(approved);
            var pLocal = new DbParameterBuilder().WithName("local").WithValue(true);
            var pCnt = new DbParameterBuilder().WithName("cnt");
            var pWg = new DbParameterBuilder().WithName("wg");
            var pAv = new DbParameterBuilder().WithName("av");
            var pDur = new DbParameterBuilder().WithName("dur");
            var pReactiontime = new DbParameterBuilder().WithName("reactiontime");
            var pReactiontype = new DbParameterBuilder().WithName("reactiontype");
            var pLoc = new DbParameterBuilder().WithName("loc");
            var pPro = new DbParameterBuilder().WithName("pro");
            var pLastid = new DbParameterBuilder().WithName("lastid").WithValue(lastid);
            var pLimit = new DbParameterBuilder().WithName("limit").WithValue(limit);
            var pTotal = new DbParameterBuilder().WithName("total").WithType(DbType.Int32).WithDirection(ParameterDirection.Output);

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
                 pApproved.Build(),
                 pLocal.Build(),
                 pCnt.Build(),
                 pWg.Build(),
                 pAv.Build(),
                 pDur.Build(),
                 pReactiontime.Build(),
                 pReactiontype.Build(),
                 pLoc.Build(),
                 pPro.Build(),
                 pLastid.Build(),
                 pLimit.Build(),
                 pTotal.Build()
            };
        }

        private static int GetTotal(DbParameter[] parameters)
        {
            return parameters[12].GetInt32();
        }
    }
}
