using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Dto
{
    public class InputLevelDto : InputLevelMeta
    {
        public bool HasFilter { get; set; }

        public string FilterName { get; set; }
    }
}
