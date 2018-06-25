namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public MetaCollection<EntityMeta> CostBlocks { get; private set; } = new MetaCollection<EntityMeta>();

        public MetaCollection<EntityMeta> Dependencies { get; private set; } = new MetaCollection<EntityMeta>();

        public MetaCollection<EntityMeta> InputLevels { get; private set; } = new MetaCollection<EntityMeta>();

        public EntityMeta this[string fullName] => this.CostBlocks[fullName] ?? this.Dependencies[fullName] ?? this.InputLevels[fullName];

        public EntityMeta GetEntityMeta(string name, string schema = null)
        {
            var fullName = EntityMeta.BuildFullName(name, schema);

            return this[fullName];
        }
    }
}
