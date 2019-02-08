using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Dto
{
    public class CostEditorDto
    {
        //public IEnumerable<RegionDto> Regions { get; set; }

        public IEnumerable<NamedId> Filters { get; set; }

        public IEnumerable<NamedId> ReferenceValues { get; set; }
    }
}
