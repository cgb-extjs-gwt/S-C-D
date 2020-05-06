using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class JoinHistoryValueQueryOptions
    {
        public bool UseRegionCondition { get; set; }

        public InputLevelJoinType? InputLevelJoinType { get; set; }

        public IEnumerable<JoinInfo> JoinInfos { get; set; }

        public IDictionary<string, IEnumerable<object>> CostBlockFilter { get; set; }

        public bool UseActualVersionRows { get; set; }
    }
}
