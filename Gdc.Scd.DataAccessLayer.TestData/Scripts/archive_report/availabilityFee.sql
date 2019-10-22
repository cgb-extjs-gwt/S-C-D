if OBJECT_ID('Archive.spGetAvailabilityFee') is not null
    drop procedure Archive.spGetAvailabilityFee;
go

create procedure [Archive].[spGetAvailabilityFee]
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , fee.AverageContractDuration_Approved          as AverageContractDuration
          , fee.InstalledBaseHighAvailability_Approved    as InstalledBaseHighAvailability
          , fee.StockValueFj_Approved                     as StockValueFj
          , fee.StockValueMv_Approved                     as StockValueMv
          , fee.TotalLogisticsInfrastructureCost_Approved as TotalLogisticsInfrastructureCost 
          , fee.MaxQty_Approved                           as MaxQty
          , fee.JapanBuy_Approved                         as JapanBuy
          , fee.CostPerKit_Approved                       as CostPerKit
          , fee.CostPerKitJapanBuy_Approved               as CostPerKitJapanBuy

    from Hardware.AvailabilityFee fee
    join Archive.GetCountries() c on c.id = fee.Country
    join Archive.GetWg(0) wg on wg.id = fee.Wg

    where fee.Deactivated = 0

    order by c.Name, wg.Name
end
