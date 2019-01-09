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
            var sql = SelectAllQuery(func, parameters);

            await _repo.ReadBySql(sql, writer.WriteBody, parameters);

            return writer.GetData();
        }

        public async Task<(string json, int total)> ExecuteJsonAsync(
                string func,
                int start,
                int limit,
                DbParameter[] parameters
            )
        {
            string sql;

            sql = CountQuery(func, parameters);

            var total = await _repo.ExecuteScalarAsync<int>(sql, parameters);

            parameters = parameters.Copy();
            sql = SelectQuery(func, parameters, start, limit);

            var json = await _repo.ExecuteAsJsonAsync(sql, parameters);

            return (json, total);
        }

        private static string CountQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                    .Append("SELECT COUNT(*) FROM ").AppendFunc(func, parameters)
                    .Build();
        }

        private static string SelectAllQuery(string func, DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(func, parameters)
                   .Build();
        }

        private static string SelectQuery(string func, DbParameter[] parameters, int start, int limit)
        {
            return new SqlStringBuilder()
                    .Append(@"SELECT * FROM(
                                SELECT ROW_NUMBER() OVER (ORDER BY (SELECT 1)) as ROWNUM, tbl.* FROM ").AppendFunc(func, parameters).Append(" tbl")
                    .Append(") T WHERE ROWNUM BETWEEN ").AppendValue(start + 1).Append(" AND ").AppendValue(start + limit)
                    .Build();
        }
    }
}
