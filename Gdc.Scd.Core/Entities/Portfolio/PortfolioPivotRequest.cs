using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Entities.Portfolio
{
    public class PortfolioPivotRequest : PivotRequest
    {
        public PortfolioFilterDto Filter { get; set; }

        public PortfolioType PortfolioType { get; set; }
    }
}
