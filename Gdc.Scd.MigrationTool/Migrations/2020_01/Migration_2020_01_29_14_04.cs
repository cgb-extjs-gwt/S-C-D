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

        public string Description => "Add 'LastModification' and 'PreviousVersion' columns in costblocks";

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
                    BuildCheckCostBlockPrimaryKeyQuery(costBlock),
                    new AlterTableSqlBuilder(costBlock)
                    {
                        Query = new AddColumnsSqlBuilder
                        {
                            Columns = new ISqlBuilder[]
                            {
                                new SimpleColumnMetaSqlBuilder(costBlock.LastCoordinateModificationDateField),
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

            ISqlBuilder BuildCheckCostBlockPrimaryKeyQuery(CostBlockEntityMeta costBlock)
            {
                var primaryKeyNotExistsQuery =
                    SqlOperators.NotExists(
                        Sql.Select()
                           .From("TABLE_CONSTRAINTS", "INFORMATION_SCHEMA")
                           .Where(new Dictionary<string, IEnumerable<object>>
                           {
                               ["CONSTRAINT_TYPE"] = new object[] { "PRIMARY KEY" },
                               ["TABLE_SCHEMA"] = new object[] { costBlock.Schema },
                               ["TABLE_NAME"] = new object[] { costBlock.Name },
                           }));

                var createPrimaryKeyQuery = Sql.Queries(new ISqlBuilder[]
                {
                    new DropIndexSqlBuilder($"IX_{costBlock.Schema}_{costBlock.Name}", costBlock),
                    new CreateColumnConstraintMetaSqlBuilder
                    {
                        Meta = costBlock,
                        Field = IdFieldMeta.DefaultId
                    }
                });

                return
                    Sql.If(primaryKeyNotExistsQuery, createPrimaryKeyQuery)
                       .ToSqlBuilder();
            }
        }
    }
}
