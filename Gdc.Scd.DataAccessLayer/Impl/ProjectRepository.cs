using Gdc.Scd.Core.Entities.ProjectCalculator;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
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

        public IQueryable<ProjectItem> GetProjectItems(long projectId)
        {
            return this.GetProjectItems(new[] { projectId });
        }

        public void Interpolate(long[] projectItemIds)
        {
            var projectItemIdsParam = DbParameterBuilder.CreateListID("projectItemIds", projectItemIds);

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
            var projectItems = itemsUpdateProjects.SelectMany(project => project.ProjectItems).ToArray();

            this.SetAddOrUpdateStateCollection(projectItems);

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

            base.Save(projectArray);
        }

        public override void Delete(long id)
        {
            this.Delete(new[] { id });
        }

        public override void Delete(IEnumerable<long> ids)
        {
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
