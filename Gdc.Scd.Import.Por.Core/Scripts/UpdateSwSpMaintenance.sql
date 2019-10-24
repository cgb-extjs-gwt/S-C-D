IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp;
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

select m.* into #tmp
from SoftwareSolution.SwSpMaintenance m
where m.Deactivated = 0
        and not exists(select * from @dig where Id = m.SwDigit)

create index ix_tmp_SLA on #tmp(Sog, DurationAvailability);

select    Sog
        , DurationAvailability

        , min([2ndLevelSupportCosts]) as [2ndLevelSupportCosts]
        , min([2ndLevelSupportCosts_Approved]) as [2ndLevelSupportCosts_Approved]
        , min(ReinsuranceFlatfee) as ReinsuranceFlatfee
        , min(ReinsuranceFlatfee_Approved) as ReinsuranceFlatfee_Approved
        , min(CurrencyReinsurance) as CurrencyReinsurance
        , min(CurrencyReinsurance_Approved) as CurrencyReinsurance_Approved
        , min(RecommendedSwSpMaintenanceListPrice) as RecommendedSwSpMaintenanceListPrice
        , min(RecommendedSwSpMaintenanceListPrice_Approved) as RecommendedSwSpMaintenanceListPrice_Approved
        , min(MarkupForProductMarginSwLicenseListPrice) as MarkupForProductMarginSwLicenseListPrice
        , min(MarkupForProductMarginSwLicenseListPrice_Approved) as MarkupForProductMarginSwLicenseListPrice_Approved
        , min(ShareSwSpMaintenanceListPrice) as ShareSwSpMaintenanceListPrice
        , min(ShareSwSpMaintenanceListPrice_Approved) as ShareSwSpMaintenanceListPrice_Approved
        , min(DiscountDealerPrice) as DiscountDealerPrice
        , min(DiscountDealerPrice_Approved) as DiscountDealerPrice_Approved

into #tmpMin
from #tmp 
group by Sog, DurationAvailability;

create index ix_tmpmin_SLA on #tmp(Sog, DurationAvailability);

update m set
        [2ndLevelSupportCosts] = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and ([2ndLevelSupportCosts] is null or [2ndLevelSupportCosts] <> t.[2ndLevelSupportCosts])) then null else t.[2ndLevelSupportCosts] end
      , [2ndLevelSupportCosts_Approved] = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and ([2ndLevelSupportCosts_Approved] is null or [2ndLevelSupportCosts_Approved] <> t.[2ndLevelSupportCosts_Approved])) then null else t.[2ndLevelSupportCosts_Approved] end
      , ReinsuranceFlatfee = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (ReinsuranceFlatfee is null or ReinsuranceFlatfee <> t.ReinsuranceFlatfee)) then null else t.ReinsuranceFlatfee end
      , ReinsuranceFlatfee_Approved = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (ReinsuranceFlatfee_Approved is null or ReinsuranceFlatfee_Approved <> t.ReinsuranceFlatfee_Approved)) then null else t.ReinsuranceFlatfee_Approved end
      , CurrencyReinsurance = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (CurrencyReinsurance is null or CurrencyReinsurance <> t.CurrencyReinsurance)) then null else t.CurrencyReinsurance end
      , CurrencyReinsurance_Approved = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (CurrencyReinsurance_Approved is null or CurrencyReinsurance_Approved <> t.CurrencyReinsurance_Approved)) then null else t.CurrencyReinsurance_Approved end
      , RecommendedSwSpMaintenanceListPrice = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (RecommendedSwSpMaintenanceListPrice is null or RecommendedSwSpMaintenanceListPrice <> t.RecommendedSwSpMaintenanceListPrice)) then null else t.RecommendedSwSpMaintenanceListPrice end
      , RecommendedSwSpMaintenanceListPrice_Approved = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (RecommendedSwSpMaintenanceListPrice_Approved is null or RecommendedSwSpMaintenanceListPrice_Approved <> t.RecommendedSwSpMaintenanceListPrice_Approved)) then null else t.RecommendedSwSpMaintenanceListPrice_Approved end
      , MarkupForProductMarginSwLicenseListPrice = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (MarkupForProductMarginSwLicenseListPrice is null or MarkupForProductMarginSwLicenseListPrice <> t.MarkupForProductMarginSwLicenseListPrice)) then null else t.MarkupForProductMarginSwLicenseListPrice end
      , MarkupForProductMarginSwLicenseListPrice_Approved = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (MarkupForProductMarginSwLicenseListPrice_Approved is null or MarkupForProductMarginSwLicenseListPrice_Approved <> t.MarkupForProductMarginSwLicenseListPrice_Approved)) then null else t.MarkupForProductMarginSwLicenseListPrice_Approved end
      , ShareSwSpMaintenanceListPrice = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (ShareSwSpMaintenanceListPrice is null or ShareSwSpMaintenanceListPrice <> t.ShareSwSpMaintenanceListPrice)) then null else t.ShareSwSpMaintenanceListPrice end
      , ShareSwSpMaintenanceListPrice_Approved = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (ShareSwSpMaintenanceListPrice_Approved is null or ShareSwSpMaintenanceListPrice_Approved <> t.ShareSwSpMaintenanceListPrice_Approved)) then null else t.ShareSwSpMaintenanceListPrice_Approved end
      , DiscountDealerPrice = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (DiscountDealerPrice is null or DiscountDealerPrice <> t.DiscountDealerPrice)) then null else t.DiscountDealerPrice end
      , DiscountDealerPrice_Approved = case when exists(select * from #tmp where Sog = t.Sog and DurationAvailability = t.DurationAvailability and (DiscountDealerPrice_Approved is null or DiscountDealerPrice_Approved <> t.DiscountDealerPrice_Approved)) then null else t.DiscountDealerPrice_Approved end

from SoftwareSolution.SwSpMaintenance m
inner join #tmpMin t on t.Sog = m.Sog and t.DurationAvailability = m.DurationAvailability
where m.Deactivated = 0 and exists(select * from @dig where Id = m.SwDigit);

IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

