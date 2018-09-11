using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class HistoryContext : ICostElementIdentifier
    {
        public string ApplicationId { get; set; }

        public long? RegionInputId { get; set; }

        public string CostBlockId { get; set; }

        public string CostElementId { get; set; }

        public string InputLevelId { get; set; }

        public static HistoryContext Build(CostEditorContext context)
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
