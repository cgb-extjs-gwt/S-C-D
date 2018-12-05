using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetHwCost
    {
        private const string FN = "Hardware.GetCosts";

        private readonly IRepositorySet _repo;

        public GetHwCost(IRepositorySet repo)
        {
            _repo = repo;
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(bool approved, HwFilterDto filter, int lastid, int limit)
        {
            JsonArrayDto result = new JsonArrayDto();

            var parameters = Prepare(approved, filter, lastid, limit);
            var sql = GetSql(approved, parameters);

            result.Json = await _repo.ExecuteAsJsonAsync(sql, parameters);
            result.Total = 1000;

            return result;
        }

        private static string GetSql(bool approved, DbParameter[] parameters)
        {
            if (approved)
            {
                return new SqlStringBuilder().Append("SELECT * FROM ").AppendFunc(FN, parameters).Build();
            }
            else
            {
                return new SqlStringBuilder()
                    .Append("SELECT cnt.CanOverrideTransferCostAndPrice, cnt.CanStoreListAndDealerPrices, c.* ")
                    .Append("FROM ").AppendFunc(FN, parameters).Append(" c ")
                    .Append("JOIN InputAtoms.Country cnt ON cnt.Name = c.Country")
                    .Build();
            }
        }

        private static DbParameter[] Prepare(bool approved, HwFilterDto filter, int lastid, int limit)
        {
            var pApproved = new DbParameterBuilder().WithName("approved").WithValue(approved);
            var pCnt = new DbParameterBuilder().WithName("cnt");
            var pWg = new DbParameterBuilder().WithName("wg");
            var pAv = new DbParameterBuilder().WithName("av");
            var pDur = new DbParameterBuilder().WithName("dur");
            var pReactiontime = new DbParameterBuilder().WithName("reactiontime");
            var pReactiontype = new DbParameterBuilder().WithName("reactiontype");
            var pLoc = new DbParameterBuilder().WithName("loc");
            var pLastid = new DbParameterBuilder().WithName("lastid").WithValue(lastid);
            var pLimit = new DbParameterBuilder().WithName("limit").WithValue(limit);

            if (filter != null)
            {
                pCnt.WithValue(filter.Country);
                pWg.WithValue(filter.Wg);
                pAv.WithValue(filter.Availability);
                pDur.WithValue(filter.Duration);
                pReactiontype.WithValue(filter.ReactionType);
                pReactiontime.WithValue(filter.ReactionTime);
                pLoc.WithValue(filter.ServiceLocation);
            }

            return new DbParameter[] {
                 pApproved.Build(),
                 pCnt.Build(),
                 pWg.Build(),
                 pAv.Build(),
                 pDur.Build(),
                 pReactiontime.Build(),
                 pReactiontype.Build(),
                 pLoc.Build(),
                 pLastid.Build(),
                 pLimit.Build()
            };
        }
    }
}
