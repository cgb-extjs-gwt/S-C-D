using System.Linq;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_12_10_33 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 37;

        public string Description => "Adding 'CalcResultSoftwareSolutionServiceCostNotApproved' permission for 'PRS PSM' role";

        public Migration_2019_03_12_10_33(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var roleRepository = this.repositorySet.GetRepository<Role>();
            var hddPermission = new Permission { Name = PermissionConstants.CalcResultHddServiceCostNotApproved };
            var softwarePermission = new Permission { Name = PermissionConstants.CalcResultSoftwareServiceCostNotApproved };

            var adminRole = roleRepository.GetAll().First(role => role.Name == "SCD Admin");

            adminRole.RolePermissions.Add(new RolePermission
            {
                Permission = hddPermission,
                Role = adminRole
            });

            adminRole.RolePermissions.Add(new RolePermission
            {
                Permission = softwarePermission,
                Role = adminRole
            });

            roleRepository.Save(adminRole);

            var prsPsmRole = roleRepository.GetAll().First(role => role.Name == "PRS PSM");

            prsPsmRole.RolePermissions.Add(new RolePermission
            {
                Permission = softwarePermission,
                Role = prsPsmRole
            });

            roleRepository.Save(prsPsmRole);

            this.repositorySet.Sync();
        }
    }
}
