namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportDto
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public bool CountrySpecific { get; set; }

        public bool HasFreesedVersion { get; set; }
    }
}
