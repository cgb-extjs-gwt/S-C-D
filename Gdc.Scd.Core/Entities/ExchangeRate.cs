using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.ExchangeRateTable, Schema = MetaConstants.ReferencesSchema)]
    public class ExchangeRate : IIdentifiable
    {
        public const string CurrencyIdColumn = "CurrencyId";

        [ForeignKey("Currency")]
        [Column(CurrencyIdColumn)]
        public long Id { get; set; }

        public Currency Currency { get; set; }

        public double Value { get; set; }
    }
}
