if OBJECT_ID('Archive.spGetProlongationMarkup') is not null
    drop procedure Archive.spGetProlongationMarkup;
go

create procedure Archive.spGetProlongationMarkup
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

          , pm.ProlongationMarkupFactor_Approved  as ProlongationMarkupFactor
          , pm.ProlongationMarkup_Approved        as ProlongationMarkup      

    from Hardware.ProlongationMarkup pm
    join Archive.GetCountries() c on c.id = pm.Country
    join Archive.GetWg(null) wg on wg.id = pm.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = pm.CentralContractGroup
    join Archive.GetReactionTimeTypeAvailability() tta on tta.Id = pm.ReactionTimeTypeAvailability

    where pm.DeactivatedDateTime is null
    order by c.Name, wg.Name
end
go