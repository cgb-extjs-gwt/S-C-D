IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp;
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

select lc.* into #tmp
from Hardware.LogisticsCosts lc
where lc.Deactivated = 0 and not exists(select * from @wg where Id = lc.Wg);

create index ix_tmp_Country_SLA on #tmp(Country, Pla, ReactionTimeType);

select    t.Country
        , t.Pla
        , t.ReactionTimeType

        , min(StandardHandling) as StandardHandling
        , min(HighAvailabilityHandling) as HighAvailabilityHandling
        , min(StandardDelivery) as StandardDelivery
        , min(ExpressDelivery) as ExpressDelivery
        , min(TaxiCourierDelivery) as TaxiCourierDelivery
        , min(ReturnDeliveryFactory) as ReturnDeliveryFactory
        , min(StandardHandling_Approved) as StandardHandling_Approved
        , min(HighAvailabilityHandling_Approved) as HighAvailabilityHandling_Approved
        , min(StandardDelivery_Approved) as StandardDelivery_Approved
        , min(ExpressDelivery_Approved) as ExpressDelivery_Approved
        , min(TaxiCourierDelivery_Approved) as TaxiCourierDelivery_Approved
        , min(ReturnDeliveryFactory_Approved) as ReturnDeliveryFactory_Approved

into #tmpMin
from #tmp t
group by t.Country, t.Pla, t.ReactionTimeType

create index ix_tmpmin_Country_SLA on #tmp(Country, Pla, ReactionTimeType);

update lc
    set StandardHandling = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (StandardHandling is null or StandardHandling <> t.StandardHandling)) then null else t.StandardHandling end
    , HighAvailabilityHandling = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (HighAvailabilityHandling is null or HighAvailabilityHandling <> t.HighAvailabilityHandling)) then null else t.HighAvailabilityHandling end
    , StandardDelivery = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (StandardDelivery is null or StandardDelivery <> t.StandardDelivery)) then null else t.StandardDelivery end
    , ExpressDelivery = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (ExpressDelivery is null or ExpressDelivery <> t.ExpressDelivery)) then null else t.ExpressDelivery end
    , TaxiCourierDelivery = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (TaxiCourierDelivery is null or TaxiCourierDelivery <> t.TaxiCourierDelivery)) then null else t.TaxiCourierDelivery end
    , ReturnDeliveryFactory = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (ReturnDeliveryFactory is null or ReturnDeliveryFactory <> t.ReturnDeliveryFactory)) then null else t.ReturnDeliveryFactory end
    , StandardHandling_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (StandardHandling_Approved is null or StandardHandling_Approved <> t.StandardHandling_Approved)) then null else t.StandardHandling_Approved end
    , HighAvailabilityHandling_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (HighAvailabilityHandling_Approved is null or HighAvailabilityHandling_Approved <> t.HighAvailabilityHandling_Approved)) then null else t.HighAvailabilityHandling_Approved end
    , StandardDelivery_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (StandardDelivery_Approved is null or StandardDelivery_Approved <> t.StandardDelivery_Approved)) then null else t.StandardDelivery_Approved end
    , ExpressDelivery_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (ExpressDelivery_Approved is null or ExpressDelivery_Approved <> t.ExpressDelivery_Approved)) then null else t.ExpressDelivery_Approved end
    , TaxiCourierDelivery_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (TaxiCourierDelivery_Approved is null or TaxiCourierDelivery_Approved <> t.TaxiCourierDelivery_Approved)) then null else t.TaxiCourierDelivery_Approved end
    , ReturnDeliveryFactory_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeType = t.ReactionTimeType and (ReturnDeliveryFactory_Approved is null or ReturnDeliveryFactory_Approved <> t.ReturnDeliveryFactory_Approved)) then null else t.ReturnDeliveryFactory_Approved end

from Hardware.LogisticsCosts lc
inner join #tmpMin t on t.Country = lc.Country and t.Pla = lc.Pla and t.ReactionTimeType = lc.ReactionTimeType
where lc.Deactivated = 0 and exists(select * from @wg where Id = lc.Wg);

IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;
