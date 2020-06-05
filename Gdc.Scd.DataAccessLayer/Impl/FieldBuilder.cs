using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class FieldBuilder : IFieldBuilder
    {
        public FieldMeta BuildAutoApprovedField(string costElementId)
        {
            return 
                new ComputedFieldMeta(
                    $"{costElementId}_Approved", 
                    new ColumnSqlBuilder(costElementId));
        }
    }
}
