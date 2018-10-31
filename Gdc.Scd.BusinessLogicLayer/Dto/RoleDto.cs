using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Dto
{
    public class RoleDto
    {
        public string Name { get; set; }

        public bool IsGlobal { get; set; }

        public NamedId Country { get; set; }

        public List<string> Permissions { get; set; }
    }
}
