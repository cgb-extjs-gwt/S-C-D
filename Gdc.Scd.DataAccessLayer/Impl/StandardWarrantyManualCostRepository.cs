using Gdc.Scd.Core.Entities.Calculation;
using System;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class StandardWarrantyManualCostRepository : EntityFrameworkRepository<StandardWarrantyManualCost>
    {
        public StandardWarrantyManualCostRepository(
                EntityFrameworkRepositorySet repositorySet
            ) : base(repositorySet) { }

        public override void Save(StandardWarrantyManualCost item)
        {
            item.ChangeUserId = item.ChangeUser.Id;
            item.ChangeDate = DateTime.Now;

            this.SetAddOrUpdateState(item.ChangeUser);

            base.Save(item);
        }
    }
}
