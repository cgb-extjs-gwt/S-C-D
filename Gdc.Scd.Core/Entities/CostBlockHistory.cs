using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CostBlockHistoryTableName, Schema = MetaConstants.HistorySchema)]
    public class CostBlockHistory : IIdentifiable
    {
        public long Id { get; set; }

        //public HistoryActionInfo Edit { get; set; }

        //public HistoryActionInfo Approve { get; set; }

        public DateTime EditDate { get; set; }

        public User EditUser { get; set; }

        public DateTime? ApproveDate { get; set; }

        public User ApproveUser { get; set; }

        public HistoryContext Context { get; set; }

        //public string ApplicationId { get; set; }

        //public string RegionInputId { get; set; }

        //public string CostBlockId { get; set; }

        //public string CostElementId { get; set; }

        //public string InputLevelId { get; set; }

        //public List<DependencyId> DependencyIds { get; set; }

        //public List<InputLevelId> InputLevelIds { get; set; }

        //public List<HistoryEditItem> EditItems { get; set; }
    }
}
