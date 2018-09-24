using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
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

        public Task<(DataTable tbl, int total)> ExecuteTableAsync(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            var parameters = Prepare(reportId, filter, start, limit);
            return _repositorySet.ExecuteProcAsTableAsync(PROC_NAME, parameters)
                                 .ContinueWith(x =>
                                 {
                                     return (tbl: x.Result, total: GetTotal(parameters));
                                 });
        }

        public Task<(string json, int total)> ExecuteJsonAsync(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            var parameters = Prepare(reportId, filter, start, limit);
            return _repositorySet.ExecuteProcAsJsonAsync(PROC_NAME, parameters)
                                 .ContinueWith(x =>
                                 {
                                     return (json: x.Result, total: GetTotal(parameters));
                                 });
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
                 new SqlParameterBuilder().WithName("@total").WithDirection(ParameterDirection.Output).Build()
            };
        }

        private static int GetTotal(DbParameter[] parameters)
        {
            return Convert.ToInt32(parameters[4].Value);
        }
    }
}
