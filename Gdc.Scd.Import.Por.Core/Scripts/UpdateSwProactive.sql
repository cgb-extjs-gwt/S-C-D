IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp;
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

select pro.* into #tmp
from SoftwareSolution.ProActiveSw pro
where pro.Deactivated = 0
        and not exists(select * from @dig where Id = pro.SwDigit)

create index ix_tmp_SLA on #tmp(Country, Sog);

select    Country
        , Sog

        , min(LocalRemoteAccessSetupPreparationEffort) as LocalRemoteAccessSetupPreparationEffort
        , min(LocalRegularUpdateReadyEffort) as LocalRegularUpdateReadyEffort
        , min(LocalPreparationShcEffort) as LocalPreparationShcEffort
        , min(CentralExecutionShcReportCost) as CentralExecutionShcReportCost
        , min(LocalRemoteShcCustomerBriefingEffort) as LocalRemoteShcCustomerBriefingEffort
        , min(LocalOnSiteShcCustomerBriefingEffort) as LocalOnSiteShcCustomerBriefingEffort
        , min(TravellingTime) as TravellingTime
        , min(OnSiteHourlyRate) as OnSiteHourlyRate
        , min(LocalRemoteAccessSetupPreparationEffort_Approved) as LocalRemoteAccessSetupPreparationEffort_Approved
        , min(LocalRegularUpdateReadyEffort_Approved) as LocalRegularUpdateReadyEffort_Approved
        , min(LocalPreparationShcEffort_Approved) as LocalPreparationShcEffort_Approved
        , min(CentralExecutionShcReportCost_Approved) as CentralExecutionShcReportCost_Approved
        , min(LocalRemoteShcCustomerBriefingEffort_Approved) as LocalRemoteShcCustomerBriefingEffort_Approved
        , min(LocalOnSiteShcCustomerBriefingEffort_Approved) as LocalOnSiteShcCustomerBriefingEffort_Approved
        , min(TravellingTime_Approved) as TravellingTime_Approved
        , min(OnSiteHourlyRate_Approved) as OnSiteHourlyRate_Approved

into #tmpMin
from #tmp 
group by Country, Sog;

create index ix_tmpmin_SLA on #tmp(Country, Sog);

update pro set
        LocalRemoteAccessSetupPreparationEffort = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalRemoteAccessSetupPreparationEffort is null or LocalRemoteAccessSetupPreparationEffort <> t.LocalRemoteAccessSetupPreparationEffort)) then null else t.LocalRemoteAccessSetupPreparationEffort end
    , LocalRegularUpdateReadyEffort = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalRegularUpdateReadyEffort is null or LocalRegularUpdateReadyEffort <> t.LocalRegularUpdateReadyEffort)) then null else t.LocalRegularUpdateReadyEffort end
    , LocalPreparationShcEffort = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalPreparationShcEffort is null or LocalPreparationShcEffort <> t.LocalPreparationShcEffort)) then null else t.LocalPreparationShcEffort end
    , CentralExecutionShcReportCost = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (CentralExecutionShcReportCost is null or CentralExecutionShcReportCost <> t.CentralExecutionShcReportCost)) then null else t.CentralExecutionShcReportCost end
    , LocalRemoteShcCustomerBriefingEffort = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalRemoteShcCustomerBriefingEffort is null or LocalRemoteShcCustomerBriefingEffort <> t.LocalRemoteShcCustomerBriefingEffort)) then null else t.LocalRemoteShcCustomerBriefingEffort end
    , LocalOnSiteShcCustomerBriefingEffort = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalOnSiteShcCustomerBriefingEffort is null or LocalOnSiteShcCustomerBriefingEffort <> t.LocalOnSiteShcCustomerBriefingEffort)) then null else t.LocalOnSiteShcCustomerBriefingEffort end
    , TravellingTime = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (TravellingTime is null or TravellingTime <> t.TravellingTime)) then null else t.TravellingTime end
    , OnSiteHourlyRate = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (OnSiteHourlyRate is null or OnSiteHourlyRate <> t.OnSiteHourlyRate)) then null else t.OnSiteHourlyRate end
    , LocalRemoteAccessSetupPreparationEffort_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalRemoteAccessSetupPreparationEffort_Approved is null or LocalRemoteAccessSetupPreparationEffort_Approved <> t.LocalRemoteAccessSetupPreparationEffort_Approved)) then null else t.LocalRemoteAccessSetupPreparationEffort_Approved end
    , LocalRegularUpdateReadyEffort_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalRegularUpdateReadyEffort_Approved is null or LocalRegularUpdateReadyEffort_Approved <> t.LocalRegularUpdateReadyEffort_Approved)) then null else t.LocalRegularUpdateReadyEffort_Approved end
    , LocalPreparationShcEffort_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalPreparationShcEffort_Approved is null or LocalPreparationShcEffort_Approved <> t.LocalPreparationShcEffort_Approved)) then null else t.LocalPreparationShcEffort_Approved end
    , CentralExecutionShcReportCost_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (CentralExecutionShcReportCost_Approved is null or CentralExecutionShcReportCost_Approved <> t.CentralExecutionShcReportCost_Approved)) then null else t.CentralExecutionShcReportCost_Approved end
    , LocalRemoteShcCustomerBriefingEffort_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalRemoteShcCustomerBriefingEffort_Approved is null or LocalRemoteShcCustomerBriefingEffort_Approved <> t.LocalRemoteShcCustomerBriefingEffort_Approved)) then null else t.LocalRemoteShcCustomerBriefingEffort_Approved end
    , LocalOnSiteShcCustomerBriefingEffort_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (LocalOnSiteShcCustomerBriefingEffort_Approved is null or LocalOnSiteShcCustomerBriefingEffort_Approved <> t.LocalOnSiteShcCustomerBriefingEffort_Approved)) then null else t.LocalOnSiteShcCustomerBriefingEffort_Approved end
    , TravellingTime_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (TravellingTime_Approved is null or TravellingTime_Approved <> t.TravellingTime_Approved)) then null else t.TravellingTime_Approved end
    , OnSiteHourlyRate_Approved = case when exists(select * from #tmp where Country = t.Country and Sog = t.Sog and (OnSiteHourlyRate_Approved is null or OnSiteHourlyRate_Approved <> t.OnSiteHourlyRate_Approved)) then null else t.OnSiteHourlyRate_Approved end

from SoftwareSolution.ProActiveSw pro
inner join #tmpMin t on t.Country = pro.Country and t.Sog = pro.Sog
where pro.Deactivated = 0 and exists(select * from @dig where Id = pro.SwDigit);

IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

