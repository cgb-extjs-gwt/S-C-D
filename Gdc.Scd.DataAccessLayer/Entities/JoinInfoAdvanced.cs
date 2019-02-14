using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class JoinInfoAdvanced
    {
        public BaseEntityMeta JoinedMeta { get; set; }

        public ConditionHelper JoinCondition { get; set; }

        public JoinType JoinType { get; set; } = JoinType.Inner;

        public string JoinedAlias { get; set; }

        public JoinInfoAdvanced()
        {
        }

        public JoinInfoAdvanced(BaseEntityMeta joinedMeta, ConditionHelper joinCondition)
        {
            this.JoinedMeta = joinedMeta;
            this.JoinCondition = joinCondition;
        }
    }
}
