using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_21_12_22 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 50;

        public string Description => "Remove Occurency Column from Import Configuration";

        public Migration_2019_03_21_12_22(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var sql = @"
                ALTER TABLE [Import].[Configuration]
                DROP COLUMN [Occurancy]";

            this.repositorySet.ExecuteSql(sql);
        }
    }
}
