using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class JoinInfo
    {
        public BaseEntityMeta Meta { get; set; }

        public string ReferenceFieldName { get; set; }

        public string JoinedTableAlias { get; set; }

        public string MetaTableAlias { get; set; }

        public JoinType JoinType { get; set; } = JoinType.Inner;

        public JoinInfo()
        {
        }

        public JoinInfo(BaseEntityMeta meta, string referenceFieldName, string alias = null, string metaTableAlias = null)
        {
            this.Meta = meta;
            this.ReferenceFieldName = referenceFieldName;
            this.JoinedTableAlias = alias;
            this.MetaTableAlias = metaTableAlias;
        }
    }
}
