using Gdc.Scd.Core.Entities.Pivot;

namespace Gdc.Scd.Core.Entities.Portfolio
{
    public class PortfolioPivotRequest : PivotRequest
    {
        public PortfolioFilterDto Filter { get; set; }
    }
}
