using System.Collections.Generic;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class JoinHistoryValueQueryOptions
    {
        public bool IsUseRegionCondition { get; set; }

        public InputLevelJoinType InputLevelJoinType { get; set; }

        public IEnumerable<JoinInfo> JoinInfos { get; set; }
    }
}
