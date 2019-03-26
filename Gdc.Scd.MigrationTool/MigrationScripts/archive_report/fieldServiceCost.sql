select  c.Name as Country
      , c.Region
      , c.ClusterRegion

      , wg.Name as Wg
      , wg.Description as WgDescription
      , wg.Pla
      , wg.Sog

      , loc.Name as ServiceLocation

      , rtt.ReactionTime
      , rtt.ReactionType

      , fsc.RepairTime_Approved           as RepairTime
      , fsc.TravelTime_Approved           as TravelTime
      , fsc.LabourCost_Approved           as LabourCost
      , fsc.TravelCost_Approved           as TravelCost
      , fsc.PerformanceRate_Approved      as PerformanceRate
      , fsc.TimeAndMaterialShare_Approved as TimeAndMaterialShare
      , fsc.CentralContractGroup          as CentralContractGroup

from Hardware.FieldServiceCost fsc
join Archive.GetCountries() c on c.Id = fsc.Country
join Archive.GetWg(null) wg on wg.id = fsc.Wg
join Dependencies.ServiceLocation loc on loc.Id = fsc.ServiceLocation

join Archive.GetReactionTimeType() rtt on rtt.Id = fsc.ReactionTimeType

where fsc.DeactivatedDateTime is null

order by c.Name, wg.Name