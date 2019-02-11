using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class RoleCodeService : IRoleCodeService
    {
        private readonly IDomainService<Wg> wgService;
        private readonly IDomainService<RoleCode> roleService;

        private const string StillReferencedMessage = "Role code(s) cannot be deleted because it is still referenced by warranty group(s).";

        public RoleCodeService(IDomainService<Wg> wgService, IDomainService<RoleCode> roleService)
        {
            this.wgService = wgService;
            this.roleService = roleService;
        }

        public bool Deactivate(RoleCode roleCode)
        {
            if (!wgService.GetAll().Where(x => x.RoleCodeId == roleCode.Id).Any())
            {
                roleCode.Deactivated = true;
                roleService.Save(roleCode);
                return true;
            }
            else
            {
                throw new Exception(StillReferencedMessage);
            }
        }

        public bool Deactivate(IEnumerable<RoleCode> roleCodes)
        {
            var roleCodeIds = roleCodes.Select(x => x.Id).ToList();

            if (!wgService.GetAll().Where(x => x.RoleCodeId!=null && roleCodeIds.Contains(x.RoleCodeId ?? 0)).Any())
            {
                foreach(var roleCode in roleCodes)
                {
                    roleCode.Deactivated = true;
                    roleService.Save(roleCode);
                }
                return true;
            }
            else
            {
                throw new Exception(StillReferencedMessage);
            }
        }

        public Task<RoleCode[]> GetAllActive()
        {
            return roleService.GetAll().Where(x => !x.Deactivated).GetAsync();
        }
    }
}
