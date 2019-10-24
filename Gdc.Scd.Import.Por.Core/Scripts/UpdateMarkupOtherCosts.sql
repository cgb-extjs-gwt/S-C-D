IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp;
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

select moc.* into #tmp
from Hardware.MarkupOtherCosts moc
where moc.Deactivated = 0
        and not exists(select * from @wg where Id = moc.Wg)

create index ix_tmp_Country_SLA on #tmp(Country, Pla, ReactionTimeTypeAvailability);

select    t.Country
        , t.Pla
        , t.ReactionTimeTypeAvailability

        , min(Markup) as Markup
        , min(Markup_Approved) as Markup_Approved
        , min(MarkupFactor) as MarkupFactor
        , min(MarkupFactor_Approved) as MarkupFactor_Approved
        , min(ProlongationMarkup) as ProlongationMarkup
        , min(ProlongationMarkup_Approved) as ProlongationMarkup_Approved
        , min(ProlongationMarkupFactor) as ProlongationMarkupFactor
        , min(ProlongationMarkupFactor_Approved) as ProlongationMarkupFactor_Approved

into #tmpMin
from #tmp t
group by t.Country, t.Pla, t.ReactionTimeTypeAvailability;

create index ix_tmpmin_Country_SLA on #tmp(Country, Pla, ReactionTimeTypeAvailability);

update moc
    set Markup =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (Markup is null or Markup <> t.Markup)) then null else t.Markup end
    , Markup_Approved =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (Markup_Approved is null or Markup_Approved <> t.Markup_Approved)) then null else t.Markup_Approved end
    , MarkupFactor =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (MarkupFactor is null or MarkupFactor <> t.MarkupFactor)) then null else t.MarkupFactor end
    , MarkupFactor_Approved =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (MarkupFactor_Approved is null or MarkupFactor_Approved <> t.MarkupFactor_Approved)) then null else t.MarkupFactor_Approved end
    , ProlongationMarkup =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (ProlongationMarkup is null or ProlongationMarkup <> t.ProlongationMarkup)) then null else t.ProlongationMarkup end
    , ProlongationMarkup_Approved =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (ProlongationMarkup_Approved is null or ProlongationMarkup_Approved <> t.ProlongationMarkup_Approved)) then null else t.ProlongationMarkup_Approved end
    , ProlongationMarkupFactor =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (ProlongationMarkupFactor is null or ProlongationMarkupFactor <> t.ProlongationMarkupFactor)) then null else t.ProlongationMarkupFactor end
    , ProlongationMarkupFactor_Approved =  case when exists(select * from #tmp where Country = t.Country and Pla = t.Pla and ReactionTimeTypeAvailability = t.ReactionTimeTypeAvailability and (ProlongationMarkupFactor_Approved is null or ProlongationMarkupFactor_Approved <> t.ProlongationMarkupFactor_Approved)) then null else t.ProlongationMarkupFactor_Approved end

from Hardware.MarkupOtherCosts moc
inner join #tmpMin t on t.Country = moc.Country and t.Pla = moc.Pla and t.ReactionTimeTypeAvailability = moc.ReactionTimeTypeAvailability
where moc.Deactivated = 0 and exists(select * from @wg where Id = moc.Wg);

IF OBJECT_ID('tempdb..#tmp') IS NOT NULL DROP TABLE #tmp
IF OBJECT_ID('tempdb..#tmpMin') IS NOT NULL DROP TABLE #tmpMin;

