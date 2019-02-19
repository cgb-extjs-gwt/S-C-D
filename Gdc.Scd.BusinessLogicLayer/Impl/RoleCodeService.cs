using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
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
        public ICostBlockService CostBlockService { get; private set; }
        private readonly DomainEnitiesMeta meta;

        private const string StillReferencedMessage = "Role code(s) cannot be deleted because it is still referenced by warranty group(s).";

        public RoleCodeService(IDomainService<Wg> wgService, IDomainService<RoleCode> roleService, ICostBlockService CostBlockService,
            DomainEnitiesMeta meta)
        {
            this.wgService = wgService;
            this.roleService = roleService;
            this.CostBlockService = CostBlockService;
            this.meta = meta;
        }

        public bool Deactivate(RoleCode roleCode)
        {
            if (!wgService.GetAll().Where(x => x.RoleCodeId == roleCode.Id).Any())
            {
                roleCode.DeactivatedDateTime = DateTime.Now;
                roleCode.ModifiedDateTime = DateTime.Now;
                roleService.Save(roleCode);
                UpdateMeta();
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
                    roleCode.DeactivatedDateTime = DateTime.Now;
                    roleCode.ModifiedDateTime = DateTime.Now;
                    roleService.Save(roleCode);
                }
                UpdateMeta();
                return true;
            }
            else
            {
                throw new Exception(StillReferencedMessage);
            }
        }

        public Task<RoleCode[]> GetAllActive()
        {
            return roleService.GetAll().Where(x => !x.DeactivatedDateTime.HasValue).GetAsync();
        }

        public void Save(IEnumerable<RoleCode> roleCodes)
        {
            foreach(var roleCode in roleCodes)
            {
                if (roleCode.Id == 0) roleCode.CreatedDateTime = DateTime.Now;
                roleCode.ModifiedDateTime = DateTime.Now;
            }
            roleService.Save(roleCodes);
            UpdateMeta();
        }

        private void UpdateMeta()
        {
            var relatedMetas = meta.CostBlocks.Where(x => x.CoordinateFields.Where(r => r.ReferenceMeta.Name == MetaConstants.RoleCodeInputLevel).Any());
            CostBlockService.UpdateByCoordinates(relatedMetas);
        }
    }
}
