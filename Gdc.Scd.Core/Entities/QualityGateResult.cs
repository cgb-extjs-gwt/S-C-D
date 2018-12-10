using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Dto;

namespace Gdc.Scd.Core.Entities
{
    public class QualityGateResult
    {
        public IEnumerable<BundleDetailGroup> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                return this.Errors != null && this.Errors.Any();
            }
        }
    }
}
