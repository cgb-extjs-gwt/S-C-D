using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Dto
{
    public class BundleDetailGroup : BaseBundleDetail
    {
        public IDictionary<string, NamedId[]> Coordinates { get; set; }
    }
}
