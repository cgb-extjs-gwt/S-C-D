using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class ComputedFieldMeta : FieldMeta
    {
        public ISqlBuilder Query { get; }

        public ComputedFieldMeta(string name, ISqlBuilder query) 
            : base(name)
        {
            this.Query = query;
        }
    }
}
