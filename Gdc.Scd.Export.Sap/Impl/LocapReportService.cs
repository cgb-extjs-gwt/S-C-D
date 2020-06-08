using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.Sap.Dto;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class LocapReportService
    {
        private const string PROC_NAME = "Report.LocapSapUpload";
        private readonly IRepositorySet _repositorySet;

        public LocapReportService(IRepositorySet repo)
        {
            _repositorySet = repo;
        }

        public List<LocapMergedData> Execute(DateTime? startPeriod)
        {
            var parameters = Prepare(startPeriod);
            var result = _repositorySet.ExecuteProc<LocapMergedData>(PROC_NAME, parameters);
            return result;
        }

        private static DbParameter[] Prepare(DateTime? startPeriod)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@periodStartDate").WithValue(startPeriod).Build()
            };
        }
    }
}
