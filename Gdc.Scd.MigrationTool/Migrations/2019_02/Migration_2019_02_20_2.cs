using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_20_2 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 17;

        public string Description => "Change columns in SW Price List and Software Price List Details";

        public Migration_2019_02_20_2(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-20-17-39.sql");
        }
    }
}
