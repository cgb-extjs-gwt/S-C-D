using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2019_12
{
    public class Migration_2019_12_23_19_32 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 134;

        public string Description => "Added IB per Country, IB per PLA, Time and Material Share to Calc Parameter Overview Report";

        public Migration_2019_12_23_19_32(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-12-23-17-02.sql");
        }
    }
}
