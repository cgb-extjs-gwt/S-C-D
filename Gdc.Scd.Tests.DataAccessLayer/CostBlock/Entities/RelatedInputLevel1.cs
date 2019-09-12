using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Tests.DataAccessLayer.CostBlock.Entities
{
    [Table(nameof(RelatedInputLevel1), Schema = MetaConstants.InputLevelSchema)]
    public class RelatedInputLevel1 : NamedId
    {
        public List<RelatedInputLevel2> RelatedItems { get; set; }
    }
}
