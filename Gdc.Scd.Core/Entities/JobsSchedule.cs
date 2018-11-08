using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("JobsSchedule", Schema = MetaConstants.SpoolerSchema)]
    public class JobsSchedule : IIdentifiable
    {
        public long Id { get; set; }
        public string JobName { get; set; }
        public bool Active { get; set; }
        public string PathToJob { get; set; }
        public bool Daily { get; set; }
        public DateTime ExactDate { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int TimeInHours { get; set; }
        public int MonthlyWeekNumber { get; set; }
        public DayOfWeek MonthlyDayOfWeek { get; set; }
    }
}
