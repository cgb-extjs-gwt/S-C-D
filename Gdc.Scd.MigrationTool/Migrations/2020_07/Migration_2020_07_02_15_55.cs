using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_07_02_15_55 : IMigrationAction
    {
		private readonly IRepositorySet repositorySet;

		public string Description => "Project Calculator. Interpolation update";

        public int Number => 184;

        public Migration_2020_07_02_15_55(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
			this.repositorySet.ExecuteFromFile("2020-06-29-14-45-tables.sql");
            this.repositorySet.ExecuteFromFile("2020-07-02-15-55-interpolation.sql");
        }
    }
}
