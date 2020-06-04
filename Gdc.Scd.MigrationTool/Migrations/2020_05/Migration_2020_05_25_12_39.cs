using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_05
{
    public class Migration_2020_05_25_12_39 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public Migration_2020_05_25_12_39(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public int Number => 175;
        public string Description => "Fix corrupted stored procedures and functions";
        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-05-25-12-38.sql");
        }
    }
}
