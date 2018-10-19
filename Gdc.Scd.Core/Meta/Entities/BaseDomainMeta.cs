namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseDomainMeta<TCostBlock, TAplication>
        where TCostBlock : BaseMeta
        where TAplication: BaseMeta
    {
        public MetaCollection<TCostBlock> CostBlocks { get; set; }

        public MetaCollection<TAplication> Applications { get; set; }
    }
}
