using Gdc.Scd.Web.Server.Entities;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class ReportController : ApiController
    {
        public ReportController() { }

        [HttpGet]
        public object Export(string type)
        {
            return new { url = "export", type = type };
        }

        [HttpGet]
        public ReportModel Schema(string type)
        {
            return new ReportModel
            {
                caption = "Auto grid server model",

                fields = new ReportColumnModel[] {
                    new ReportColumnModel { name= nameof(SampleReport.col_1), text= "Super fields 1", type= ReportColumnType.number },
                    new ReportColumnModel { name= nameof(SampleReport.col_2), text= "Super fields 2", type= ReportColumnType.text },
                    new ReportColumnModel { name= nameof(SampleReport.col_3), text= "Super fields 3", type= ReportColumnType.text },
                    new ReportColumnModel { name= nameof(SampleReport.col_4), text= "Super fields 4", type= ReportColumnType.text }
                },

                filter = new ReportFilterModel[] {
                    new ReportFilterModel{ name= nameof(SampleReport.col_1), text= "Super fields 1", type= ReportColumnType.number },
                    new ReportFilterModel{ name= nameof(SampleReport.col_2), text= "Super fields 2", type= ReportColumnType.text },
                    new ReportFilterModel{ name= nameof(SampleReport.col_4), text= "Super fields 4", type= ReportColumnType.text }
                }
            };
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

    public class ReportModel
    {
        public string caption { get; set; }

        public ReportColumnModel[] fields { get; set; }

        public ReportFilterModel[] filter { get; set; }
    }

    public class ReportColumnModel
    {
        public string text { get; set; }

        public string name { get; set; }

        public ReportColumnType? type { get; set; }

        public bool? allowNull { get; set; }

        public int? flex { get; set; }
    }

    public class ReportFilterModel
    {
        public string text { get; set; }

        public string name { get; set; }

        public ReportColumnType? type { get; set; }

        public object value { get; set; }
    }

    public enum ReportColumnType
    {
        text,
        number
    }
}