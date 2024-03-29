﻿using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_22_17_48 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 54;

        public string Description => "Split Material Cost In Warranty into EMEIA and Non EMEIA";

        public Migration_2019_03_22_17_48(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-22-17-45.sql");
            repositorySet.ExecuteFromFile("2019-03-25-12-55.sql");
        }
    }
}
