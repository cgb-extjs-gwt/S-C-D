IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp;
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

select fsc.* into #tmp
from Hardware.FieldServiceCost fsc
where fsc.Deactivated = 0
      and not exists(select * from @wg where Id = fsc.Wg);

create index ix_tmp_Country_SLA on #tmp(Country, Pla, ServiceLocation, ReactionTimeType);

select    Country
        , Pla
        , ServiceLocation
        , ReactionTimeType

        , min(RepairTime) as RepairTime
        , min(TravelTime) as TravelTime
        , min(LabourCost) as LabourCost
        , min(TravelCost) as TravelCost
        , min(PerformanceRate) as PerformanceRate
        , min(TimeAndMaterialShare) as TimeAndMaterialShare
        , min(RepairTime_Approved) as RepairTime_Approved
        , min(TravelTime_Approved) as TravelTime_Approved
        , min(LabourCost_Approved) as LabourCost_Approved
        , min(TravelCost_Approved) as TravelCost_Approved
        , min(PerformanceRate_Approved) as PerformanceRate_Approved
        , min(TimeAndMaterialShare_Approved) as TimeAndMaterialShare_Approved

into #tmpMin
from #tmp 
group by Country, Pla, ServiceLocation, ReactionTimeType;

create index ix_tmpmin_Country_SLA on #tmp(Country, Pla, ServiceLocation, ReactionTimeType);

update fsc set 
        RepairTime = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (RepairTime is null or RepairTime <> t.RepairTime)) then null else t.RepairTime end
      , TravelTime = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (TravelTime is null or TravelTime <> t.TravelTime)) then null else t.TravelTime end
      , LabourCost = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (LabourCost is null or LabourCost <> t.LabourCost)) then null else t.LabourCost end
      , TravelCost = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (TravelCost is null or TravelCost <> t.TravelCost)) then null else t.TravelCost end
      , PerformanceRate = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (PerformanceRate is null or PerformanceRate <> t.PerformanceRate)) then null else t.PerformanceRate end
      , TimeAndMaterialShare = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (TimeAndMaterialShare is null or TimeAndMaterialShare <> t.TimeAndMaterialShare)) then null else t.TimeAndMaterialShare end
      , RepairTime_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (RepairTime_Approved is null or RepairTime_Approved <> t.RepairTime_Approved)) then null else t.RepairTime_Approved end
      , TravelTime_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (TravelTime_Approved is null or TravelTime_Approved <> t.TravelTime_Approved)) then null else t.TravelTime_Approved end
      , LabourCost_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (LabourCost_Approved is null or LabourCost_Approved <> t.LabourCost_Approved)) then null else t.LabourCost_Approved end
      , TravelCost_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (TravelCost_Approved is null or TravelCost_Approved <> t.TravelCost_Approved)) then null else t.TravelCost_Approved end
      , PerformanceRate_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (PerformanceRate_Approved is null or PerformanceRate_Approved <> t.PerformanceRate_Approved)) then null else t.PerformanceRate_Approved end
      , TimeAndMaterialShare_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ServiceLocation = t.ServiceLocation and ReactionTimeType = t.ReactionTimeType  and (TimeAndMaterialShare_Approved is null or TimeAndMaterialShare_Approved <> t.TimeAndMaterialShare_Approved)) then null else t.TimeAndMaterialShare_Approved end

from Hardware.FieldServiceCost fsc
inner join #tmpMin t on t.Country = fsc.Country and t.Pla = fsc.Pla and t.ServiceLocation = fsc.ServiceLocation and t.ReactionTimeType = fsc.ReactionTimeType
where fsc.Deactivated = 0 and exists(select * from @wg where Id = fsc.Wg);

IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp;
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

