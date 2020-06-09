using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Report })]
    public class ReportController : ApiController
    {
        IDomainService<Country> countryService;
        private readonly IReportService service;

        public ReportController(
                IReportService reportService,
                IDomainService<Country> countryService
            )
        {
            this.countryService = countryService;
            this.service = reportService;
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Export([FromUri]ReportParameter data)
        {
            if (!data.IsValid)
            {
                return this.NotFoundContent();
            }

            try
            {
                var excel = await service.Excel(data.Report, data.AsFilterCollection(this.countryService));
                return this.ExcelContent(excel.data, excel.fileName);
            }
            catch
            {
                return this.NotFoundContent();
            }
        }

        [HttpPost]
        public Task<HttpResponseMessage> Export([FromUri]long id, [FromBody]ReportFormData data)
        {
            return service.Excel(id, data.AsFilterCollection())
                          .ContinueWith(x => this.ExcelContent(x.Result.data, x.Result.fileName));
        }

        //TODO: realize user/country user access level validation
        [HttpPost]
        public Task<HttpResponseMessage> ExportByName([FromBody]ReportFormData data)
        {
            return service.Excel(data.Report, data.AsFilterCollection())
                            .ContinueWith(x => this.ExcelContent(x.Result.data, x.Result.fileName));
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

        [HttpPost]
        public Task<HttpResponseMessage> View([FromUri]long id, [FromBody]ReportFormData data)
        {
            if (!IsRangeValid(data.Start, data.Limit))
            {
                return null;
            }
            return service.GetJsonArrayData(id, data.AsFilterCollection(), data.Start, data.Limit)
                          .ContinueWith(x => this.JsonContent(x.Result.json, x.Result.total));
        }

        [HttpPost]
        public Task UploadToSap([FromUri]long id, [FromBody]ReportFormData data)
        {
            return null;
        }

        private static bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }

        public class ReportFormData
        {
            public long Id { get; set; }

            public int Start { get; set; }

            public int Limit { get; set; }

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

                    switch (token.Type)
                    {
                        case JTokenType.Array:
                            val = token.ToObject<long[]>();
                            break;

                        case JTokenType.Integer:
                            val = token.ToObject<long>();
                            break;

                        case JTokenType.Float:
                            val = token.ToObject<double>();
                            break;

                        case JTokenType.String:
                            var str = token.ToObject<string>();
                            if (!string.IsNullOrEmpty(str))
                            {
                                val = str;
                            }
                            break;

                        case JTokenType.Boolean:
                            val = token.ToObject<bool>();
                            break;
                    }

                    if (val != null)
                    {
                        pairs.Add(new KeyValuePair<string, object>(o.Key, val));
                    }
                }

                return new ReportFilterCollection(pairs);
            }
        }

        public class ReportParameter
        {
            public string Report { get; set; }

            public string Country { get; set; }

            public bool IsValid
            {
                get
                {
                    return !string.IsNullOrEmpty(Report);
                }
            }

            public ReportFilterCollection AsFilterCollection(IDomainService<Country> countryService)
            {
                IEnumerable<KeyValuePair<string, object>> d = null;

                if (!string.IsNullOrEmpty(Country))
                {
                    var countryId = countryService.GetAll()
                                                  .Where(x => x.Name.ToUpper() == Country.ToUpper())
                                                  .Select(x => x.Id)
                                                  .First();

                    d = new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("cnt", countryId) };
                }
                else
                {
                    d = new KeyValuePair<string, object>[0];
                }

                return new ReportFilterCollection(d);
            }

        }
    }
}