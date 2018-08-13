using System.Linq;
using Gdc.Scd.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockHistoryRepository : EntityFrameworkRepository<CostBlockHistory>
    {
        public CostBlockHistoryRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override IQueryable<CostBlockHistory> GetAll()
        {
            return base.GetAll().Include(history => history.EditUser).Include(history => history.ApproveRejectUser);
        }

        public override void Save(CostBlockHistory item)
        {
            this.SetAddOrUpdateState(item.EditUser);
            this.SetAddOrUpdateState(item.ApproveRejectUser);

            base.Save(item);
        }
    }
}
