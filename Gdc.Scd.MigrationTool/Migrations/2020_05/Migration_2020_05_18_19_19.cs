using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_05
{
    public class Migration_2020_05_18_19_19 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;
        private readonly IDataMigrator dataMigrator;
        private readonly DomainEnitiesMeta meta;


        public int Number => 173;

        public string Description => "Add Risk factor to Markup Standard Warranty";

        public Migration_2020_05_18_19_19(IDataMigrator dataMigrator,
            IRepositorySet repositorySet,
            DomainEnitiesMeta meta)
        {
            this.repositorySet = repositorySet;
            this.meta = meta;
            this.dataMigrator = dataMigrator;
        }

        public void Execute()
        {
            const string RiskStandardWarrantyCostElement = "RiskStandardWarranty";
            const string RiskFactorStandardWarrantyCostElement = "RiskFactorStandardWarranty";

            this.dataMigrator.AddCostElements(new[]
            {
                new CostElementInfo
                {
                    Meta = this.meta.CostBlocks[MetaConstants.HardwareSchema, "MarkupStandardWaranty"],
                    CostElementIds = new[] { RiskStandardWarrantyCostElement, RiskFactorStandardWarrantyCostElement }
                }
            });

            this.repositorySet.ExecuteFromFile("2020-05-18-19-39.sql");
        }
    }
}
