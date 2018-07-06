namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainMeta
    {
        public MetaCollection<CostAtomMeta> CostAtoms { get; set; }

        public MetaCollection<CostBlockMeta> CostBlocks { get; set; }

        //public MetaCollection<InputLevelMeta> InputLevels { get; set; }

        public MetaCollection<ApplicationMeta> Applications { get; set; }

        //public InputLevelMeta GetPreviousInputLevel(string inputLevelId)
        //{
        //    InputLevelMeta previousInputLevel = null;

        //    foreach (var inputLevel in this.InputLevels)
        //    {
        //        if (inputLevel.Id == inputLevelId)
        //        {
        //            break;
        //        }

        //        previousInputLevel = inputLevel;
        //    }

        //    return previousInputLevel;
        //}
    }
}
