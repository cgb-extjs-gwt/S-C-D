namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportFilterDto
    {
        public string text { get; set; }

        public string name { get; set; }

        public ReportColumnType? type { get; set; }

        public object value { get; set; }
    }
}