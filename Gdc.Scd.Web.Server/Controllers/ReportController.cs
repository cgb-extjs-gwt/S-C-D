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
        public AutoGridModel Schema(string type)
        {
            return new AutoGridModel
            {
                caption = "Auto grid server model",

                fields = new AutoColumnModel[] {
                    new AutoColumnModel { name= "col_1", text= "Super fields 1", type= AutoColumnType.number },
                    new AutoColumnModel { name= "col_2", text= "Super fields 2", type= AutoColumnType.text },
                    new AutoColumnModel { name= "col_3", text= "Super fields 3", type= AutoColumnType.text },
                    new AutoColumnModel { name= "col_4", text= "Super fields 4", type= AutoColumnType.text }
                },

                filter = new AutoFilterModel[] {
                    new AutoFilterModel{ name= "col_1", text= "Super fields 1", type= AutoColumnType.number },
                    new AutoFilterModel{ name= "col_2", text= "Super fields 2", type= AutoColumnType.text },
                    new AutoFilterModel{ name= "col_4", text= "Super fields 4", type= AutoColumnType.text }
                }
            };
        }

        [HttpGet]
        public DataInfo<object> View(string type, [FromUri] object filter, [FromUri]int start = 0, [FromUri]int limit = 50)
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

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

            return new DataInfo<object>() { Items = d, Total = d.Length };

        }

        private bool isRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }

    public class AutoGridModel
    {
        public string caption { get; set; }

        public AutoColumnModel[] fields { get; set; }

        public AutoFilterModel[] filter { get; set; }
    }

    public class AutoColumnModel
    {
        public string text { get; set; }

        public string name { get; set; }

        public AutoColumnType? type { get; set; }

        public bool? allowNull { get; set; }

        public int? flex { get; set; }
    }

    public class AutoFilterModel
    {
        public string text { get; set; }

        public string name { get; set; }

        public AutoColumnType? type { get; set; }

        public object value { get; set; }
    }

    public enum AutoColumnType
    {
        text,
        number
    }
}