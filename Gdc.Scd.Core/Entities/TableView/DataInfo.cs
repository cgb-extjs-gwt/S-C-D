using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class DataInfo : CostElementIdentifier
    {
        public long? DependencyItemId { get; set; }

        public string DataIndex { get; set; }

        public DataInfo()
        { 
        }

        public DataInfo(ICostElementIdentifier costElementIdentifier)
            : base(costElementIdentifier)
        {
        }
    }
}
