if OBJECT_ID('Archive.spGetFieldServiceCost') is not null
    drop procedure Archive.spGetFieldServiceCost;
go

create procedure Archive.spGetFieldServiceCost
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ccg.Name                             as ContractGroup
          , ccg.Code                             as ContractGroupCode

          , loc.Name as ServiceLocation

          , rtt.ReactionTime
          , rtt.ReactionType

          , fsc.RepairTime_Approved           as RepairTime
          , fsc.TravelTime_Approved           as TravelTime
          , fsc.LabourCost_Approved           as LabourCost
          , fsc.TravelCost_Approved           as TravelCost
          , fsc.PerformanceRate_Approved      as PerformanceRate
          , fsc.TimeAndMaterialShare_Approved as TimeAndMaterialShare

    from Hardware.FieldServiceCost fsc
    join Archive.GetReactionTimeType() rtt on rtt.Id = fsc.ReactionTimeType
    join Archive.GetCountries() c on c.Id = fsc.Country
    join Archive.GetWg(null) wg on wg.id = fsc.Wg
    join Dependencies.ServiceLocation loc on loc.Id = fsc.ServiceLocation
    join InputAtoms.CentralContractGroup ccg on ccg.Id = fsc.CentralContractGroup

    where fsc.DeactivatedDateTime is null

    order by c.Name, wg.Name
end
go