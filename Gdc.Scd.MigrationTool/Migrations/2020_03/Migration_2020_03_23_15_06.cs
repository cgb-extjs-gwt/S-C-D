using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_03_23_15_06 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;
        private readonly DomainEnitiesMeta meta;

        public int Number => 164;

        public string Description => "Change LOCK_ESCALATION in Cost Block tables";

        public Migration_2020_03_23_15_06(IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
            this.meta = meta;
        }

        public void Execute()
        {
            var queries =
                this.meta.CostBlocks.Select(costBlock => new AlterTableSqlBuilder(costBlock)
                {
                    Query = new SetLockEscalationSqlBuilder
                    {
                        LockEscalationType = LockEscalationType.Disable
                    }
                });

            this.repositorySet.ExecuteSql(Sql.Queries(queries));
        }
    }
}
