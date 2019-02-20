using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;

namespace Gdc.Scd.Core.Dto
{
    public class BundleDetailGroupDto : BaseBundleDetail
    {
        public IDictionary<string, NamedId[]> Coordinates { get; set; }
    }
}
