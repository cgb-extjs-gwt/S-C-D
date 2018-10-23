using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class RolePermission : IIdentifiable
    {
        public long Id { get; set; }

        public Role Role { get; set; }

        public Permission Permission { get; set; }
    }
}
