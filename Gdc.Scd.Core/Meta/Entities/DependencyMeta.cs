using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DependencyMeta : BaseDomainMeta, IStoreTyped
    {
        public StoreType StoreType { get; set; }
    }
}
