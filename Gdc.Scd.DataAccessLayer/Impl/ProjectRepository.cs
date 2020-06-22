using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ProjectRepository : EntityFrameworkRepository<Project>, IProjectRepository
    {
        private readonly IRepository<ProjectItem> projectItemRepository;

        public ProjectRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
            this.projectItemRepository = repositorySet.GetRepository<ProjectItem>();
        }

        public override Project Get(long id)
        {
            return
                base.GetAll()
                    .Where(project => project.Id == id)
                    .Include(project => project.User)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.Availability)
                                                             .ThenInclude(avail => avail.Start)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.Availability)
                                                             .ThenInclude(avail => avail.End)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.ReactionTime)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.Duration)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.FieldServiceCost)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.Reinsurance)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.MarkupOtherCosts)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.LogisticsCosts)
                    .Include(project => project.ProjectItems).ThenInclude(projectItem => projectItem.AvailabilityFee)
                    .FirstOrDefault();
        }

        public IQueryable<ProjectItem> GetProjectItems(long projectId)
        {
            return this.GetProjectItems(new[] { projectId });
        }

        public void Interpolate(long[] projectItemIds)
        {
            var projectItemIdsParam = DbParameterBuilder.CreateListID("@projectItemIds", projectItemIds);

            this.repositorySet.ExecuteProc("[ProjectCalculator].[InterpolateProjects]", projectItemIdsParam);
        }

        public override void Save(Project item)
        {
            this.Save(new[] { item });
        }

        public override void Save(IEnumerable<Project> projects)
        {
            var projectArray = projects.ToArray();
            var itemsUpdateProjects = projectArray.Where(project => project.ProjectItems != null).ToArray();

            var oldProjects = itemsUpdateProjects.Where(project => !this.IsNewItem(project)).ToArray();
            var oldProjectItemIds =
                oldProjects.SelectMany(project => project.ProjectItems)
                           .Where(projectItem => !this.IsNewItem(projectItem))
                           .Select(projectItem => projectItem.Id)
                           .ToArray();

            var oldPorjectIds = oldProjects.Select(project => project.Id).ToArray();
            var deleteProjectItemIds =
                this.GetAll()
                    .Where(project => oldPorjectIds.Contains(project.Id))
                    .SelectMany(project => project.ProjectItems)
                    .Select(projectItem => projectItem.Id)
                    .Except(oldProjectItemIds)
                    .ToArray();

            this.projectItemRepository.Delete(deleteProjectItemIds);

            foreach (var project in projectArray)
            {
                base.Save(project);
            }

            this.SetAddOrUpdateStateCollection(itemsUpdateProjects.SelectMany(project => project.ProjectItems));
        }

        public override void Delete(long id)
        {
            this.Delete(new[] { id });
        }

        public override void Delete(IEnumerable<long> ids)
        {
            foreach (var id in ids)
            {
                base.Delete(id);
            }

            var projectItemIds =
                    this.GetProjectItems(ids)
                        .Select(projectItem => projectItem.Id);

            this.projectItemRepository.Delete(projectItemIds);
        }

        public override void DeleteAll()
        {
            base.DeleteAll();

            this.projectItemRepository.DeleteAll();
        }

        private IQueryable<ProjectItem> GetProjectItems(IEnumerable<long> projectIds)
        {
            return
                this.GetAll()
                    .Where(project => projectIds.Contains(project.Id))
                    .SelectMany(project => project.ProjectItems);
        }
    }
}
