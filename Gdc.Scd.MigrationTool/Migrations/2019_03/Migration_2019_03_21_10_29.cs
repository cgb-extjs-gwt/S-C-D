using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_21_10_29 : IMigrationAction
    {
        private readonly IDomainService<RolePermission> rolePemissionService;

        private readonly IDomainService<Role> roleService;

        public int Number => 49;

        public string Description => "Adding permission 'ApprovalShowAllItems'";

        public Migration_2019_03_21_10_29(IDomainService<RolePermission> rolePemissionService, IDomainService<Role> roleService)
        {
            this.rolePemissionService = rolePemissionService;
            this.roleService = roleService;
        }

        public void Execute()
        {
            var adminRole = this.roleService.GetAll().Where(role => role.Name == "SCD Admin").First();

            this.rolePemissionService.Save(new RolePermission
            {
                Role = adminRole,
                Permission = new Permission
                {
                    Name = PermissionConstants.ApprovalShowAllItems
                }
            });
        }
    }
}
