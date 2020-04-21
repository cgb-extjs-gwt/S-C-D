using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Impl;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_20_11_42 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;
        private readonly IMetaProvider metaProvider;
        private readonly IDataMigrator dataMigrator;

        public string Description => "AvailabilityFee country company split";

        public int Number => 170;

        public Migration_2020_04_20_11_42(            
            IRepositorySet repositorySet,
            IMetaProvider metaProvider,
            IDataMigrator dataMigrator)
        {
            this.repositorySet = repositorySet;
            this.metaProvider = metaProvider;
            this.dataMigrator = dataMigrator;
        }

        public void Execute()
        {
            var oldMeta = this.metaProvider.GetArchiveEntitiesMeta("DomainConfig_2020_04_20_11_42_old");
            var newMeta = this.metaProvider.GetArchiveEntitiesMeta("DomainConfig_2020_04_20_11_42_new");

            var oldCostBlock = oldMeta.CostBlocks[MetaConstants.HardwareSchema, "AvailabilityFeeCountryWg"];
            var newCostBlocks = new[]
            {
                newMeta.CostBlocks[MetaConstants.HardwareSchema, "AvailabilityFeeWgCountry"],
                newMeta.CostBlocks[MetaConstants.HardwareSchema, "AvailabilityFeeCountryCompany"]
            };

            this.dataMigrator.SplitCostBlock(oldCostBlock, newCostBlocks, newMeta, true);

            //[Hardware].[AvailabilityFeeView]
            this.repositorySet.ExecuteSql(@"
ALTER VIEW [Hardware].[AvailabilityFeeView] as 
    select   feeCountryWg.Country
           , feeCountryWg.Wg
           
           , case  when wg.WgType = 0 then 1 else 0 end as IsMultiVendor
           
           , feeCountryWg.InstalledBaseHighAvailability as IB
           , feeCountryWg.InstalledBaseHighAvailability_Approved as IB_Approved
           
           , feeCountryCompany.TotalLogisticsInfrastructureCost          / er.Value as TotalLogisticsInfrastructureCost
           , feeCountryCompany.TotalLogisticsInfrastructureCost_Approved / er.Value as TotalLogisticsInfrastructureCost_Approved
           
           , case when wg.WgType = 0 then feeCountryCompany.StockValueMv          else feeCountryCompany.StockValueFj          end / er.Value as StockValue
           , case when wg.WgType = 0 then feeCountryCompany.StockValueMv_Approved else feeCountryCompany.StockValueFj_Approved end / er.Value as StockValue_Approved
           
           , feeCountryCompany.AverageContractDuration
           , feeCountryCompany.AverageContractDuration_Approved
           
           , case when feeCountryWg.JapanBuy = 1          then feeWg.CostPerKitJapanBuy else feeWg.CostPerKit end as CostPerKit
           , case when feeCountryWg.JapanBuy_Approved = 1 then feeWg.CostPerKitJapanBuy else feeWg.CostPerKit end as CostPerKit_Approved
           
           , feeWg.MaxQty

    from Hardware.AvailabilityFeeWgCountry AS feeCountryWg
    JOIN InputAtoms.Wg wg on wg.Id = feeCountryWg.Wg
	JOIN Hardware.AvailabilityFeeWg AS feeWg ON feeCountryWg.Wg = feeWg.Wg
    JOIN Hardware.AvailabilityFeeCountryCompany AS feeCountryCompany ON feeCountryWg.Country = feeCountryCompany.Country AND wg.CompanyId = feeCountryCompany.Company
    JOIN InputAtoms.Country c on c.Id = feeCountryWg.Country
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where 
		feeCountryWg.DeactivatedDateTime is null and 
		feeWg.DeactivatedDateTime is null and 
        feeCountryCompany.DeactivatedDateTime is null and 
		wg.DeactivatedDateTime is null
");

            //Create triggers
            foreach (var costBlock in newCostBlocks)
            {
                this.repositorySet.ExecuteSql($@"
                    CREATE TRIGGER [{costBlock.Schema}].[{costBlock.Name}Updated]
                    ON [{costBlock.Schema}].[{costBlock.Name}]
                    AFTER INSERT, UPDATE
                    AS BEGIN
                        EXEC [Hardware].[UpdateAvailabilityFee];
                    END");
            }

            this.repositorySet.ExecuteProc("[Hardware].[UpdateAvailabilityFee]");
        }
    }
}
