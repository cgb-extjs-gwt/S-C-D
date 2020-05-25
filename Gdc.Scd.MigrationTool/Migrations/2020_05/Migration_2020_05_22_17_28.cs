using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_05_22_17_28 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 173;

        public string Description => "Add report part table";

        public Migration_2020_05_22_17_28(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-05-22-17-28.sql");
        }
    }
}
