﻿using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_12_2 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 2;

        public string Description => "Change ProActive report";

        public Migration_2019_02_12_2(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var queries = SqlFormatter.BuildFromFile(@"MigrationScripts\2019-02-12_2.sql");
            foreach (var query in queries)
            {
                repositorySet.ExecuteSql(query);
            }
        }
    }
}
