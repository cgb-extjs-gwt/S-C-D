using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class FieldMeta : IMetaIdentifialble, ICloneable
    {
        public string Name { get; private set; }

        string IMetaIdentifialble.Id => this.Name;

        public FieldMeta(string name)
        {
            this.Name = name;
        }

        public abstract object Clone();
    }
}
