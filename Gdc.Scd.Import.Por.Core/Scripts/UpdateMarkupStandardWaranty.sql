IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp;
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

select msw.* into #tmp
from Hardware.MarkupStandardWaranty msw
where msw.Deactivated = 0
        and not exists(select * from @wg where Id = msw.Wg)

create index ix_tmp_Country_SLA on #tmp(Country, Pla);

select    t.Country
    , t.Pla

    , min(MarkupFactorStandardWarranty) as MarkupFactorStandardWarranty
    , min(MarkupFactorStandardWarranty_Approved) as MarkupFactorStandardWarranty_Approved
    , min(MarkupStandardWarranty) as MarkupStandardWarranty
    , min(MarkupStandardWarranty_Approved) as MarkupStandardWarranty_Approved

into #tmpMin
from #tmp t
group by t.Country, t.Pla;

create index ix_tmpmin_Country_SLA on #tmp(Country, Pla);

update msw set
    MarkupFactorStandardWarranty = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and (MarkupFactorStandardWarranty is null or MarkupFactorStandardWarranty <> t.MarkupFactorStandardWarranty)) then null else t.MarkupFactorStandardWarranty end
, MarkupFactorStandardWarranty_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and (MarkupFactorStandardWarranty_Approved is null or MarkupFactorStandardWarranty_Approved <> t.MarkupFactorStandardWarranty_Approved)) then null else t.MarkupFactorStandardWarranty_Approved end
, MarkupStandardWarranty = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and (MarkupStandardWarranty is null or MarkupStandardWarranty <> t.MarkupStandardWarranty)) then null else t.MarkupStandardWarranty end
, MarkupStandardWarranty_Approved = case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and (MarkupStandardWarranty_Approved is null or MarkupStandardWarranty_Approved <> t.MarkupStandardWarranty_Approved)) then null else t.MarkupStandardWarranty_Approved end

from Hardware.MarkupStandardWaranty msw
inner join #tmpMin t on t.Country = msw.Country and t.Pla = msw.Pla 
where msw.Deactivated = 0 and exists(select * from @wg where Id = msw.Wg);

IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;
