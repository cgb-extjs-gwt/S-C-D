using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    [Table("Country", Schema = MetaConstants.InputLevelSchema)]
    public class Country : NamedId
    {
    }
}
