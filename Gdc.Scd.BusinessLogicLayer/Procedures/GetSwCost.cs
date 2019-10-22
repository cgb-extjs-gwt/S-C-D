using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetSwCost
    {
        private const string PROC = "SoftwareSolution.SpGetCosts";

        private readonly IRepositorySet _repo;

        public GetSwCost(IRepositorySet repo)
        {
            _repo = repo;
        }

        public Task<(string json, int total, bool hasMore)> ExecuteJsonAsync(bool approved, SwFilterDto filter, int start, int limit)
        {
            var parameters = Prepare(approved, filter, start, limit + 1);
            return _repo.ExecuteProcAsJsonAsync(PROC, limit, parameters);
        }

        private static DbParameter[] Prepare(bool approved, SwFilterDto filter, int lastid, int limit)
        {
            var pApproved = new DbParameterBuilder().WithName("approved").WithValue(approved);
            var pDigit = new DbParameterBuilder().WithName("digit");
            var pAv = new DbParameterBuilder().WithName("av");
            var pDuration = new DbParameterBuilder().WithName("year");
            var pLastid = new DbParameterBuilder().WithName("lastid").WithValue(lastid);
            var pLimit = new DbParameterBuilder().WithName("limit").WithValue(limit);

            if (filter != null)
            {
                pDigit.WithListIdValue(filter.Digit);
                pAv.WithListIdValue(filter.Availability);
                pDuration.WithListIdValue(filter.Duration);
            }

            return new DbParameter[] {
                 pApproved.Build(),
                 pDigit.Build(),
                 pAv.Build(),
                 pDuration.Build(),
                 pLastid.Build(),
                 pLimit.Build()
            };
        }
    }
}
