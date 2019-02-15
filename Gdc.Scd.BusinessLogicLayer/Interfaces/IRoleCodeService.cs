using Gdc.Scd.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IRoleCodeService
    {
        bool Deactivate(RoleCode roleCode);
        bool Deactivate(IEnumerable<RoleCode> roleCode);
        Task<RoleCode[]> GetAllActive();
        void Save(IEnumerable<RoleCode> roleCodes);
    }
}
