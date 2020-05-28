using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_05
{
    public class Migration_2020_05_28_17_24 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public Migration_2020_05_28_17_24(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public int Number => 176;
        public string Description => "Add risk stdw to hw calc report";
        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-05-28-17-21.sql");
        }
    }
}
