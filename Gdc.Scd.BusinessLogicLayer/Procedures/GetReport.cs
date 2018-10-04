using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
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

        public Task<DataTable> ExecuteTableAsync(string func, DbParameter[] parameters)
        {
            return _repo.ExecuteAsTableAsync(SelectQuery(func, parameters), parameters);
        }

        public async Task<DataTableDto> ExecuteTableAsync(
                string func,
                int start,
                int limit,
                DbParameter[] parameters
            )
        {
            var result = new DataTableDto();

            result.Total = await _repo.ExecuteScalarAsync<int>(CountQuery(func, parameters), parameters);

            result.Data = await _repo.ExecuteAsTableAsync(SelectQuery(func, parameters, start, limit), Copy(parameters));

            return result;
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

            result.Json = await _repo.ExecuteAsJsonAsync(SelectQuery(func, parameters, start, limit), Copy(parameters));

            return result;
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(string func, DbParameter[] parameters)
        {
            var result = new JsonArrayDto();

            string sql = SelectQuery(func, parameters);
            result.Json = await _repo.ExecuteAsJsonAsync(sql, parameters);

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
                   .Append("SELECT TOP(10) * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        private static string SelectQuery(string func, DbParameter[] parameters, int start, int limit)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Append(" WHERE ROWNUM BETWEEN ").AppendValue(start).Append(" AND ").AppendValue(limit)
                   .Build();
        }

        private static DbParameter[] Copy(DbParameter[] parameters)
        {
            var len = parameters == null ? 0 : parameters.Length;
            var result = new DbParameter[len];

            int i = 0;

            for (; i < len; i++)
            {
                result[i] = parameters[i].Copy();
            }

            return result;
        }
    }
}
