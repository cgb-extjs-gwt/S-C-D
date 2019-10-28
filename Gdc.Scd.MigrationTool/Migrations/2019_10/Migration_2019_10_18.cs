using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_10_18 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 125;

        public string Description => "Add ServiceTypes Column to Wg Table";

        public Migration_2019_10_18(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-10-18-17-50.sql");
        }
    }
}
