using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Tests.DataAccessLayer.CostBlock.Entities
{
    [Table(nameof(Dependency2), Schema = MetaConstants.DependencySchema)]
    public class Dependency2 : NamedId
    {
    }
}
