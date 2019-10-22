using System.Linq;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_08_09_37 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 98;

        public string Description => "Hdd retension parameter report permission";

        public Migration_2019_05_08_09_37(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var excludedRoles = new[]
            {
                "Country key user",
                "Country Finance Director",
                "Guest",
                "Portfolio"
            };

            var roleRepository = this.repositorySet.GetRepository<Role>();

            var roles = 
                roleRepository.GetAll()
                              .Where(role => !excludedRoles.Contains(role.Name))
                              .ToArray();

            var reportHddRetentionParameterPermission = new Permission { Name = PermissionConstants.ReportHddRetentionParameter };

            foreach (var role in roles)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    Role = role,
                    Permission = reportHddRetentionParameterPermission
                });
            }

            roleRepository.Save(roles);

            this.repositorySet.Sync();
        }
    }
}
