using System.Collections.Generic;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class EntityMeta : BaseEntityMeta
    {
        public MetaCollection<FieldMeta> Fields { get; private set; } = new MetaCollection<FieldMeta>();

        public override IEnumerable<FieldMeta> AllFields => this.Fields;

        public EntityMeta(string name, string shema = null) 
            : base(name, shema)
        {
        }
        

        //public string Name { get; private set; }

        //public string Schema { get; private set; }

        //public string FullName => BuildFullName(this.Name, this.Schema);

        //public MetaCollection<FieldMeta> Fields { get; private set; } = new MetaCollection<FieldMeta>();

        //string IMetaIdentifialble.Id => this.FullName;

        //public EntityMeta(string name, string shema = null)
        //{
        //    this.Name = name;
        //    this.Schema = shema;
        //}

        //public static string BuildFullName(string name, string schema = null)
        //{
        //    return schema == null ? name : $"{schema}_{name}";
        //}
    }
}
