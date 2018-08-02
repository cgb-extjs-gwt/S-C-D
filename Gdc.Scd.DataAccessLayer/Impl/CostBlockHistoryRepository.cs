using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class CostBlockHistoryRepository : EntityFrameworkRepository<CostBlockHistory>
    {
        public CostBlockHistoryRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override void Save(CostBlockHistory item)
        {
            base.Save(item);

            this.AddOrUpdate(item.EditUser);
            this.AddOrUpdate(item.ApproveRejectUser);
        }
    }
}
