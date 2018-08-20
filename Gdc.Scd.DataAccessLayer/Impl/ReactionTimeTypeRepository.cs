using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ReactionTimeTypeRepository : EntityFrameworkRepository<ReactionTimeType>
    {
        public ReactionTimeTypeRepository(EntityFrameworkRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public override void Save(ReactionTimeType item)
        {
            this.SetAddOrUpdateState(item.ReactionTime);
            this.SetAddOrUpdateState(item.ReactionType);

            base.Save(item);
        }
    }
}
