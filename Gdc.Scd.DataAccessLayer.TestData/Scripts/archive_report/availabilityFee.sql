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

            , fee2.AverageContractDuration_Approved          as AverageContractDuration
            , fee2.InstalledBaseHighAvailability_Approved    as InstalledBaseHighAvailability
            , fee2.StockValueFj_Approved                     as StockValueFj
            , fee2.StockValueMv_Approved                     as StockValueMv
            , fee2.TotalLogisticsInfrastructureCost_Approved as TotalLogisticsInfrastructureCost 
            , fee.MaxQty_Approved                           as MaxQty
            , fee2.JapanBuy_Approved                         as JapanBuy
            , fee.CostPerKit_Approved                       as CostPerKit
            , fee.CostPerKitJapanBuy_Approved               as CostPerKitJapanBuy

    from Hardware.AvailabilityFeeWg fee

    join Archive.GetWg(0) wg on wg.id = fee.Wg

    join Hardware.AvailabilityFeeCountryWg fee2 on fee2.Wg = wg.Id

    join Archive.GetCountries() c on c.id = fee2.Country

    order by c.Name, wg.Name
end
GO