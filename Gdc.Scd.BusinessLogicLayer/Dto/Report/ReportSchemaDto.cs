namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportSchemaDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public ReportColumnDto[] Fields { get; set; }

        public ReportFilterDto[] Filter { get; set; }
    }
}
