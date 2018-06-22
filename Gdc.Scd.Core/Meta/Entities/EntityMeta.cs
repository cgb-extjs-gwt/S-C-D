using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class EntityMeta : IMetaIdentifialble
    {
        public string Name { get; private set; }

        public string Namespace { get; set; }

        public string FullName => BuildFullName(this.Name, this.Namespace);

        public MetaCollection<FieldMeta> Fields { get; private set; } = new MetaCollection<FieldMeta>();

        string IMetaIdentifialble.Id => this.FullName;

        public EntityMeta(string name)
        {
            this.Name = name;
        }

        public static string BuildFullName(string name, string nameSpace = null)
        {
            return nameSpace == null ? name : $"{nameSpace}_{name}";
        }
    }
}
