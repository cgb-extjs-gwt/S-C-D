using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
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

        public async Task<(string json, int total)> ExecuteJsonAsync(bool approved, SwFilterDto filter, int lastid, int limit)
        {
            var parameters = Prepare(approved, filter, lastid, limit);

            var json = await _repo.ExecuteProcAsJsonAsync(PROC, parameters);
            var total = GetTotal(parameters);

            return (json, total);
        }

        private static DbParameter[] Prepare(bool approved, SwFilterDto filter, int lastid, int limit)
        {
            var pApproved = new DbParameterBuilder().WithName("approved").WithValue(approved);
            var pCnt = new DbParameterBuilder().WithName("cnt");
            var pDigit = new DbParameterBuilder().WithName("digit");
            var pAv = new DbParameterBuilder().WithName("av");
            var pYear = new DbParameterBuilder().WithName("year");
            var pLastid = new DbParameterBuilder().WithName("lastid").WithValue(lastid);
            var pLimit = new DbParameterBuilder().WithName("limit").WithValue(limit);
            var pTotal = new DbParameterBuilder().WithName("total").WithType(DbType.Int32).WithDirection(ParameterDirection.Output);

            if (filter != null)
            {
                pCnt.WithListIdValue(filter.Country);
                pDigit.WithListIdValue(filter.Digit);
                pAv.WithListIdValue(filter.Availability);
                pYear.WithListIdValue(filter.Year);
            }

            return new DbParameter[] {
                 pApproved.Build(),
                 pCnt.Build(),
                 pDigit.Build(),
                 pAv.Build(),
                 pYear.Build(),
                 pLastid.Build(),
                 pLimit.Build(),
                 pTotal.Build()
            };
        }

        private static int GetTotal(DbParameter[] parameters)
        {
            return parameters[7].GetInt32();
        }
    }
}
