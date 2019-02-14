namespace Gdc.Scd.Core.Meta.Entities
{
    public class InputLevelMetaInfo<TInputLevel> where TInputLevel : InputLevelMeta
    {
        public TInputLevel InputLevel { get; set; }

        public bool Hide { get; set; }
    }
}
