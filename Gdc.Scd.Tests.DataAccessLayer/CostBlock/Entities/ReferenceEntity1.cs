using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Tests.DataAccessLayer.CostBlock.Entities
{
    [Table(nameof(ReferenceEntity1), Schema = MetaConstants.ReferencesSchema)]
    public class ReferenceEntity1 : NamedId
    {
    }
}
