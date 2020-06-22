using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ProjectService : DomainService<Project>, IProjectService
    {
        private static readonly PeriodConverter periodConverter = new PeriodConverter();
        private static readonly PeriodService periodService = new PeriodService();

        private readonly IProjectRepository projectRepository;
        private readonly IUserService userService;
        private readonly IDomainService<Wg> wgService;
        private readonly IDomainService<Country> countryService;
        private readonly IDomainService<ReactionType> reactionTypeService;
        private readonly IDomainService<ServiceLocation> serviceLocationService;
        private readonly IDomainService<Currency> currencyService;

        public ProjectService(
            IRepositorySet repositorySet, 
            IProjectRepository projectRepository,
            IUserService userService,
            IDomainService<Wg> wgService,
            IDomainService<Country> countryService,
            IDomainService<ReactionType> reactionTypeService,
            IDomainService<ServiceLocation> serviceLocationService,
            IDomainService<Currency> currencyService)
            : base(repositorySet)
        {
            this.projectRepository = projectRepository;
            this.userService = userService;
            this.wgService = wgService;
            this.countryService = countryService;
            this.reactionTypeService = reactionTypeService;
            this.serviceLocationService = serviceLocationService;
            this.currencyService = currencyService;
        }

        public IQueryable<ProjectItem> GetProjectItems(long projectId)
        {
            return this.projectRepository.GetProjectItems(projectId);
        }

        public ProjectItemEditData GetProjectItemEditData()
        {
            return new ProjectItemEditData
            {
                Wgs = GetWgs(),
                Countries = this.countryService.GetAll().Where(country => country.IsMaster).Select(GetNamedId).ToArray(),
                ReactionTypes = this.reactionTypeService.GetAll().Select(GetNamedId).ToArray(),
                ServiceLocations = this.serviceLocationService.GetAll().Select(GetNamedId).ToArray(),
                ReinsuranceCurrencies = this.currencyService.GetAll().ToArray(),
                ReactionTimePeriods = periodService.GetPeriods(PeriodType.Minutes, PeriodType.Hours),
                DurationPeriods = periodService.GetPeriods(PeriodType.Months, PeriodType.Years),
            };

            NamedId[] GetWgs()
            {
                return
                    this.wgService.GetAll()
                                  .Include(wg => wg.Sog)
                                  .Where(wg => wg.WgType == WgType.Por)
                                  .ToArray()
                                  .Select(wg => 
                                  {
                                      var sog = wg.Sog == null ? string.Empty : $"({wg.Sog.Name})";

                                      return new NamedId(wg.Id, $"{wg.Name} {sog}");
                                  })
                                  .ToArray();
            }

            NamedId GetNamedId(NamedId item)
            {
                return new NamedId(item.Id, item.Name);
            }
        }

        public override Project Get(long id)
        {
            var project = base.Get(id);

            foreach (var projectItem in project.ProjectItems)
            {
                projectItem.Duration.Value = periodConverter.Convert(
                    projectItem.Duration.Months,
                    PeriodType.Months,
                    projectItem.Duration.PeriodType);

                if (projectItem.ReactionTime.Minutes.HasValue)
                {
                    projectItem.ReactionTime.Value = periodConverter.Convert(
                        projectItem.ReactionTime.Minutes.Value,
                        PeriodType.Minutes,
                        projectItem.ReactionTime.PeriodType.Value);
                }
            }

            return project;
        }

        public override IQueryable<Project> GetAll()
        {
            return base.GetAll().Include(project => project.User);
        }

        public Project SaveWithInterpolation(Project item)
        {
            this.Save(item);
            this.Interpolate(new[] { item });

            item = this.Get(item.Id);

            return item;
        }

        protected override void InnerSave(Project project)
        {
            if (this.projectRepository.IsNewItem(project))
            {
                project.CreationDate = DateTime.UtcNow;
                project.UserId = this.userService.GetCurrentUser().Id;
            }

            if (project.ProjectItems != null)
            {
                foreach (var projectItem in project.ProjectItems)
                {
                    projectItem.Availability.Name = projectItem.Availability.ToString();
                    projectItem.Duration.Name = periodConverter.GetPeriodName(projectItem.Duration.Months, PeriodType.Months, projectItem.Duration.PeriodType);
                    projectItem.ReactionTime.Name = periodConverter.GetPeriodName(projectItem.ReactionTime.Minutes, PeriodType.Minutes, projectItem.ReactionTime.PeriodType);

                    if (projectItem.Duration.Value.HasValue)
                    {
                        projectItem.Duration.Months = periodConverter.Convert(
                            projectItem.Duration.Value.Value,
                            projectItem.Duration.PeriodType,
                            PeriodType.Months);
                    }

                    if (projectItem.ReactionTime.Value.HasValue)
                    {
                        projectItem.ReactionTime.Minutes = periodConverter.Convert(
                            projectItem.ReactionTime.Value.Value,
                            projectItem.ReactionTime.PeriodType.Value,
                            PeriodType.Minutes);
                    }
                }
            }

            base.InnerSave(project);
        }

        private void Interpolate(IEnumerable<Project> projects)
        {
            var projectItemIds =
                projects.Where(project => project.ProjectItems != null)
                        .SelectMany(project => project.ProjectItems)
                        .Where(projectItem => projectItem.IsRecalculation)
                        .Select(projectItem => projectItem.Id)
                        .ToArray();

            this.projectRepository.Interpolate(projectItemIds);
        }

        private class PeriodInfo
        {
            public int Minutes { get; }

            public string Caption { get; }

            public PeriodInfo(int devider, string caption)
            {
                this.Minutes = devider;
                this.Caption = caption;
            }
        }

        private class PeriodConverter
        {
            private const int MinutesInHour = 60;
            private const int MinutesInDay = MinutesInHour * 24;
            private const int MinutesInWeek = MinutesInDay * 7;
            private const int MinutesInMonth = MinutesInDay * 30;
            private const int MinutesInYear = MinutesInMonth * 12;

            private readonly Dictionary<PeriodType, PeriodInfo> periodInfos = new Dictionary<PeriodType, PeriodInfo>
            {
                [PeriodType.Minutes] = new PeriodInfo(1, "mins"),
                [PeriodType.Hours] = new PeriodInfo(MinutesInHour, "hours"),
                [PeriodType.Days] = new PeriodInfo(MinutesInDay, "days"),
                [PeriodType.Weeks] = new PeriodInfo(MinutesInWeek, "weeks"),
                [PeriodType.Months] = new PeriodInfo(MinutesInMonth, "months"),
                [PeriodType.Years] = new PeriodInfo(MinutesInYear, "years"),
            };

            public int Convert(int value, PeriodType sourceType, PeriodType targetType)
            {
                var source = this.periodInfos[sourceType];
                var target = this.periodInfos[targetType];

                return this.Convert(value, source, target);
            }

            public string GetPeriodName(int value, PeriodType sourceType, PeriodType targetType)
            {
                var source = this.periodInfos[sourceType];
                var target = this.periodInfos[targetType];

                var convertedValue = this.Convert(value, source, target);

                return $"{convertedValue} {target.Caption}";
            }

            public string GetPeriodName(int? value, PeriodType? sourceType, PeriodType? targetType)
            {
                return value.HasValue
                    ? this.GetPeriodName(value.Value, sourceType.Value, targetType.Value)
                    : "none";
            }

            private int Convert(int value, PeriodInfo source, PeriodInfo target)
            {
                return value * source.Minutes / target.Minutes;
            }
        }

        private class PeriodService
        {
            private readonly Dictionary<PeriodType, NamedId> periods = new Dictionary<PeriodType, NamedId>();

            public PeriodService()
            {
                this.AddPeriod(PeriodType.Minutes, "min");
                this.AddPeriod(PeriodType.Hours, "h");
                this.AddPeriod(PeriodType.Months, "m");
                this.AddPeriod(PeriodType.Years, "y");
            }

            public NamedId[] GetPeriods(params PeriodType[] periodTypes)
            {
                return periodTypes.Select(periodType => this.periods[periodType]).ToArray();
            }

            private void AddPeriod(PeriodType periodType, string name)
            {
                var period = new NamedId
                {
                    Id = (long)periodType,
                    Name = name
                };

                this.periods.Add(periodType, period);
            }
        }
    }
}
