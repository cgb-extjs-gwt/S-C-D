using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Portfolio
{
    [Table("PricipalPortfolio", Schema = MetaConstants.PortfolioSchema)]
    public class PrincipalPortfolio : Portfolio
    {
        public bool IsCorePortfolio { get; set; }

        public bool IsMasterPortfolio { get; set; }

        public bool IsGlobalPortfolio { get; set; }
    }
}
