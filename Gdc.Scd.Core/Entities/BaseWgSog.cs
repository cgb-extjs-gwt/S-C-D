using Gdc.Scd.Core.Attributes;

namespace Gdc.Scd.Core.Entities
{
    public class BaseWgSog : NamedId
    {
        [MustCompare(true, IsIgnoreCase = true)]
        public string Description { get; set; }

        [MustCompare(true, IsIgnoreCase = true)]
        public string Alignment { get; set; }

        [MustCompare(true)]
        public long PlaId { get; set; }
        public Pla Pla { get; set; }

        [MustCompare(true)]
        public long? SFabId { get; set; }
        public SFab SFab { get; set; }

        [MustCompare(true)]
        public string SCD_ServiceType { get; set; }
    }
}
