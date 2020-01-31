using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Tests.Common.CostBlock.Entities
{
    [Table(nameof(SimpleInputLevel1), Schema = MetaConstants.InputLevelSchema)]
    public class SimpleInputLevel1 : NamedId
    {
    }
}
