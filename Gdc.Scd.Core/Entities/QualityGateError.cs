using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.Core.Entities
{
    public class QualityGateError : NamedId
    {
        public IDictionary<string, NamedId> Dependencies { get; set; }

        public bool IsRegionError { get; set; }

        public bool IsPeriodError { get; set; }
    }
}
