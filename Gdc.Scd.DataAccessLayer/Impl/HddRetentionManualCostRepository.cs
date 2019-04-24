using Gdc.Scd.Core.Entities.Calculation;
using System;

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
            item.ChangeDate = DateTime.Now;
            SetAddOrUpdateState(item.ChangeUser);

            base.Save(item);
        }
    }
}
