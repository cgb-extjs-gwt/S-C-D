using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Server.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public HttpResponseMessage Export([FromUri]long id)
        {
            string fn;
            var data = service.Excel(id, GetFilter(), out fn);
            return this.ExcelContent(data, fn);
        }

        [HttpGet]
        public DataInfo<ReportDto> GetAll()
        {
            IEnumerable<ReportDto> d = service.GetReports();
            return new DataInfo<ReportDto> { Items = d, Total = d.Count() };
        }

        [HttpGet]
        public ReportSchemaDto Schema([FromUri]long id)
        {
            return service.GetSchema(id);
        }

        [HttpGet]
        public HttpResponseMessage View([FromUri]long id, [FromUri]int start = 0, [FromUri]int limit = 50)
        {
            if (!IsRangeValid(start, limit))
            {
                return null;
            }

            int total;
            string json = service.GetJsonArrayData(id, GetFilter(), start, limit, out total);
            return this.JsonContent(json, total);
        }

        private ReportFilterCollection GetFilter()
        {
            return new ReportFilterCollection(Request.GetQueryNameValuePairs());
        }

        private static bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }
}