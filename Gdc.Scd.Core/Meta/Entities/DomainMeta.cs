namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainMeta
    {
        public MetaCollection<CostBlockMeta> CostBlocks { get; set; }

        public MetaCollection<ApplicationMeta> Applications { get; set; }

        public MetaCollection<InputLevelMeta> InputLevels { get; set; }
    }
}
