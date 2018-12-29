using Gdc.Scd.Core.Entities.Calculation;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class HardwareManualCostRepository : EntityFrameworkRepository<HardwareManualCost>
    {
        public HardwareManualCostRepository(
                EntityFrameworkRepositorySet repositorySet
            ) : base(repositorySet) { }

        public override void Save(HardwareManualCost item)
        {
            item.ChangeUserId = item.ChangeUser.Id;

            this.SetAddOrUpdateState(item.ChangeUser);

            base.Save(item);
        }
    }
}
