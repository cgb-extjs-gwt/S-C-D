using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CostBlockHistoryTableName, Schema = MetaConstants.HistorySchema)]
    public class CostBlockHistory : IIdentifiable
    {
        public long Id { get; set; }

        public DateTime EditDate { get; set; }

        public User EditUser { get; set; }

        public DateTime? ApproveRejectDate { get; set; }

        public User ApproveRejectUser { get; set; }

        public CostBlockHistoryState State { get; set; }

        public HistoryContext Context { get; set; }

        public int EditItemCount { get; set; }

        public bool IsDifferentValues { get; set; }
    }
}
