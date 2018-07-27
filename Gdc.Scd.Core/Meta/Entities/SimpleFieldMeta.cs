using System;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class SimpleFieldMeta : FieldMeta
    {
        public SimpleFieldMeta(string name, TypeCode type) 
            : base(name)
        {
            this.Type = type;
        }

        public TypeCode Type { get; private set; }

        public override object Clone()
        {
            return new SimpleFieldMeta(this.Name, this.Type);
        }
    }
}
