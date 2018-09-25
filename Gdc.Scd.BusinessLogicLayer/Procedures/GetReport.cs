using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetReport
    {
        private const string PROC_NAME = "Report.GetReport";

        private readonly IRepositorySet _repositorySet;

        public GetReport(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public Task<DataTableDto> ExecuteTableAsync(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            var parameters = Prepare(reportId, filter, start, limit);
            return _repositorySet.ExecuteProcAsTableAsync(PROC_NAME, parameters)
                                 .ContinueWith(x =>
                                 {
                                     return new DataTableDto { Data = x.Result, Total = GetTotal(parameters) };
                                 });
        }

        public Task<JsonArrayDto> ExecuteJsonAsync(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            var parameters = Prepare(reportId, filter, start, limit);
            return _repositorySet.ExecuteProcAsJsonAsync(PROC_NAME, parameters)
                                 .ContinueWith(x =>
                                 {
                                     return new JsonArrayDto { Json = x.Result, Total = GetTotal(parameters) };
                                 });
        }

        public async Task<JsonArrayDto> ExecuteJsonAsync(
                string func,
                int start,
                int limit
            )
        {
            var parameters = Prepare(start, limit);

            var result = new JsonArrayDto();

            result.Total = await _repositorySet.ExecuteScalarAsync<int>(CountQuery(func, null));

            result.Json = await _repositorySet.ExecuteAsJsonAsync(SelectQuery(func, null, start, limit), parameters);

            return result;
        }

        private static string CountQuery(string func, ReportFilterCollection filter)
        {
            var builder = new SqlStringBuilder();

            builder.Append("select count(*) from ").Append(func)
                   .Append("(");

            AppendFuncArgs(filter, builder);

            builder.Append(")");

            return builder.AsSql();
        }

        private static string SelectQuery(string func, ReportFilterCollection filter, int start, int limit)
        {
            var builder = new SqlStringBuilder();

            builder.Append("select * from ").Append(func).Append("(");

            AppendFuncArgs(filter, builder);

            builder.Append(") WHERE ROWNUM BETWEEN @start AND @limit");

            return builder.AsSql();
        }

        private static SqlStringBuilder AppendFuncArgs(
                ReportFilterCollection filter,
                SqlStringBuilder builder
            )
        {
            //bool flag = false;

            //foreach (var i in filter)
            //{
            //    if (flag)
            //    {
            //        builder.Append(", ");
            //    }
            //    builder.Append("@").Append(i.Key);
            //    flag = true;
            //}

            return builder;
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

        private static int GetTotal(DbParameter[] parameters)
        {
            return parameters[4].GetInt32();
        }
    }
}
