using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class EntityMeta : IMetaIdentifialble
    {
        public string Name { get; private set; }

        public string Schema { get; private set; }

        public string FullName => BuildFullName(this.Name, this.Schema);

        public MetaCollection<FieldMeta> Fields { get; private set; } = new MetaCollection<FieldMeta>();

        string IMetaIdentifialble.Id => this.FullName;

        public EntityMeta(string name, string shema = null)
        {
            this.Name = name;
            this.Schema = shema;
        }

        public static string BuildFullName(string name, string schema = null)
        {
            return schema == null ? name : $"{schema}_{name}";
        }
    }
}
