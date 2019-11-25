if not exists(select id from Dependencies.ServiceLocation where name = 'Backlevel Service')
    insert into Dependencies.ServiceLocation(Name, ExternalName, [Order]) values ('Backlevel Service', 'Backlevel Service', 11);
go

declare @loc bigint = (select id from Dependencies.ServiceLocation where name = 'Backlevel Service');

if not exists(select * from Hardware.FieldServiceCost where ServiceLocation = @loc)
begin

    insert into Hardware.FieldServiceCost(Country, Pla, Wg, ReactionTimeType, ServiceLocation, CentralContractGroup)
    select fsc.Country, fsc.Pla, fsc.Wg, fsc.ReactionTimeType, @loc, fsc.CentralContractGroup
    from Hardware.FieldServiceCost fsc
    where exists(select * from InputAtoms.Wg where DeactivatedDateTime is null and Id = fsc.Wg)
    group by fsc.Country, fsc.Pla, fsc.Wg, fsc.ReactionTimeType, fsc.CentralContractGroup

end

