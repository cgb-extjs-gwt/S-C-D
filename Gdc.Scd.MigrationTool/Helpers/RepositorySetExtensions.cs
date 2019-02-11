using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using System.Collections.Generic;
using System.IO;

namespace Gdc.Scd.MigrationTool
{
    public static class RepositorySetExtensions
    {
        public static void ExecuteFromFile(this IRepositorySet repositorySet, string fn)
        {
            fn = Path.Combine(@"MigrationScripts", fn);
            ExecuteQueries(repositorySet, SqlFormatter.BuildFromFile(fn));
        }

        public static void ExecuteQueries(this IRepositorySet repositorySet, IEnumerable<SqlHelper> queries)
        {
            foreach (var query in queries)
            {
                repositorySet.ExecuteSql(query);
            }
        }
    }
}
