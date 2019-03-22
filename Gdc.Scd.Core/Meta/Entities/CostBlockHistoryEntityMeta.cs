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

        public SimpleFieldMeta ContextApplicationIdField { get; } = BuildContextField(nameof(CostElementContext.ApplicationId));

        public SimpleFieldMeta ContextCostBlockIdField { get; } = BuildContextField(nameof(CostElementContext.CostBlockId));

        public SimpleFieldMeta ContextCostElementIdField { get; } = BuildContextField(nameof(CostElementContext.CostElementId));

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.IdField;
                yield return this.EditDateField;
                yield return this.ContextApplicationIdField;
                yield return this.ContextCostBlockIdField;
                yield return this.ContextCostElementIdField;
            }
        }

        public CostBlockHistoryEntityMeta()
            : base(MetaConstants.CostBlockHistoryTableName, MetaConstants.HistorySchema)
        {
        }

        private static SimpleFieldMeta BuildContextField(string name)
        {
            return new SimpleFieldMeta($"{nameof(CostBlockHistory.Context)}_{name}", TypeCode.String);
        }
    }
}
