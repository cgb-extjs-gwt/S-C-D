using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class YearAvailabilityRepository : EntityFrameworkRepository<YearAvailability>
    {
        public YearAvailabilityRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override void Save(YearAvailability item)
        {
            this.SetAddOrUpdateState(item.Year);
            this.SetAddOrUpdateState(item.Availability);

            base.Save(item);
        }
    }
}
