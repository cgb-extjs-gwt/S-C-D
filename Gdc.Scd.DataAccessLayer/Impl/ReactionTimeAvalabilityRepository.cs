using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ReactionTimeAvalabilityRepository : EntityFrameworkRepository<ReactionTimeAvalability>
    {
        public ReactionTimeAvalabilityRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override void Save(ReactionTimeAvalability item)
        {
            this.SetAddOrUpdateState(item.ReactionTime);
            this.SetAddOrUpdateState(item.Availability);

            base.Save(item);
        }
    }
}
