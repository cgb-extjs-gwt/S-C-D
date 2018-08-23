using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ReactionTimeTypeAvalabilityRepository : EntityFrameworkRepository<ReactionTimeTypeAvalability>
    {
        public ReactionTimeTypeAvalabilityRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override void Save(ReactionTimeTypeAvalability item)
        {
            this.SetAddOrUpdateState(item.ReactionTime);
            this.SetAddOrUpdateState(item.ReactionType);
            this.SetAddOrUpdateState(item.Availability);

            base.Save(item);
        }
    }
}
