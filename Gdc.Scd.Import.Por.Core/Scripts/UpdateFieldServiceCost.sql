insert into Hardware.FieldServiceCalc(
              Country
            , Wg
            , ServiceLocation
            , RepairTime
            , RepairTime_Approved
            , TravelTime
            , TravelTime_Approved
            , TravelCost
            , TravelCost_Approved
            , LabourCost
            , LabourCost_Approved
            , OohUpliftFactor
            , OohUpliftFactor_Approved)
select    f.Country
        , f.Wg
        , f.ServiceLocation
            
        , MIN(f.RepairTime)
        , MIN(f.RepairTime_Approved)
        , MIN(f.TravelTime)
        , MIN(f.TravelTime_Approved)
        , MIN(f.TravelCost)
        , MIN(f.TravelCost_Approved)
        , MIN(f.LabourCost)
        , MIN(f.LabourCost_Approved)
        , MIN(i.OohUpliftFactor)
        , MIN(i.OohUpliftFactor_Approved)
from Hardware.FieldServiceCost f
left join Hardware.FieldServiceCalc fsc on fsc.Country = f.Country and fsc.Wg = f.Wg and fsc.ServiceLocation = f.ServiceLocation
where f.Deactivated = 0 and fsc.Country is null
group by f.Country, f.Wg, f.ServiceLocation;

insert into Hardware.FieldServiceTimeCalc(
              Country
            , Wg
            , ReactionTimeType
            , PerformanceRate
            , PerformanceRate_Approved
            , TimeAndMaterialShare
            , TimeAndMaterialShare_Approved)
select    f.Country
        , f.Wg
        , f.ReactionTimeType
        , MIN(f.PerformanceRate)
        , MIN(f.PerformanceRate_Approved)
        , MIN(f.TimeAndMaterialShare)
        , MIN(f.TimeAndMaterialShare_Approved)
from Hardware.FieldServiceCost f
join Dependencies.ReactionTimeType rtt on rtt.id = f.ReactionTimeType and rtt.IsDisabled = 0
left join Hardware.FieldServiceTimeCalc fsc on fsc.Country = f.Country and fsc.Wg = f.Wg and fsc.ReactionTimeType = f.ReactionTimeType
where f.Deactivated = 0 and fsc.Country is null
group by f.Country, f.Wg, f.ReactionTimeType;



