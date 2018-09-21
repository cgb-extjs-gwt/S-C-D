using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.enums;

namespace Gdc.Scd.Core.Entities
{
    public class BaseWgSog : NamedId
    {
        [MustCompare(true, IsIgnoreCase = true)]
        public string Description { get; set; }

        [MustCompare(true, IsIgnoreCase = true)]
        public string Alignment { get; set; }

        [MustCompare(true, IsIgnoreCase = true)]
        public string HWProductDescription { get; set; }

        [MustCompare(true)]
        public long PlaId { get; set; }
        public Pla Pla { get; set; }

        [MustCompare(true)]
        public long? SFabId { get; set; }
        public SFab SFab { get; set; }
    }
}
