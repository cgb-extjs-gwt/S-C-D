using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Server.Entities;
using System.Net.Http;
using System.Threading.Tasks;
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
        public Task<HttpResponseMessage> Export([FromUri]long id)
        {
            return service.Excel(id, GetFilter())
                          .ContinueWith(x =>
                          {
                              var res = x.Result;
                              return this.ExcelContent(res.data, res.fileName);
                          });
        }

        [HttpGet]
        public Task<DataInfo<ReportDto>> GetAll()
        {
            return service.GetReports()
                          .ContinueWith(x =>
                          {
                              var d = x.Result;
                              return new DataInfo<ReportDto> { Items = d, Total = d.Length };
                          });
        }

        [HttpGet]
        public Task<ReportSchemaDto> Schema([FromUri]long id)
        {
            return service.GetSchema(id);
        }

        [HttpGet]
        public Task<HttpResponseMessage> View([FromUri]long id, [FromUri]int start = 0, [FromUri]int limit = 50)
        {
            if (!IsRangeValid(start, limit))
            {
                return Task.FromResult<HttpResponseMessage>(null);
            }

            return service.GetJsonArrayData(id, GetFilter(), start, limit)
                          .ContinueWith(x =>
                          {
                              var res = x.Result;
                              return this.JsonContent(res.json, res.total);
                          });
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