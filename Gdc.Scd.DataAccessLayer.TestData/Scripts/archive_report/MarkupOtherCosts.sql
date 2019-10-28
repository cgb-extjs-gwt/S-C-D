if OBJECT_ID('Archive.spGetMarkupOtherCosts') is not null
    drop procedure Archive.spGetMarkupOtherCosts;
go

create procedure [Archive].[spGetMarkupOtherCosts]
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

          , tta.Availability
          , tta.ReactionTime
          , tta.ReactionType

          , moc.MarkupFactor_Approved  as MarkupFactor
          , moc.Markup_Approved        as Markup

          , moc.ProlongationMarkupFactor_Approved as ProlongationMarkupFactor
          , moc.ProlongationMarkup_Approved as ProlongationMarkup

    from Hardware.MarkupOtherCosts moc
    join Archive.GetCountries() c on c.id = moc.Country
    join Archive.GetWg(null) wg on wg.id = moc.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = moc.CentralContractGroup
    join Archive.GetReactionTimeTypeAvailability() tta on tta.Id = moc.ReactionTimeTypeAvailability

    where moc.Deactivated = 0
    order by c.Name, wg.Name
end
go

