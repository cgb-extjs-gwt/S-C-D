using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ReportService : IReportService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<Report> reportRepo;

        private readonly IRepository<ReportColumn> columnRepo;

        private readonly IRepository<ReportFilter> filterRepo;

        public ReportService(
                IRepositorySet repositorySet,
                IRepository<Report> reportRepo,
                IRepository<ReportColumn> columnRepo,
                IRepository<ReportFilter> filterRepo
            )
        {
            this.repositorySet = repositorySet;
            this.reportRepo = reportRepo;
            this.columnRepo = columnRepo;
            this.filterRepo = filterRepo;
        }

        public Task<(Stream data, string fileName)> Excel(long reportId, ReportFilterCollection filter)
        {
            throw new NotImplementedException();
        }

        public Task<DataTable> GetData(
                long reportId,
                ReportFilterCollection filter
            )
        {
            throw new System.NotImplementedException();
        }

        public Task<(DataTable tbl, int total)> GetData(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<(string json, int total)> GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            return new GetReport(repositorySet).ExecuteJsonAsync(reportId, filter, start, limit);
        }

        public Task<ReportDto[]> GetReports()
        {
            return reportRepo.GetAll()
                             .Select(x => new ReportDto
                             {
                                 Id = x.Id,
                                 Name = x.Name,
                                 Title = x.Title,
                                 CountrySpecific = x.CountrySpecific,
                                 HasFreesedVersion = x.HasFreesedVersion
                             })
                             .GetAsync();
        }

        public async Task<ReportSchemaDto> GetSchema(long reportId)
        {
            var report = await reportRepo.GetAll()
                                         .Where(x => x.Id == reportId)
                                         .Select(x => new ReportSchemaDto
                                         {
                                             Id = x.Id,
                                             Name = x.Name,
                                             Title = x.Title
                                         })
                                         .GetFirstOrDefaultAsync();

            if (report == null)
            {
                throw new System.ArgumentException("Report not found");
            }

            report.Fields = await columnRepo.GetAll()
                                            .Where(x => x.Report.Id == reportId)
                                            .Select(x => new ReportColumnDto
                                            {
                                                TypeId = x.Type.Id,
                                                Type = x.Type.Name,
                                                Name = x.Name,
                                                Text = x.Text,
                                                AllowNull = x.AllowNull,
                                                Flex = x.Flex
                                            })
                                            .GetAsync();

            report.Filter = await filterRepo.GetAll()
                                            .Where(x => x.Report.Id == reportId)
                                            .Select(x => new ReportFilterDto
                                            {
                                                MultiSelect = x.Type.MultiSelect,
                                                TypeId = x.Type.Id,
                                                Type = x.Type.Name,
                                                Name = x.Name,
                                                Text = x.Text,
                                                Value = x.Value
                                            })
                                            .GetAsync();

            return report;
        }
    }
}
