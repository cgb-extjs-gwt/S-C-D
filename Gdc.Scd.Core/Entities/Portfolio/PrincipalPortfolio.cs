using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Portfolio
{
    [Table("PrincipalPortfolio", Schema = MetaConstants.PortfolioSchema)]
    public class PrincipalPortfolio : Portfolio
    {
        public bool IsGlobalPortfolio { get; set; }

        public bool IsMasterPortfolio { get; set; }

        public bool IsCorePortfolio { get; set; }
    }
}
