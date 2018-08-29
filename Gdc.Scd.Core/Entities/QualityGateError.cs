using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class QualityGateError
    {
        public NamedId WarrantyGroup { get; set; }

        public IDictionary<string, NamedId> Dependencies { get; set; }

        public bool IsRegionError { get; set; }

        public bool IsPeriodError { get; set; }
    }
}
