using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_06_29_14_45 : IMigrationAction
    {
		private readonly IRepositorySet repositorySet;

		public string Description => "Project Calculator";

        public int Number => 183;

        public Migration_2020_06_29_14_45(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
			this.repositorySet.ExecuteFromFile("2020-06-29-14-45-tables.sql");
            this.repositorySet.ExecuteFromFile("2020-06-29-14-45-interpolation.sql");
            this.repositorySet.ExecuteFromFile("2020-06-29-14-45-calculation.sql");
            this.repositorySet.ExecuteFromFile("2020-06-29-14-45-reports.sql");
        }
    }
}
