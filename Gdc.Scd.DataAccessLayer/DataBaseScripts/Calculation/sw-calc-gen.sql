IF OBJECT_ID('tempdb..#ShrinkLog') IS NOT NULL DROP PROC #ShrinkLog
GO

CREATE PROC #ShrinkLog
AS
    DBCC SHRINKFILE ('Scd_2_log', 1);
    RETURN 0
GO

delete from SoftwareSolution.ServiceCostCalculation;

exec #ShrinkLog;
go

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

exec #ShrinkLog;
go

IF OBJECT_ID('tempdb..#ShrinkLog') IS NOT NULL DROP PROC #ShrinkLog

GO