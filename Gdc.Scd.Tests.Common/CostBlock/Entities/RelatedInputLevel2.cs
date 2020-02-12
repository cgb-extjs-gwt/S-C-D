using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Tests.Common.CostBlock.Entities
{
    [Table(nameof(RelatedInputLevel2), Schema = MetaConstants.InputLevelSchema)]
    public class RelatedInputLevel2 : NamedId
    {
        public long RelatedInputLevel1Id { get; set; }

        public List<RelatedInputLevel3> RelatedItems { get; set; }
    }
}
