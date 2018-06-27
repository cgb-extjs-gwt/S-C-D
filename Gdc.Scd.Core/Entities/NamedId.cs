using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class NamedId : IIdentifiable
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
