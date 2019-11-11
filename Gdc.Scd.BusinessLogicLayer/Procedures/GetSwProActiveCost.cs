using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetSwProActiveCost
    {
        private const string PROC = "SoftwareSolution.SpGetProActiveCosts";

        private readonly IRepositorySet _repo;

        public GetSwProActiveCost(IRepositorySet repo)
        {
            _repo = repo;
        }

        public Task<(string json, int total, bool hasMore)> ExecuteJsonAsync(bool approved, SwFilterDto filter, int start, int limit)
        {
            var parameters = Prepare(approved, filter, start, limit + 1); //get one row over limit for correct paging
            return _repo.ExecuteProcAsJsonAsync(PROC, limit, parameters);
        }

        private static DbParameter[] Prepare(bool approved, SwFilterDto filter, int lastid, int limit)
        {
            var pApproved = new DbParameterBuilder().WithName("approved").WithValue(approved);
            var pCnt = new DbParameterBuilder().WithName("cnt");
            var pFsp = new DbParameterBuilder().WithName("fsp");
            var pDigit = new DbParameterBuilder().WithName("digit");
            var pAv = new DbParameterBuilder().WithName("av");
            var pYear = new DbParameterBuilder().WithName("year");
            var pLastid = new DbParameterBuilder().WithName("lastid").WithValue(lastid);
            var pLimit = new DbParameterBuilder().WithName("limit").WithValue(limit);

            if (filter != null)
            {
                pCnt.WithListIdValue(filter.Country);
                pFsp.WithValue(filter.Fsp);
                pDigit.WithListIdValue(filter.Digit);
                pAv.WithListIdValue(filter.Availability);
                pYear.WithListIdValue(filter.Year);
            }

            return new DbParameter[] {
                 pApproved.Build(),
                 pCnt.Build(),
                 pFsp.Build(),
                 pDigit.Build(),
                 pAv.Build(),
                 pYear.Build(),
                 pLastid.Build(),
                 pLimit.Build()
            };
        }
    }
}
