if OBJECT_ID('Archive.spGetFieldServiceCalc') is not null
    drop procedure Archive.spGetFieldServiceCalc;
go

create procedure Archive.spGetFieldServiceCalc
AS
begin
    select    c.Name as Country
            , c.Region
            , c.ClusterRegion
            , c.Currency
            , c.ExchangeRate

            , wg.Name as Wg
            , wg.Description as WgDescription
            , wg.Pla
            , wg.Sog

            , loc.Name as ServiceLocation

            , fsc.RepairTime_Approved           as RepairTime
            , fsc.TravelTime_Approved           as TravelTime
            , fsc.LabourCost_Approved           as LabourCost
            , fsc.TravelCost_Approved           as TravelCost

    from Hardware.FieldServiceCalc fsc
    join Archive.GetCountries() c on c.Id = fsc.Country
    join Archive.GetWg(null) wg on wg.id = fsc.Wg
    join Dependencies.ServiceLocation loc on loc.Id = fsc.ServiceLocation

    order by c.Name, wg.Name
end
go