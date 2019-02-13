using Gdc.Scd.Core.Entities.Calculation;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class HddRetentionManualCostRepository : EntityFrameworkRepository<HddRetentionManualCost>
    {
        public HddRetentionManualCostRepository(
                EntityFrameworkRepositorySet repositorySet
            ) : base(repositorySet) { }

        public override void Save(HddRetentionManualCost item)
        {
            item.ChangeUserId = item.ChangeUser.Id;
            SetAddOrUpdateState(item.ChangeUser);

            base.Save(item);
        }
    }
}
