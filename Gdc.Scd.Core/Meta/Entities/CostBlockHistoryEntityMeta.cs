using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockHistoryEntityMeta : BaseEntityMeta
    {
        public IdFieldMeta IdField { get; } = new IdFieldMeta();

        public SimpleFieldMeta EditDateField { get; } = new SimpleFieldMeta(nameof(CostBlockHistory.EditDate), TypeCode.DateTime);

        public SimpleFieldMeta EditUserField { get; } = new SimpleFieldMeta(nameof(CostBlockHistory.EditUserId), TypeCode.UInt64);

        public SimpleFieldMeta ContextApplicationIdField { get; } = BuildContextField(nameof(CostElementContext.ApplicationId));

        public SimpleFieldMeta ContextCostBlockIdField { get; } = BuildContextField(nameof(CostElementContext.CostBlockId));

        public SimpleFieldMeta ContextCostElementIdField { get; } = BuildContextField(nameof(CostElementContext.CostElementId));

        public SimpleFieldMeta ContextRegionInputIdField { get; } = BuildContextField(nameof(CostElementContext.RegionInputId), TypeCode.Int64);

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.IdField;
                yield return this.EditDateField;
                yield return this.EditUserField;
                yield return this.ContextApplicationIdField;
                yield return this.ContextCostBlockIdField;
                yield return this.ContextCostElementIdField;
                yield return this.ContextRegionInputIdField;
            }
        }

        public CostBlockHistoryEntityMeta()
            : base(MetaConstants.CostBlockHistoryTableName, MetaConstants.HistorySchema)
        {
        }

        private static SimpleFieldMeta BuildContextField(string name, TypeCode type = TypeCode.String)
        {
            return new SimpleFieldMeta($"{nameof(CostBlockHistory.Context)}_{name}", type);
        }
    }
}
