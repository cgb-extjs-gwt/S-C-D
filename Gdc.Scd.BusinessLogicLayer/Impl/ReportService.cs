using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ReportService : IReportService
    {
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

        public IEnumerable<ReportDto> GetReports()
        {
            return new ReportDto[]
            {
                new ReportDto { Id = 1, Name = "HW and ProActive Service based on Master portfolio / HW and ProActive Service based on Portfolio Alignment", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Id = 1, Name = "MCT Contract", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Id = 1, Name = "Locap", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Id = 1, Name = "Locap Detailed", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Id = 1, Name = "HDD Retention", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Id = 1, Name = "ProActive_Parameter", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Id = 1, Name = "SCD Parameter", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Id = 1, Name = "Service Coverage based on Master Portfolio", CountrySpecific = true },
                new ReportDto { Id = 1, Name = "Logistic Reports" },
                new ReportDto { Id = 1, Name = "PO Standard Warranty Material" },
                new ReportDto { Id = 1, Name = "CalcOutput vs. FREEZE", CountrySpecific = true },
                new ReportDto { Id = 1, Name = "CalcOutput new vs.old", CountrySpecific = true },
                new ReportDto { Id = 1, Name = "SolutionPack ProActive Costing", CountrySpecific = true },
                new ReportDto { Id = 1, Name = "PriceList_CD_CS", CountrySpecific = true },
                new ReportDto { Id = 1, Name = "SW Service Price List", CountrySpecific = false },
                new ReportDto { Id = 1, Name = "SW Service Price List detailed", CountrySpecific = false },
                new ReportDto { Id = 1, Name = "Mapping SOG to WG_Ordercodes_Software and Solutions", CountrySpecific = false },
                new ReportDto { Id = 1, Name = "SolutionPack Price List", CountrySpecific = false },
                new ReportDto { Id = 1, Name = "SolutionPack Price List Details", CountrySpecific = false },
                new ReportDto { Id = 1, Name = "ProActive Costing", CountrySpecific = true },
                new ReportDto { Id = 1, Name = "ISPROA_Parameter", CountrySpecific = true },
                new ReportDto { Id = 1, Name = "FLAT Fee Reports", CountrySpecific = false }
            };
        }

        public ReportSchemaDto GetSchema(long reportId)
        {
            return new ReportSchemaDto
            {
                Title = "Auto grid server model",

                Fields = new ReportColumnDto[] {
                    new ReportColumnDto { Name= "col_1", Text= "Super fields 1", Type= ReportColumnType.NUMBER },
                    new ReportColumnDto { Name= "col_2", Text= "Super fields 2", Type= ReportColumnType.TEXT },
                    new ReportColumnDto { Name= "col_3", Text= "Super fields 3", Type= ReportColumnType.TEXT },
                    new ReportColumnDto { Name= "col_4", Text= "Super fields 4", Type= ReportColumnType.TEXT }
                },

                Filter = new ReportFilterDto[] {
                    new ReportFilterDto { Name= "col_1", Text= "Super fields 1", Type= ReportColumnType.NUMBER },
                    new ReportFilterDto { Name= "col_2", Text= "Super fields 2", Type= ReportColumnType.TEXT },
                    new ReportFilterDto { Name= "col_4", Text= "Super fields 4", Type= ReportColumnType.TEXT }
                }
            };
        }
    }
}
