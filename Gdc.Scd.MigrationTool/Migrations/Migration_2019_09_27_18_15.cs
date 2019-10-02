using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_09_27_18_15 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 122;

        public string Description => "Add relation between license and fsp";

        public Migration_2019_09_27_18_15()
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-09-27-18-13.sql");
        }
    }
}
