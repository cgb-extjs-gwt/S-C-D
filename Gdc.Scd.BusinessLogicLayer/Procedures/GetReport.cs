using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetReport
    {
        private readonly IRepositorySet _repo;

        public GetReport(IRepositorySet repo)
        {
            _repo = repo;
        }

        public Task<DataTableDto> ExecuteTableAsync(string func, ReportFilterCollection filter, int start, int limit)
        {
            throw new System.NotImplementedException();
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(
                string func,
                int start,
                int limit
            )
        {
            var parameters = Prepare(start, limit);

            var result = new JsonArrayDto();

            result.Total = await _repo.ExecuteScalarAsync<int>(CountQuery(func));

            result.Json = await _repo.ExecuteAsJsonAsync(SelectQuery(func), parameters);

            return result;
        }

        private static string CountQuery(string func, params DbParameter[] parameters)
        {
            var builder = new SqlStringBuilder();

            builder.Append("SELECT COUNT(*) FROM ").AppendFunc(func, parameters);

            return builder.Build();
        }

        private static string SelectQuery(string func, params DbParameter[] parameters)
        {
            var builder = new SqlStringBuilder();

            builder.Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Append(" WHERE ROWNUM BETWEEN @start AND @limit");

            return builder.Build();
        }

        private static DbParameter[] Prepare(
                long reportId,
                ReportFilterCollection filter,
                int start,
                int limit
            )
        {
            return new DbParameter[] {
                 new SqlParameterBuilder().WithName("@reportId").WithValue(reportId).Build(),
                 new SqlParameterBuilder().WithName("@filter").WithKeyValue(filter).Build(),
                 new SqlParameterBuilder().WithName("@start").WithValue(start).Build(),
                 new SqlParameterBuilder().WithName("@limit").WithValue(limit).Build(),
                 new SqlParameterBuilder().WithName("@total").WithType(DbType.Int32).WithDirection(ParameterDirection.Output).Build()
            };
        }

        private static DbParameter[] Prepare(
                int start,
                int limit
            )
        {
            return new DbParameter[] {
                 new SqlParameterBuilder().WithName("@start").WithValue(start).Build(),
                 new SqlParameterBuilder().WithName("@limit").WithValue(start + limit).Build()
            };
        }
    }
}
