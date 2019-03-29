using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_29_12_32 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 63;

        public string Description => "Enable Triggers use calculated PK column";

        public Migration_2019_03_29_12_32(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-29-12-21.sql");
        }
    }
}
