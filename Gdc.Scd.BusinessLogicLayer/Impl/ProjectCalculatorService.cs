using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ProjectCalculatorService : DomainService<Project>, IProjectCalculatorService
    {
        private const int MinutesInHour = 60;
        private const int MinutesInDay = MinutesInHour * 24;
        private const int MinutesInWeek = MinutesInDay * 7;
        private const int MinutesInMonth = MinutesInDay * 30;
        private const int MinutesInYear = MinutesInMonth * 12;

        private static readonly PeriodNameBuilder periodNameBuilder = new PeriodNameBuilder();

        public ProjectCalculatorService(IRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override void Save(Project item)
        {
            base.Save(item);
            this.InterpolateProjects();
        }

        public override void Save(IEnumerable<Project> items)
        {
            base.Save(items);
            this.InterpolateProjects();
        }

        protected override void InnerSave(Project item)
        {
            item.Availability.Name = item.Availability.ToString();
            item.Duration.Name = periodNameBuilder.GetPeriodName(item.Duration.Months * MinutesInMonth, item.Duration.PeriodType);
            item.ReactionTime.Name = periodNameBuilder.GetPeriodName(item.ReactionTime.Minutes, item.ReactionTime.PeriodType);
            item.IsCalculated = false;

            base.InnerSave(item);
        }

        private void InterpolateProjects()
        {
            this.repositorySet.ExecuteProc("[ProjectCalculator].[InterpolateProjects]");
        }

        private class PeriodInfo
        {
            public int Devider { get; }

            public string Caption { get; }

            public PeriodInfo(int devider, string caption)
            {
                this.Devider = devider;
                this.Caption = caption;
            }
        }

        private class PeriodNameBuilder
        {
            private readonly Dictionary<PeriodType, PeriodInfo> periodInfos = new Dictionary<PeriodType, PeriodInfo>
            {
                [PeriodType.Minutes] = new PeriodInfo(1, "mins"),
                [PeriodType.Days] = new PeriodInfo(MinutesInDay, "days"),
                [PeriodType.Weeks] = new PeriodInfo(MinutesInWeek, "weeks"),
                [PeriodType.Months] = new PeriodInfo(MinutesInMonth, "months"),
                [PeriodType.Years] = new PeriodInfo(MinutesInYear, "years"),
            };

            public string GetPeriodName(int minutes, PeriodType periodType)
            {
                var periodInfo = this.periodInfos[periodType];

                return $"{minutes / periodInfo.Devider} {periodInfo.Caption}";
            }

            public string GetPeriodName(int? minutes, PeriodType? periodType)
            {
                return minutes.HasValue
                    ? this.GetPeriodName(minutes.Value, periodType.Value)
                    : "none";
            }
        }
    }
}
