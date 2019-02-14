using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Portfolio
{
    [Table(MetaConstants.LocalPortfolioTableName, Schema = MetaConstants.PortfolioSchema)]
    public class LocalPortfolio: Portfolio
    {
        [Required]
        public Country Country { get; set; }
    }
}
