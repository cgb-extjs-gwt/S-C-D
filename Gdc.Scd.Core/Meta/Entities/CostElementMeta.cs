namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseDomainMeta
    {
        public DependencyMeta Dependency { get; set; }

        public string Description { get; set; }

        public string ScopeId { get; set; }
    }
}
