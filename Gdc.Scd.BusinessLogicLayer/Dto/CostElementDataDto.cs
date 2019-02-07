using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Dto
{
    public class CostElementDataDto
    {
        public IEnumerable<NamedId> Regions { get; set; }

        public IEnumerable<NamedId> DependencyItems { get; set; }
    }
}
