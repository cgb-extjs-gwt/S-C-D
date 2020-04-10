using System.Collections.Generic;
using System.Data;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class DistinctItemsInfo
    {
        public BaseEntityMeta Meta { get; set; }

        public string ReferenceFieldName { get; set; }

        public ConditionHelper FilterCondition { get; set; }

        public IEnumerable<FilterInfo> Filters { get; set; }

        public IEnumerable<JoinInfoAdvanced> JoinInfos { get; set; }

        public IsolationLevel? IsolationLevel { get; set; }
    }
}
