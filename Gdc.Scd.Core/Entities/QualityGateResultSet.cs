using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Entities
{
    public class QualityGateResultSet
    {
        public IDictionary<string, QualityGateResult> QualityGateResults { get; } = new Dictionary<string, QualityGateResult>();

        public bool HasErrors
        {
            get
            {
                return 
                    this.QualityGateResults != null && 
                    this.QualityGateResults.Values.Any(qualityGateResult => qualityGateResult.HasErrors);
            }
        }
    }
}
