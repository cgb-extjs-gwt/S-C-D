
delete from SoftwareSolution.ServiceCostCalculation;

insert into SoftwareSolution.ServiceCostCalculation(
    SogId,
    YearId,
    AvailabilityId
) (
    select   sog.Id
           , y.Id
           , av.Id
    from   InputAtoms.Sog sog
         , Dependencies.Year y
         , Dependencies.Availability av
);

go

