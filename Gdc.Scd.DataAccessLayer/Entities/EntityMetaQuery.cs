using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class EntityMetaQuery
    {
        public BaseEntityMeta Meta { get; set; }

        public SqlHelper Query { get; set; }
    }
}
