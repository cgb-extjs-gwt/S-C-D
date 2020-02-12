using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Tests.Common.CostBlock.Entities
{
    [Table(nameof(SimpleInputLevel2), Schema = MetaConstants.InputLevelSchema)]
    public class SimpleInputLevel2 : NamedId
    {
    }
}
