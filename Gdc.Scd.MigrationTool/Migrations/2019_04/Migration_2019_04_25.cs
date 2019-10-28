using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_25 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 90;

        public string Description => "Add sw order number to SW Calculation output report";

        public Migration_2019_04_25(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-25-12-00.sql");
        }
    }
}
