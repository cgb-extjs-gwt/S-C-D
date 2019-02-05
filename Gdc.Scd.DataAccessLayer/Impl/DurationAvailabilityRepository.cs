using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class DurationAvailabilityRepository : EntityFrameworkRepository<DurationAvailability>
    {
        public DurationAvailabilityRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override void Save(DurationAvailability item)
        {
            this.SetAddOrUpdateState(item.Year);
            this.SetAddOrUpdateState(item.Availability);

            base.Save(item);
        }
    }
}
