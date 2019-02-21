using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_21_2 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 18;

        public string Description => "Some reports are now in local currency";

        public Migration_2019_02_21_2(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-20-17-11.sql");
        }
    }
}
