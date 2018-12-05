namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public bool CountrySpecific { get; set; }

        public bool HasFreesedVersion { get; set; }
    }
}
