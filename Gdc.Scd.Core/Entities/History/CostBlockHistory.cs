using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CostBlockHistoryTableName, Schema = MetaConstants.HistorySchema)]
    public class CostBlockHistory : IIdentifiable
    {
        private User editUser;

        private User approveRejectUser;

        public long Id { get; set; }

        public DateTime EditDate { get; set; }

        public long? EditUserId { get; set; }

        public User EditUser
        {
            get
            {
                return this.editUser;
            }
            set
            {
                this.EditUserId = value?.Id;
                this.editUser = value;
            }
        }

        public DateTime? ApproveRejectDate { get; set; }

        public long? ApproveRejectUserId { get; set; }

        public User ApproveRejectUser
        {
            get
            {
                return this.approveRejectUser;
            }
            set
            {
                this.ApproveRejectUserId = value?.Id;
                this.approveRejectUser = value;
            }
        }

        public string RejectMessage { get; set; }

        public CostBlockHistoryState State { get; set; }

        public CostElementContext Context { get; set; }

        public int EditItemCount { get; set; }

        public bool IsDifferentValues { get; set; }

        public bool HasQualityGateErrors { get; set; }

        public string QualityGateErrorExplanation { get; set; }

        public EditorType EditorType { get; set; }
    }
}
