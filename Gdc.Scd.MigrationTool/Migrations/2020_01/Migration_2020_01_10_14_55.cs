using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_01_10_14_55 : IMigrationAction
    {
        private readonly DomainEnitiesMeta meta;

        private readonly IRepositorySet repositorySet;

        public int Number => 777777777;

        public string Description => "Performance";

        public Migration_2020_01_10_14_55(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
            this.meta = meta;
        }

        public void Execute()
        {
            var queries =
                this.meta.RelatedItemsHistories.Select(
                    meta => Sql.CreateIndexIfNotExists(
                        $"IX_{meta.Schema}_{meta.Name}_{meta.CostBlockHistoryField.Name}", 
                        meta, 
                        new[] { new IndexColumn(meta.CostBlockHistoryField.Name) }));

            this.repositorySet.ExecuteSql(Sql.Queries(queries));
        }
    }
}
