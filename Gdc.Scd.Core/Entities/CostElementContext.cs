namespace Gdc.Scd.Core.Entities
{
    public class CostElementContext : CostElementIdentifier
    {
        public long? RegionInputId { get; set; }

        public string InputLevelId { get; set; }

        public static CostElementContext Build(CostElementContext context)
        {
            return new CostElementContext
            {
                ApplicationId = context.ApplicationId,
                RegionInputId = context.RegionInputId,
                CostBlockId = context.CostBlockId,
                CostElementId = context.CostElementId,
                InputLevelId = context.InputLevelId,
            };
        }
    }
}
