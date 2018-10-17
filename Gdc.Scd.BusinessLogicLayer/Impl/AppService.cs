using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Dto;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class AppService : IAppService
    {
        private readonly DomainMeta meta;
        private readonly IUserService userService;

        public AppService(DomainMeta meta, IUserService userService)
        {
            this.meta = meta;
            this.userService = userService;
        }

        public AppData GetAppData()
        {
            var user = this.userService.GetCurrentUser();

            return new AppData
            {
                Meta = this.GetMetaDto(user),
                UserRoles = this.GetRoleDtos(user)
            };
        }

        private IEnumerable<RoleDto> GetRoleDtos(User user)
        {
            return user.UserRoles.Select(userRole => new RoleDto
            {
                Name = userRole.Role.Name,
                IsGlobal = userRole.Role.IsGlobal,
                Country = userRole.Country == null ? null : this.Copy<NamedId>(userRole.Country),
                Permissions = userRole.Role.Permissions.Select(permission => permission.Name).ToList()
            });
        }

        private DomainMetaDto GetMetaDto(User user)
        {
            var costBlockDtos = new List<CostBlockDto>();

            foreach (var costBlock in this.meta.CostBlocks)
            {
                var costElementDtos = new List<CostElementDto>();

                foreach (var costElement in costBlock.CostElements)
                {
                    var costElementDto = new CostElementDto
                    {
                        IsUsingCostEditor = 
                            costElement.CostEditorRoles == null || 
                            costElement.CostEditorRoles.Count == 0 || 
                            this.ContainsRole(costElement.CostEditorRoles, user),

                        IsUsingTableView = costElement.TableViewRoles != null && this.ContainsRole(costElement.TableViewRoles, user)
                    };

                    if (costElementDto.IsUsingCostEditor || costElementDto.IsUsingTableView)
                    {
                        this.Copy(costElement, costElementDto);

                        costElementDtos.Add(costElementDto);
                    }
                }

                if (costElementDtos.Count > 0)
                {
                    var costBlockDto = this.BuildCostBlockDto(costBlock, costElementDtos);

                    costBlockDtos.Add(costBlockDto);
                }
            }

            return this.BuildDomainMetaDto(this.meta, costBlockDtos);
        }

        private CostBlockDto BuildCostBlockDto(CostBlockMeta costBlock, IEnumerable<CostElementDto> costElementDtos)
        {
            var costBlockDto = new CostBlockDto
            {
                CostElements = new MetaCollection<CostElementDto>(costElementDtos)
            };

            this.Copy(costBlock, costBlockDto, nameof(CostBlockDto.CostElements));

            return costBlockDto;
        }

        private DomainMetaDto BuildDomainMetaDto(DomainMeta domainMeta, IEnumerable<CostBlockDto> costBlockDtos)
        {
            var applications =
                costBlockDtos.SelectMany(costBlockDto => costBlockDto.ApplicationIds)
                             .Select(applicationId => domainMeta.Applications[applicationId])
                             .Distinct()
                             .Where(application => application != null);

            var domainMetaDto = new DomainMetaDto
            {
                CostBlocks = new MetaCollection<CostBlockDto>(costBlockDtos),
                Applications = new MetaCollection<ApplicationMeta>(applications)
            };

            this.Copy(domainMeta, domainMetaDto, nameof(DomainMetaDto.CostBlocks), nameof(DomainMetaDto.Applications));

            return domainMetaDto;
        }

        private bool ContainsRole(HashSet<string> roleNames, User user)
        {
            return user.Roles.Any(role => roleNames.Contains(role.Name));
        }

        private void Copy(object source, object target, params string[] ignoreProperties)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            var targetProperties = (IEnumerable<PropertyInfo>)target.GetType().GetProperties(bindingFlags);

            if (ignoreProperties.Length > 0)
            {
                var ignorePropertyHashSet = new HashSet<string>(ignoreProperties);

                targetProperties = targetProperties.Where(property => !ignorePropertyHashSet.Contains(property.Name));
            }

            var sourceType = source.GetType();

            foreach (var targetProperty in targetProperties)
            {
                var sourceProperty = sourceType.GetProperty(targetProperty.Name, bindingFlags);

                if (sourceProperty != null && sourceProperty.PropertyType == targetProperty.PropertyType)
                {
                    var sourceValue = sourceProperty.GetValue(source);

                    targetProperty.SetValue(target, sourceValue);
                }
            }
        }

        private TTarget Copy<TTarget>(object source, params string[] ignoreProperties) where TTarget : new()
        {
            var target = new TTarget();

            this.Copy(source, target, ignoreProperties);

            return target;
        }
    }
}
