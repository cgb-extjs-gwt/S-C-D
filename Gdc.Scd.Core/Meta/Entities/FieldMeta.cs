using System;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class FieldMeta : IMetaIdentifialble, ICloneable
    {
        public string Name { get; set; }

        string IMetaIdentifialble.Id => this.Name;

        public FieldMeta(string name)
        {
            this.Name = name;
        }

        public bool IsNullOption { get; set; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
