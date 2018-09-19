namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportSchema
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string Caption { get; set; }

        public ReportColumn[] Fields { get; set; }

        public ReportFilter[] Filter { get; set; }
    }
}
