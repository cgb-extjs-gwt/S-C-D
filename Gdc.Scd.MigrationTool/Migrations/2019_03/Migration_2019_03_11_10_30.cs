using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_11_10_30 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 33;

        public string Description => "Removing 'Approval' permission from 'PRS PSM' role";

        public Migration_2019_03_11_10_30(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var rolePermissionRepository = this.repositorySet.GetRepository<RolePermission>();

            var rolePermissions =
                rolePermissionRepository.GetAll()
                                        .Where(rolePerm => rolePerm.Role.Name == "PRS PSM" && rolePerm.Permission.Name == "Approval");

            foreach (var rolePermission in rolePermissions)
            {
                rolePermissionRepository.Delete(rolePermission.Id);
            }

            this.repositorySet.Sync();
        }
    }
}
