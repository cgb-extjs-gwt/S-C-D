using System;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseMeta: IMetaIdentifialble, ICloneable
    {
        public string Id { get; set; }

        public string Caption { get; set; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
