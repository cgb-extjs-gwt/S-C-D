using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_12 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 3;

        public string Description => "Fix IsApplicable under Availability Fee";

        public Migration_2019_02_12(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var queries = new List<SqlHelper>();
            queries.AddRange(SqlFormatter.BuildFromFile(@"MigrationScripts\2019-02-12.sql"));
            foreach (var query in queries)
                this.repositorySet.ExecuteSql(query);
        }
    }
}
