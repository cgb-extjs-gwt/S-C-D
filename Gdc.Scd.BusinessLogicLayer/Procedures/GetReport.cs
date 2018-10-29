using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Data.Common;
using System.IO;
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

        public async Task<Stream> ExecuteExcelAsync(ReportSchemaDto schema, string func, DbParameter[] parameters)
        {
            var writer = new ReportExcelWriter(schema);
            var sql = SelectAllQuery(func, parameters, 1000);

            await _repo.ReadBySql(sql, writer.WriteBody, parameters);

            return writer.GetData();
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(
                string func,
                int start,
                int limit,
                DbParameter[] parameters
            )
        {
            string sql;
            JsonArrayDto result = new JsonArrayDto();

            sql = CountQuery(func, parameters);

            result.Total = await _repo.ExecuteScalarAsync<int>(sql, parameters);

            parameters = parameters.Copy();
            sql = SelectQuery(func, parameters, start, limit);

            result.Json = await _repo.ExecuteAsJsonAsync(sql, parameters);

            return result;
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(string func, DbParameter[] parameters)
        {
            string sql;
            JsonArrayDto result = new JsonArrayDto();

            sql = SelectQuery(func, parameters);
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
            return SelectAllQuery(func, parameters, 30);
        }

        private static string SelectAllQuery(string func, DbParameter[] parameters, int max)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        private static string SelectQuery(string func, DbParameter[] parameters, int start, int limit)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Append(" WHERE ROWNUM BETWEEN ").AppendValue(start).Append(" AND ").AppendValue(limit)
                   .Build();
        }
    }
}
