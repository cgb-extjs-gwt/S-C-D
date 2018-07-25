using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseDomainMeta: IMetaIdentifialble
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
