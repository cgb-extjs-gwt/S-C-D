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
        public object Export(string type)
        {
            return service.Excel(type);
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
        public DataInfo<object> View(string type, [FromUri]int start = 0, [FromUri]int limit = 50)
        {
            if (!IsRangeValid(start, limit))
            {
                return null;
            }

            int total;
            IEnumerable<object> d = service.GetData(type, GetFilter(), start, limit, out total);
            return new DataInfo<object> { Items = d, Total = total };
        }

        private ReportFilterCollection GetFilter()
        {
            return new ReportFilterCollection(Request.GetQueryNameValuePairs());
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }
}