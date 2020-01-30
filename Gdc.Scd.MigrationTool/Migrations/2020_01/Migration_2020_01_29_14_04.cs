using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations._2020_01
{
    public class Migration_2020_01_29_14_04 : IMigrationAction
    {
        private readonly DomainEnitiesMeta meta;

        private readonly IRepositorySet repositorySet;

        public int Number => 777777777;

        public string Description => "Add 'PreviousVersion' column in costblocks";

        public Migration_2020_01_29_14_04(DomainEnitiesMeta meta, IRepositorySet repositorySet)
        {
            this.meta = meta;
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var queires = 
                this.meta.CostBlocks.SelectMany(costBlock => new ISqlBuilder[]
                {
                    BuildCheckCostBlockUniqueConstarintQuery(costBlock),
                    new AlterTableSqlBuilder(costBlock)
                    {
                        Query = new AddColumnsSqlBuilder
                        {
                            Columns = new ISqlBuilder[]
                            {
                                new ReferenceColumnMetaSqlBuilder(costBlock.PreviousVersionField)
                            }
                        }
                    },
                    new CreateColumnConstraintMetaSqlBuilder
                    {
                        Meta = costBlock,
                        Field = costBlock.PreviousVersionField.Name
                    }
                });

            this.repositorySet.ExecuteSql(Sql.Queries(queires));

            ISqlBuilder BuildCheckCostBlockUniqueConstarintQuery(CostBlockEntityMeta costBlock)
            {
                var uniqueNotExistsQuery =
                    SqlOperators.NotExists(
                        Sql.Select()
                           .From("TABLE_CONSTRAINTS", "INFORMATION_SCHEMA")
                           .Where(new Dictionary<string, IEnumerable<object>>
                           {
                               ["CONSTRAINT_TYPE"] = new object[] { "PRIMARY KEY", "UNIQUE" },
                               ["TABLE_SCHEMA"] = new object[] { costBlock.Schema },
                               ["TABLE_NAME"] = new object[] { costBlock.Name },
                           }))
                           .ToSqlBuilder();

                var createUniqueConstrainQuery = new AlterTableSqlBuilder(costBlock)
                {
                    Query = new RawSqlBuilder($"ADD CONSTRAINT AK_{costBlock.Schema}{costBlock.Name}_{costBlock.IdField.Name} UNIQUE ({costBlock.IdField.Name})")
                };

                return
                    Sql.If(uniqueNotExistsQuery, createUniqueConstrainQuery)
                       .ToSqlBuilder();
            }
        }
    }
}
