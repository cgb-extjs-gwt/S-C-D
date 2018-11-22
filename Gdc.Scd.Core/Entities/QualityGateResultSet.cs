using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Entities
{
    public class QualityGateResultSet
    {
        public List<QualityGateResultSetItem> Items { get; } = new List<QualityGateResultSetItem>();

        public bool HasErrors
        {
            get
            {
                return 
                    this.Items != null && 
                    this.Items.Any(item => item.QualityGateResult.HasErrors);
            }
        }
    }
}
