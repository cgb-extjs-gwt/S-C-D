using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_05
{
    public class Migration_2020_05_29_14_30 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public Migration_2020_05_29_14_30(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public int Number => 177;
        public string Description => "Add stdw duration, stdw location to global support pack reports";
        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-05-29-13-53.sql");
        }
    }
}
