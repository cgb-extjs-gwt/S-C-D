using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using DayOfWeek = Gdc.Scd.Core.Entities.ProjectCalculator.DayOfWeek;

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

        protected override void InnerSave(Project item)
        {
            item.Availability.Value = this.GetAvailabilityValue(item.Availability.Start, item.Availability.End);
            item.Availability.Name = item.Availability.ToString();
            item.Duration.Name = periodNameBuilder.GetPeriodName(item.Duration.Months * MinutesInMonth, item.Duration.PeriodType);
            item.ReactionTime.Name = periodNameBuilder.GetPeriodName(item.ReactionTime.Minutes, item.ReactionTime.PeriodType);

            base.InnerSave(item);
        }

        public int GetAvailabilityValue(DayHour start, DayHour end)
        {
            const int Vip = 10;
            const int Premium = 5;
            const int StartStandartHours = 8;
            const int EndStandartHours = 17;

            if (start.Day > end.Day)
            {
                throw new Exception("The starting day must be more than ending");
            }

            if (start.Hour > end.Hour)
            {
                throw new Exception("The starting hour must be more than ending");
            }

            var result = 0;

            if (start.Day <= DayOfWeek.Saturday && end.Day >= DayOfWeek.Saturday)
            {
                var hourCount = end.Hour - start.Hour + 1;

                result += hourCount * Premium;

                if (end.Day == DayOfWeek.Sunday)
                {
                    result += hourCount * Vip;
                }
            }

            if (start.Day >= DayOfWeek.Monday && end.Day >= DayOfWeek.Friday)
            {
                var premiumHourCount = 0;

                if (start.Hour < StartStandartHours)
                {
                    premiumHourCount += StartStandartHours - start.Hour;
                }

                if (end.Hour > EndStandartHours)
                {
                    premiumHourCount += end.Hour - EndStandartHours;
                }

                result += premiumHourCount * (DayOfWeek.Friday - start.Day + 1) * Premium;
            }

            return result;
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
