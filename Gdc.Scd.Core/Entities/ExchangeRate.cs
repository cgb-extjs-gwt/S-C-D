using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("ExchangeRate", Schema = MetaConstants.ReferencesSchema)]
    public class ExchangeRate : IIdentifiable
    {
        public long Id { get; set; }

        public Currency Currency1 { get; set; }

        public Currency Currency2 { get; set; }

        public double Value { get; set; }
    }
}
