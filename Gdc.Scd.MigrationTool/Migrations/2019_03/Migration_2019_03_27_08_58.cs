using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_27_08_58 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 56;

        public string Description => "Add columns from logistics to hardware overview reports";

        public Migration_2019_03_27_08_58(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-27-08-58.sql");
        }
    }
}
