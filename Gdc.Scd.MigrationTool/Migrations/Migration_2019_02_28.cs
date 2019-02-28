using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_28 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 24;

        public string Description => "Enable/Disable triggers Stored Procedures";

        public Migration_2019_02_28(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-28-16-17.sql");
        }
    }
}
