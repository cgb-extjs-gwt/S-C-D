namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportSchemaDto
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string Caption { get; set; }

        public ReportColumnDto[] Fields { get; set; }

        public ReportFilterDto[] Filter { get; set; }
    }
}
