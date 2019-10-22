using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_27_16_56 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 106;

        public string Description => "Add IB Group column to country";

        public Migration_2019_05_27_16_56(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-27-16-52.sql");
        }
    }
}
