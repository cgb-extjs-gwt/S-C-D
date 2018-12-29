namespace Gdc.Scd.Core.Entities
{
    public class HistoryContext : CostElementIdentifier
    {
        public long? RegionInputId { get; set; }

        public string InputLevelId { get; set; }

        public static HistoryContext Build(HistoryContext context)
        {
            return new HistoryContext
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
