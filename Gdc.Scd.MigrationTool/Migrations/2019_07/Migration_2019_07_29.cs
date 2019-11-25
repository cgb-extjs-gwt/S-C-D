using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_07_29 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 114;

        public string Description => "Consider Manual TC TP Flag";

        public Migration_2019_07_29(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-07-29.sql");
        }
    }
}
