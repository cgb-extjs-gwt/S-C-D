using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class CostElementInfo
    {
        public CostBlockEntityMeta Meta { get; set; }

        public string[] CostElementIds { get; set; }
    }
}
