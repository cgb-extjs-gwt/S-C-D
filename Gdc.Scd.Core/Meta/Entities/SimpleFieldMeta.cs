using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class SimpleFieldMeta : FieldMeta
    {
        public SimpleFieldMeta(string id, TypeCode type) 
            : base(id)
        {
            this.Type = type;
        }

        public TypeCode Type { get; private set; }
    }
}
