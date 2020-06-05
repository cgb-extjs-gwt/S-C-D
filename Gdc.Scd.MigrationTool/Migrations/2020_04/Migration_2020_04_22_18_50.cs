using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_22_18_50 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public string Description => "AvailabilityFee calculation fix";

        public int Number => 171;

        public Migration_2020_04_22_18_50(           
            IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            //[Report].[FlatFeeReport]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Report].[FlatFeeReport]
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    select    c.Name as Country
            , c.CountryGroup
            , wg.Name as Wg
            , wg.Description as WgDescription
        
            , c.Currency
            , calc.Fee_Approved * er.Value / 12 as Fee
        
            , fee2.InstalledBaseHighAvailability_Approved as IB
            , fee.CostPerKit as CostPerKit
            , fee.CostPerKitJapanBuy as CostPerKitJapanBuy
            , fee.MaxQty as MaxQty
            , fee2.JapanBuy_Approved as JapanBuy

    from Hardware.AvailabilityFeeWg fee
    join InputAtoms.Wg wg on wg.id = fee.Wg
    
    join Hardware.AvailabilityFeeWgCountry fee2 on fee2.Wg = wg.Id and fee2.DeactivatedDateTime is null
    left join Hardware.AvailabilityFeeCalc calc on calc.Wg = fee.Wg and calc.Country = fee2.Country 
    
    join InputAtoms.CountryView c on c.Id = fee2.Country
    
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where     fee.DeactivatedDateTime is null
          and (@cnt is null or fee2.Country = @cnt)
          and (@wg is null or fee.Wg = @wg)

)");

            //[Archive].[GetWg]
            this.repositorySet.ExecuteSql(@"
ALTER function [Archive].[GetWg](@software bit)
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , Description nvarchar(255)
    , Pla nvarchar(255)
    , ClusterPla nvarchar(255)
    , Sog nvarchar(255)
	, CompanyId bigint null
)
begin

    insert into @tbl
    select  wg.Id
          , wg.Name as Wg
          , wg.Description
          , pla.Name as Pla
          , cpla.Name as ClusterPla
          , sog.Name as Sog
		  , wg.CompanyId
    from InputAtoms.Wg wg 
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId
    left join InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.Deactivated = 0
          and (@software is null or wg.IsSoftware = @software)

    return;

end");

            //[Archive].[spGetAvailabilityFee]
            this.repositorySet.ExecuteSql(@"
ALTER procedure [Archive].[spGetAvailabilityFee]
AS
begin
    select    c.Name as Country
            , c.Region
            , c.ClusterRegion

            , wg.Name as Wg
            , wg.Description as WgDescription
            , wg.Pla
            , wg.Sog

            , fee3.AverageContractDuration_Approved          as AverageContractDuration
            , fee2.InstalledBaseHighAvailability_Approved    as InstalledBaseHighAvailability
            , fee3.StockValueFj_Approved                     as StockValueFj
            , fee3.StockValueMv_Approved                     as StockValueMv
            , fee3.TotalLogisticsInfrastructureCost_Approved as TotalLogisticsInfrastructureCost 
            , fee.MaxQty_Approved                           as MaxQty
            , fee2.JapanBuy_Approved                         as JapanBuy
            , fee.CostPerKit_Approved                       as CostPerKit
            , fee.CostPerKitJapanBuy_Approved               as CostPerKitJapanBuy

    from Hardware.AvailabilityFeeWg fee

    join Archive.GetWg(0) wg on wg.id = fee.Wg

    join Hardware.AvailabilityFeeWgCountry fee2 on fee2.Wg = wg.Id

	join Hardware.AvailabilityFeeCountryCompany fee3 on fee2.Country = fee3.Country and wg.CompanyId = fee3.Company

    join Archive.GetCountries() c on c.id = fee2.Country

    order by c.Name, wg.Name
end
");
        }
    }
}
