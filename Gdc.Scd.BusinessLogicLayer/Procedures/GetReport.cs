using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
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
                int limit,
                DbParameter[] parameters
            )
        {
            var result = new JsonArrayDto();

            result.Total = await _repo.ExecuteScalarAsync<int>(CountQuery(func, parameters), parameters);

            result.Json = await _repo.ExecuteAsJsonAsync(SelectQuery(func, parameters), Copy(parameters, start, limit));

            return result;
        }

        private static string CountQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                    .Append("SELECT COUNT(*) FROM ").AppendFunc(func, parameters)
                    .Build();
        }

        private static string SelectQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Append(" WHERE ROWNUM BETWEEN @start AND @limit")
                   .Build();
        }

        private static DbParameter[] Copy(DbParameter[] parameters, int start, int limit)
        {
            const int MIN = 2;

            var len = parameters == null ? 0 : parameters.Length;
            var result = new DbParameter[len + MIN];

            int i = 0;

            for (; i < len; i++)
            {
                result[i] = parameters[i].Copy();
            }

            result[i++] = new DbParameterBuilder().WithName("@start").WithValue(start).Build();
            result[i++] = new DbParameterBuilder().WithName("@limit").WithValue(start + limit).Build();

            return result;
        }
    }
}
