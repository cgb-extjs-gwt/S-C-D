namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportColumnDto
    {
        public string text { get; set; }

        public string name { get; set; }

        public ReportColumnType? type { get; set; }

        public bool? allowNull { get; set; }

        public int? flex { get; set; }
    }
}