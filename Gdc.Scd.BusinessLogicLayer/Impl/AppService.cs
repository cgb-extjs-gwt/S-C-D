using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Dto;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class AppService : IAppService
    {
        private static readonly string[] costElementPermissions = new[]
        {
            PermissionConstants.Admin,
            PermissionConstants.Approval,
            PermissionConstants.OwnApproval
        };

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
                UserRoles = this.GetRoleDtos(user),
                AppVersion = ConfigurationManager.AppSettings["ApplicationVersion"]
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
            var userPermissions = new HashSet<string>(user.Permissions.Select(permission => permission.Name));
            var isAddingCostElement = costElementPermissions.Any(permission => userPermissions.Contains(permission));

            foreach (var costBlock in this.meta.CostBlocks)
            {
                var usingInfo = new UsingInfo();
                var costElementDtos = new List<CostElementDto>();

                foreach (var costElement in costBlock.CostElements)
                {
                    var isManualInput = costElement.InputType == InputType.Manually || costElement.InputType == InputType.ManualyAutomaticly;
                    var isReadonly = costElement.InputType == InputType.AutomaticallyReadonly;

                    var costElementDto = new CostElementDto
                    {
                        UsingInfo = new UsingInfo
                        {
                            IsUsingCostEditor = (isManualInput || isReadonly) && this.ContainsRole(costElement.CostEditorRoles, user),
                            IsUsingTableView = isManualInput && this.ContainsRole(costElement.TableViewRoles, user)
                        }
                    };

                    costElementDto.UsingInfo.IsUsingCostImport =
                        costElementDto.UsingInfo.IsAnyUsing() &&
                        costElement.HasInputLevel(MetaConstants.WgInputLevelName) &&
                        costElement.InputType != InputType.AutomaticallyReadonly;

                    if (isAddingCostElement || costElementDto.UsingInfo.IsUsingCostEditor || costElementDto.UsingInfo.IsUsingTableView)
                    {
                        costElementDto.SetInputLevels(this.BuildInputLevelDtos(costElement));

                        this.Copy(costElement, costElementDto, nameof(CostElementDto.InputLevels));

                        costElementDtos.Add(costElementDto);

                        usingInfo.IsUsingCostEditor = usingInfo.IsUsingCostEditor || costElementDto.UsingInfo.IsUsingCostEditor;
                        usingInfo.IsUsingTableView = usingInfo.IsUsingTableView || costElementDto.UsingInfo.IsUsingTableView;
                        usingInfo.IsUsingCostImport = usingInfo.IsUsingCostImport || costElementDto.UsingInfo.IsUsingCostImport;
                    }
                }

                if (costElementDtos.Count > 0)
                {
                    var costBlockDto = this.BuildCostBlockDto(costBlock, costElementDtos, usingInfo);

                    costBlockDtos.Add(costBlockDto);
                }
            }

            return this.BuildDomainMetaDto(this.meta, costBlockDtos);
        }

        private MetaCollection<InputLevelDto> BuildInputLevelDtos(CostElementMeta costElementMeta)
        {
            var inputLevelDtos = new MetaCollection<InputLevelDto>();
            var index = 0;

            foreach (var inputLevelInfo in costElementMeta.InputLevelMetaInfos)
            {
                var inputLevelDto = this.Copy<InputLevelDto>(inputLevelInfo.InputLevel, nameof(InputLevelDto.LevelNumber));

                inputLevelDto.LevelNumber = index++;
                inputLevelDto.Hide = inputLevelInfo.Hide;

                var inputLevelMeta = costElementMeta.GetFilterInputLevel(inputLevelInfo.InputLevel.Id);

                if (inputLevelMeta != null)
                {
                    inputLevelDto.HasFilter = true;
                    inputLevelDto.FilterName = inputLevelMeta.Name;
                }

                inputLevelDtos.Add(inputLevelDto);
            }

            return inputLevelDtos;
        }

        private CostBlockDto BuildCostBlockDto(
            CostBlockMeta costBlock, 
            IEnumerable<CostElementDto> costElementDtos,
            UsingInfo usingInfo)
        {
            var costBlockDto = new CostBlockDto
            {
                CostElements = new MetaCollection<CostElementDto>(costElementDtos),
                UsingInfo = usingInfo
            };

            this.Copy(costBlock, costBlockDto, nameof(CostBlockDto.CostElements));

            return costBlockDto;
        }

        private DomainMetaDto BuildDomainMetaDto(DomainMeta domainMeta, IEnumerable<CostBlockDto> costBlockDtos)
        {
            var applicationInfos = new Dictionary<string, UsingInfo>();

            foreach(var costBlockDto in costBlockDtos)
            {
                foreach(var applicationId in costBlockDto.ApplicationIds)
                {
                    if (applicationInfos.TryGetValue(applicationId, out var usingInfo))
                    {
                        applicationInfos[applicationId] = new UsingInfo
                        {
                            IsUsingCostEditor = usingInfo.IsUsingCostEditor || costBlockDto.UsingInfo.IsUsingCostEditor,
                            IsUsingTableView = usingInfo.IsUsingTableView || costBlockDto.UsingInfo.IsUsingTableView,
                            IsUsingCostImport = usingInfo.IsUsingCostImport || costBlockDto.UsingInfo.IsUsingCostImport
                        };
                    }
                    else
                    {
                        applicationInfos[applicationId] = costBlockDto.UsingInfo;
                    }
                }
            }

            var applications = new List<ApplicationDto>();

            foreach(var applicationInfo in applicationInfos)
            {
                var applicationDto = this.Copy<ApplicationDto>(domainMeta.Applications[applicationInfo.Key]);

                applicationDto.UsingInfo = applicationInfo.Value;

                if (applicationDto.UsingInfo.IsAnyUsing())
                {
                    applications.Add(applicationDto);
                }
            }

            var domainMetaDto = new DomainMetaDto
            {
                CostBlocks = new MetaCollection<CostBlockDto>(costBlockDtos),
                Applications = new MetaCollection<ApplicationDto>(applications)
            };

            this.Copy(domainMeta, domainMetaDto, nameof(DomainMetaDto.CostBlocks), nameof(DomainMetaDto.Applications));

            return domainMetaDto;
        }

        private bool ContainsRole(HashSet<string> roleNames, User user)
        {
            return roleNames != null && user.Roles.Any(role => roleNames.Contains(role.Name));
        }

        private void Copy(object source, object target, params string[] ignoreProperties)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            var targetProperties = 
                target.GetType()
                      .GetProperties(bindingFlags)
                      .Where(property => property.SetMethod != null);

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
