using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Gdc.Scd.BusinessLogicLayer.Impl.Report
{

    public class ReportTypes
    {
        public const string HW_AND_PROACTIVE = "hw-and-proactive";
        public const string MCT_CONTRACT = "mct-contract";
        public const string LOCAP = "locap";
        public const string LOCAP_DETAILED = "locap-detailed";
        public const string HDD_RETENTION = "hdd-retention";
        public const string PROACTIVE_PARAMETER = "proactive-parameter";
        public const string SCD_PARAMETER = "scd-parameter";
        public const string SERVICE_COVERAGE_BASED_ON_MASTER_PORTFOLIO = "service-coverage-based-on-master-portfolio";
        public const string LOGISTIC_REPORTS = "logistic-reports";
        public const string PO_STANDARD_WARRANTY_MATERIAL = "po-standard-warranty-material";
        public const string CALCOUTPUT_VS_FREEZE = "calcoutput-vs-freeze";
        public const string CALCOUTPUT_NEW_VSOLD = "calcoutput-new-vsold";
        public const string SOLUTIONPACK_PROACTIVE_COSTING = "solutionpack-proactive-costing";
        public const string PRICELIST_CD_CS = "pricelist-cd-cs";
        public const string SW_SERVICE_PRICE_LIST = "sw-service-price-list";
        public const string SW_SERVICE_PRICE_LIST_DETAILED = "sw-service-price-list-detailed";
        public const string MAPPING_SOG_TO_WG_ORDERCODES_SOFTWARE_AND_SOLUTIONS = "mapping-sog-to-wg-ordercodes-software-and-solutions";
        public const string SOLUTIONPACK_PRICE_LIST = "solutionpack-price-list";
        public const string SOLUTIONPACK_PRICE_LIST_DETAILS = "solutionpack-price-list-details";
        public const string PROACTIVE_COSTING = "proactive-costing";
        public const string ISPROA_PARAMETER = "isproa-parameter";
        public const string FLAT_FEE_REPORTS = "flat-fee-reports";
    }


    public class ReportService : IReportService
    {
        public Stream Excel(string type)
        {
            throw new System.NotImplementedException();
        }

        public DataTable GetData(
                string type,
                ReportFilterCollection filter,
                int start,
                int limit,
                out int total
            )
        {
            throw new System.NotImplementedException();
        }

        public string GetJsonArrayData(string type, ReportFilterCollection filter, int start, int limit, out int total)
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
                new ReportDto { Type = ReportTypes.HW_AND_PROACTIVE,                                    Name = "HW and ProActive Service based on Master portfolio / HW and ProActive Service based on Portfolio Alignment", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Type = ReportTypes.MCT_CONTRACT,                                        Name = "MCT Contract", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Type = ReportTypes.LOCAP,                                               Name = "Locap", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Type = ReportTypes.LOCAP_DETAILED,                                      Name = "Locap Detailed", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Type = ReportTypes.HDD_RETENTION,                                       Name = "HDD Retention", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Type = ReportTypes.PROACTIVE_PARAMETER,                                 Name = "ProActive_Parameter", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Type = ReportTypes.SCD_PARAMETER,                                       Name = "SCD Parameter", CountrySpecific = true, HasFreesedVersion = true },
                new ReportDto { Type = ReportTypes.SERVICE_COVERAGE_BASED_ON_MASTER_PORTFOLIO,          Name = "Service Coverage based on Master Portfolio", CountrySpecific = true },
                new ReportDto { Type = ReportTypes.LOGISTIC_REPORTS,                                    Name = "Logistic Reports" },
                new ReportDto { Type = ReportTypes.PO_STANDARD_WARRANTY_MATERIAL,                       Name = "PO Standard Warranty Material" },
                new ReportDto { Type = ReportTypes.CALCOUTPUT_VS_FREEZE,                                Name = "CalcOutput vs. FREEZE", CountrySpecific = true },
                new ReportDto { Type = ReportTypes.CALCOUTPUT_NEW_VSOLD,                                Name = "CalcOutput new vs.old", CountrySpecific = true },
                new ReportDto { Type = ReportTypes.SOLUTIONPACK_PROACTIVE_COSTING,                      Name = "SolutionPack ProActive Costing", CountrySpecific = true },
                new ReportDto { Type = ReportTypes.PRICELIST_CD_CS,                                     Name = "PriceList_CD_CS", CountrySpecific = true },
                new ReportDto { Type = ReportTypes.SW_SERVICE_PRICE_LIST,                               Name = "SW Service Price List", CountrySpecific = false },
                new ReportDto { Type = ReportTypes.SW_SERVICE_PRICE_LIST_DETAILED,                      Name = "SW Service Price List detailed", CountrySpecific = false },
                new ReportDto { Type = ReportTypes.MAPPING_SOG_TO_WG_ORDERCODES_SOFTWARE_AND_SOLUTIONS, Name = "Mapping SOG to WG_Ordercodes_Software and Solutions", CountrySpecific = false },
                new ReportDto { Type = ReportTypes.SOLUTIONPACK_PRICE_LIST,                             Name = "SolutionPack Price List", CountrySpecific = false },
                new ReportDto { Type = ReportTypes.SOLUTIONPACK_PRICE_LIST_DETAILS,                     Name = "SolutionPack Price List Details", CountrySpecific = false },
                new ReportDto { Type = ReportTypes.PROACTIVE_COSTING,                                   Name = "ProActive Costing", CountrySpecific = true },
                new ReportDto { Type = ReportTypes.ISPROA_PARAMETER,                                    Name = "ISPROA_Parameter", CountrySpecific = true },
                new ReportDto { Type = ReportTypes.FLAT_FEE_REPORTS,                                    Name = "FLAT Fee Reports", CountrySpecific = false }
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
