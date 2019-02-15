using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_15 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 11;

        public string Description => "Display only Master Countries on Availability fee Admin";

        public Migration_2019_02_15(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-15.sql");
        }
    }
}
