using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_01
{
    public class Migration_2020_01_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 140;

        public string Description => "Add local currency to Standard Report";

        public Migration_2020_01_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-20-20-06.sql");
        }
    }
}
