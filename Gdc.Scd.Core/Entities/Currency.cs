using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CurrencyTable, Schema = MetaConstants.ReferencesSchema)]
    public class Currency : NamedId
    {
    }
}
