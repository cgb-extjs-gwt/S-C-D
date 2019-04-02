if OBJECT_ID('Archive.spGetReinsurance') is not null
    drop procedure Archive.spGetReinsurance;
go

create procedure Archive.spGetReinsurance
AS
begin
    select  wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , dur.Name as Duration
          , rta.Availability
          , rta.ReactionTime

          , cur.Name as Currency
          , er.Value as ExchangeRate

          , r.ReinsuranceFlatfee_Approved      as ReinsuranceFlatfee
          , r.ReinsuranceUpliftFactor_Approved as ReinsuranceUpliftFactor

    from Hardware.Reinsurance r
    join Archive.GetWg(null) wg on wg.id = r.Wg
    join Dependencies.Duration dur on dur.Id = r.Duration
    join Archive.GetReactionTimeAvailability() rta on rta.Id = r.ReactionTimeAvailability

    left join [References].Currency cur on cur.Id = r.CurrencyReinsurance_Approved
    left join [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance_Approved

    where r.DeactivatedDateTime is null
    order by wg.Name, dur.Name
end
go
