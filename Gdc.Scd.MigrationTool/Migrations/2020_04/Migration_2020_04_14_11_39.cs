using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_14_11_39 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 166;

        public string Description => "Change release process, add Reactive manual";

        public Migration_2020_04_14_11_39(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-04-14-11-39.sql");
            this.repositorySet.ExecuteFromFile("2020-04-14-11-39-2.sql");
        }
    }
}
