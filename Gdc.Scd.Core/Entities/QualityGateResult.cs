using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Entities
{
    public class QualityGateResult
    {
        public IEnumerable<CostBlockValueHistory> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                return this.Errors != null && this.Errors.Any();
            }
        }
    }
}
