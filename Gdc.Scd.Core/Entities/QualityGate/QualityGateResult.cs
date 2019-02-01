using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Dto;

namespace Gdc.Scd.Core.Entities.QualityGate
{
    public class QualityGateResult
    {
        public IEnumerable<BundleDetailGroupDto> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                return this.Errors != null && this.Errors.Any();
            }
        }
    }
}
