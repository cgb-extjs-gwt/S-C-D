using System.Collections.Generic;
using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.Core.Meta.Dto;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class AppData
    {
        public DomainMetaDto Meta { get; set; }

        public IEnumerable<RoleDto> UserRoles { get; set; }
    }
}
