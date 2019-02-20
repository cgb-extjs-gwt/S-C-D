using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class InputLevelMetaInfo<TInputLevel> : IMetaIdentifialble 
        where TInputLevel : InputLevelMeta
    {
        string IMetaIdentifialble.Id => this.InputLevel?.Id;

        public TInputLevel InputLevel { get; set; }

        public bool Hide { get; set; }
    }
}
