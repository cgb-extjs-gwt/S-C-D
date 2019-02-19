using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Entities.Alert;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json.Linq;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Report })]
    public class ReportController : ApiController
    {
        private readonly IReportService service;

        private readonly INotifyChannel channel;

        private readonly IKernel serviceProvider;

        public ReportController(
                IReportService reportService,
                INotifyChannel channel,
                IKernel serviceProvider
            )
        {
            this.service = reportService;
            this.channel = channel;
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public Task<HttpResponseMessage> Export([FromUri]long id)
        {
            return service.Excel(id, GetFilter()).ContinueWith(x => this.ExcelContent(x.Result.data, x.Result.fileName));
        }

        //TODO: realize user/country user access level validation
        [HttpPost]
        public Task<HttpResponseMessage> ExportByName([FromBody]ReportFormData data)
        {
            if (data.IsValid())
            {
                return service.Excel(data.Report, data.AsFilterCollection())
                              .ContinueWith(x => this.ExcelContent(x.Result.data, x.Result.fileName));
            }
            else
            {
                return this.NotFoundContentAsync();
            }
        }

        [HttpGet]
        public IHttpActionResult ExportAsync([FromUri]long id)
        {
            HostingEnvironment.QueueBackgroundWorkItem(ct => CreateReportAsync(id));
            return Ok();
        }

        [HttpGet]
        public HttpResponseMessage Load([FromUri]string key)
        {
            var res = AppCache.Get(key) as FileStreamDto;
            return res == null ? this.NotFoundContent() : this.ExcelContent(res.Data, res.FileName);
        }

        [HttpGet]
        public DataInfo<ReportDto> GetAll()
        {
            var d = service.GetReports();
            return new DataInfo<ReportDto> { Items = d, Total = d.Length };
        }

        [HttpGet]
        public ReportSchemaDto Schema([FromUri]long id)
        {
            return service.GetSchema(id);
        }

        [HttpGet]
        public ReportSchemaDto Schema([FromUri]string name)
        {
            return service.GetSchema(name);
        }

        [HttpGet]
        public Task<HttpResponseMessage> View(
                [FromUri]long id,
                [FromUri]int start = 0,
                [FromUri]int limit = 25
            )
        {
            if (!IsRangeValid(start, limit))
            {
                return null;
            }
            return service.GetJsonArrayData(id, GetFilter(), start, limit)
                          .ContinueWith(x => this.JsonContent(x.Result.json, x.Result.total));
        }

        private async Task CreateReportAsync(long id)
        {
            var srv = serviceProvider.Get<IReportService>();

            var report = await srv.Excel(id, GetFilter());
            var key = Guid.NewGuid().ToString();

            AppCache.Set(key, report);
            channel.Send(TextAlert.Report("Your report is completed", DownloadLink(key)));
        }

        private ReportFilterCollection GetFilter()
        {
            throw new System.NotImplementedException();
            //return new ReportFilterCollection(Request.GetQueryNameValuePairs());
        }

        private static bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }

        public static string DownloadLink(string key)
        {
            return string.Concat("/api/report/load?key=", key);
        }

        public class ReportFormData
        {
            public string Report { get; set; }

            public string Filter { get; set; }

            public ReportFilterCollection AsFilterCollection()
            {
                IList<KeyValuePair<string, object>> pairs = new List<KeyValuePair<string, object>>();

                var jo = JObject.Parse(Filter);

                foreach (var o in jo)
                {
                    object val = null;

                    var token = o.Value;

                    if (token.Type == JTokenType.Array)
                    {
                        val = token.ToObject<long[]>();
                    }
                    if (token.Type == JTokenType.Integer)
                    {
                        val = token.ToObject<long>();
                    }
                    if (token.Type == JTokenType.Float)
                    {
                        val = token.ToObject<double>();
                    }
                    if (token.Type == JTokenType.String)
                    {
                        val = token.ToObject<string>();
                    }

                    if (val != null)
                    {
                        pairs.Add(new KeyValuePair<string, object>(o.Key, val));
                    }
                }

                return new ReportFilterCollection(pairs);
            }

            public bool IsValid()
            {
                return !string.IsNullOrEmpty(Report);
            }
        }
    }
}