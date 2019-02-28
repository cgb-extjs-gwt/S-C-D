using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
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

        public Task<Stream> ExecuteExcelAsync(ReportSchemaDto schema, string func, DbParameter[] parameters)
        {
            if (IsProcedure(func))
            {
                return ExecuteExcelProcAsync(schema, func, parameters);
            }
            else
            {
                return ExecuteExcelFuncAsync(schema, func, parameters);
            }
        }

        public Task<(string json, int total)> ExecuteJsonAsync(
                string func,
                int start,
                int limit,
                DbParameter[] parameters
            )
        {
            if (IsProcedure(func))
            {
                return ExecuteProcAsJsonAsync(func, start, limit, parameters);
            }
            else
            {
                return ExecuteFuncAsJsonAsync(func, start, limit, parameters);
            }
        }

        public Task<(string json, int total)> ExecuteFuncAsJsonAsync(
                string func,
                int start,
                int limit,
                DbParameter[] parameters
            )
        {
            string sql = SelectQuery(func, parameters, start, limit);

            return _repo.ExecuteAsJsonAsync(sql, parameters);

            //return (json, total);
        }

        public Task<(string json, int total)> ExecuteProcAsJsonAsync(
                string proc,
                int start,
                int limit,
                DbParameter[] parameters
            )
        {
            parameters = Prepare(parameters, start, limit);

            return _repo.ExecuteProcAsJsonAsync(proc, parameters);
        }

        private async Task<Stream> ExecuteExcelFuncAsync(ReportSchemaDto schema, string func, DbParameter[] parameters)
        {
            var writer = new ReportExcelWriter(schema);
            var sql = SelectAllQuery(func, parameters);

            await _repo.ReadBySql(sql, writer.WriteBody, parameters);

            return writer.GetData();
        }

        private async Task<Stream> ExecuteExcelProcAsync(ReportSchemaDto schema, string func, DbParameter[] parameters)
        {
            var writer = new ReportExcelWriter(schema);

            parameters = Prepare(parameters, -1, -1);
            await _repo.ExecuteProcAsync(func, writer.WriteBody, parameters);

            return writer.GetData();
        }

        private static bool IsProcedure(string func)
        {
            return func.Contains(".sp"); //TODO: remove stub form exec procedure
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

        private static DbParameter[] Prepare(DbParameter[] parameters, int start, int limit)
        {
            var len = parameters.Length;
            var result = new DbParameter[len + 3];

            int i = 0;
            for (; i < len; i++)
            {
                result[i] = parameters[i];
            }

            var pLastId = new DbParameterBuilder().WithName("lastid");
            var pLimit = new DbParameterBuilder().WithName("limit");
            var pTotal = new DbParameterBuilder().WithName("total").WithType(DbType.Int32).WithDirection(ParameterDirection.Output);

            if (start >= 0)
            {
                pLastId.WithValue(start);
            }

            if(limit > 0)
            {
                pLimit.WithValue(limit);
            }

            result[i++] = pLastId.Build();
            result[i++] = pLimit.Build();
            result[i++] = pTotal.Build();

            return result;
        }

        private static int GetTotal(DbParameter[] parameters)
        {
            return parameters[parameters.Length - 1].GetInt32();
        }
    }
}
