using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Dto
{
    public class MetaDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public static MetaDto Build(BaseDomainMeta meta)
        {
            return new MetaDto
            {
                Id = meta.Id,
                Name = meta.Name
            };
        }
    }
}
