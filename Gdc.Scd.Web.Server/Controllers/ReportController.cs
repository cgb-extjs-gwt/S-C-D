using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Server.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class ReportController : ApiController
    {
        private readonly IReportService service;

        public ReportController(IReportService reportService)
        {
            this.service = reportService;
        }

        [HttpGet]
        public object Export(string type)
        {
            return new { url = "export", type = type };
        }

        [HttpGet]
        public DataInfo<ReportDto> GetAll()
        {
            IEnumerable<ReportDto> d = service.GetReports();
            return new DataInfo<ReportDto> { Items = d, Total = d.Count() };
        }

        [HttpGet]
        public ReportSchemaDto Schema(string type)
        {
            return service.GetSchema(type);
        }

        [HttpGet]
        public DataInfo<SampleReport> View(string type, [FromUri] object filter, [FromUri]int start = 0, [FromUri]int limit = 50)
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            var d = new SampleReport[]
            {
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
                new SampleReport { col_1 = "v1", col_2 = 2, col_3 = "3", col_4 = "bla bla bla" },
            };

            return new DataInfo<SampleReport> { Items = d, Total = d.Length };
        }

        private bool isRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }

    public class SampleReport
    {
        public string col_1 { get; set; }
        public int col_2 { get; set; }
        public string col_3 { get; set; }
        public string col_4 { get; set; }
    }
}