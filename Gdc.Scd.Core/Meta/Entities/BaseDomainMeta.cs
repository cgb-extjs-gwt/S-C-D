namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseDomainMeta<TCostBlock>
        where TCostBlock : BaseMeta
    {
        public MetaCollection<TCostBlock> CostBlocks { get; set; }

        public MetaCollection<ApplicationMeta> Applications { get; set; }
    }
}
