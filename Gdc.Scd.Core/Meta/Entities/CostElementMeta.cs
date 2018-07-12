namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseDomainMeta
    {
        public DependencyMeta Dependency { get; set; }

        public string Description { get; set; }

        public MetaCollection<InputLevelMeta> InputLevels { get; set; }

        public InputLevelMeta RegionInput { get; set; }

        public InputType InputType { get; set; }

        public InputLevelMeta GetPreviousInputLevel(string inputLevelId)
        {
            InputLevelMeta previousInputLevel = null;

            foreach (var inputLevel in this.InputLevels)
            {
                if (inputLevel.Id == inputLevelId)
                {
                    break;
                }

                previousInputLevel = inputLevel;
            }

            return previousInputLevel;
        }
    }
}
