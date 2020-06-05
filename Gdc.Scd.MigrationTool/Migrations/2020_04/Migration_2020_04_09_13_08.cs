using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_09_13_08 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 165;

        public string Description => "Add SODA hw proactive output";

        public Migration_2020_04_09_13_08(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-04-09-13-08.sql");
        }
    }
}
