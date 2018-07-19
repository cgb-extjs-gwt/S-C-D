using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class CostElementData
    {
        public IEnumerable<NamedId> Regions { get; set; }

        public IEnumerable<NamedId> Filters { get; set; }

        public IEnumerable<NamedId> ReferenceValues { get; set; }
    }
}
