using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Portfolio;

namespace Gdc.Scd.Core.Entities.Portfolio
{
    public class PrincipalPortfolioInheritance : BasePortfolioInheritance
    {
        public bool IsGlobalPortfolio { get; set; }

        public bool IsMasterPortfolio { get; set; }

        public bool IsCorePortfolio { get; set; }
    }
}
