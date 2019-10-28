using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_22 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 21;

        public string Description => "Updating combined views";

        public Migration_2019_02_22(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            IConfigureDatabaseHandler viewConfigureHandler = new ViewConfigureHandler(repositorySet)
            {
                IsAlterView = true
            };

            viewConfigureHandler.Handle();
        }
    }
}
