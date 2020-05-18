using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_05_15_10_14 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 172;

        public string Description => "Add new archives";

        public Migration_2020_05_15_10_14(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-05-15-10-14.sql");
        }
    }
}
