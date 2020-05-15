using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;

using DayOfWeek = Gdc.Scd.Core.Entities.ProjectCalculator.DayOfWeek;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ProjectCalculatorService : DomainService<Project>, IProjectCalculatorService
    {
        public ProjectCalculatorService(IRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        protected override void InnerSave(Project item)
        {
            item.Availability.Value = this.GetAvailabilityValue(item.Availability.Start, item.Availability.End);

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
    }
}
