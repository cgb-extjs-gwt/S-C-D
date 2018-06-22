namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseMeta
    {
        public Dependency Dependency { get; set; }

        public string Description { get; set; }

        public string ScopeId { get; set; }
    }
}
