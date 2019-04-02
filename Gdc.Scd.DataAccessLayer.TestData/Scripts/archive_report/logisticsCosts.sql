if OBJECT_ID('Archive.spGetLogisticsCosts') is not null
    drop procedure Archive.spGetLogisticsCosts;
go

create procedure Archive.spGetLogisticsCosts
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

          , rtt.ReactionTime
          , rtt.ReactionType

          , lc.StandardHandling_Approved         as StandardHandling
          , lc.HighAvailabilityHandling_Approved as HighAvailabilityHandling
          , lc.StandardDelivery_Approved         as StandardDelivery
          , lc.ExpressDelivery_Approved          as ExpressDelivery
          , lc.TaxiCourierDelivery_Approved      as TaxiCourierDelivery
          , lc.ReturnDeliveryFactory_Approved    as ReturnDeliveryFactory

    from Hardware.LogisticsCosts lc
    join Archive.GetCountries() c on c.id = lc.Country
    join Archive.GetWg(null) wg on wg.id = lc.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = lc.CentralContractGroup

    join Archive.GetReactionTimeType() rtt on rtt.Id = lc.ReactionTimeType

    where lc.DeactivatedDateTime is null
    order by c.Name, wg.Name
end
go