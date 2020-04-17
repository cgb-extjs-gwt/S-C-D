using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_17_15_11 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 168;

        public string Description => "Change release process, add proactive release, reactive realease";

        public Migration_2020_04_17_15_11(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-04-17-15-11.sql");
        }
    }
}
