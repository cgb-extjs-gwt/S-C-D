if OBJECT_ID('Archive.spGetAvailabilityFee') is not null
    drop procedure Archive.spGetAvailabilityFee;
go

CREATE procedure [Archive].[spGetAvailabilityFee]
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

        into #tmp

    from Hardware.AvailabilityFeeWg fee

    join Archive.GetWg(0) wg on wg.id = fee.Wg

    join Hardware.AvailabilityFeeWgCountry fee2 on fee2.Wg = wg.Id and fee2.DeactivatedDateTime is null

    join Hardware.AvailabilityFeeCountryCompany fee3 on fee2.Country = fee3.Country and wg.CompanyId = fee3.Company and fee3.DeactivatedDateTime is null

    join Archive.GetCountries() c on c.id = fee2.Country

    where fee.DeactivatedDateTime is null;

    select * from #tmp
    order by Country, Wg;

    drop table #tmp;

end
