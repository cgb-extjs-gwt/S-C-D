using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_13 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 9;

        public string Description => "Replace Year to Duration Dependency for Software and Solution Cost.";

        public Migration_2019_02_13(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-13.sql");
        }
    }
}
