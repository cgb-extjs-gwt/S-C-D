using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
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

        public Stream Excel(
                long reportId,
                ReportFilterCollection filter,
                out string fileName
            )
        {
            throw new System.NotImplementedException();
        }

        public DataTable GetData(
                long reportId,
                ReportFilterCollection filter
            )
        {
            throw new System.NotImplementedException();
        }

        public DataTable GetData(
                long reportId,
                ReportFilterCollection filter,
                int start,
                int limit,
                out int total
            )
        {
            throw new System.NotImplementedException();
        }

        public string GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit, out int total)
        {
            var d = new object[]
            {
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
            };
            total = d.Length;

            return Newtonsoft.Json.JsonConvert.SerializeObject(d);
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
