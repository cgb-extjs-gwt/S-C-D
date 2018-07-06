namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostAtomMeta : BaseDomainMeta
    {
        public MetaCollection<CostElementMeta> CostElements { get; set; }
    }
}
