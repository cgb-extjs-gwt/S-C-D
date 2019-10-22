using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_18 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 82;

        public string Description => "Add Global Support Pack";

        public Migration_2019_04_18(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-17.sql");
        }
    }
}
