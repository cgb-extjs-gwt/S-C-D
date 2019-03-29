using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_28_15_50 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 60;

        public string Description => "Added Solution SOGs";

        public Migration_2019_03_28_15_50(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-28-13-52.sql");
        }
    }
}
