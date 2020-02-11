using System;
using Gdc.Scd.Core.Meta.Interfaces;
using Newtonsoft.Json;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseMeta: IMetaIdentifialble, ICloneable
    {
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Caption { get; set; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
