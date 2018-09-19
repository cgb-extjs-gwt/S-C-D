using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ReportService : IReportService
    {
        public object Excel(string type)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<object> GetData(
                string type,
                ReportFilterCollection filter,
                int start,
                int limit,
                out int total
            )
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
            return d;
        }

        public IEnumerable<ReportDto> GetReports()
        {
            return new ReportDto[]
            {
                new ReportDto { Name = "Sample report abc", Type = "abc" },
                new ReportDto { Name = "CBA sample report", Type = "cba" },
                new ReportDto { Name = "HDD retention report", Type = "hdd-retention" },
                new ReportDto { Name = "XYZ report", Type = "xyz" }
            };
        }

        public ReportSchema GetSchema(string type)
        {
            return new ReportSchema
            {
                Caption = "Auto grid server model",

                Fields = new ReportColumn[] {
                    new ReportColumn { name= "col_1", text= "Super fields 1", type= ReportColumnType.number },
                    new ReportColumn { name= "col_2", text= "Super fields 2", type= ReportColumnType.text },
                    new ReportColumn { name= "col_3", text= "Super fields 3", type= ReportColumnType.text },
                    new ReportColumn { name= "col_4", text= "Super fields 4", type= ReportColumnType.text }
                },

                Filter = new ReportFilter[] {
                    new ReportFilter { name= "col_1", text= "Super fields 1", type= ReportColumnType.number },
                    new ReportFilter { name= "col_2", text= "Super fields 2", type= ReportColumnType.text },
                    new ReportFilter { name= "col_4", text= "Super fields 4", type= ReportColumnType.text }
                }
            };
        }
    }
}
