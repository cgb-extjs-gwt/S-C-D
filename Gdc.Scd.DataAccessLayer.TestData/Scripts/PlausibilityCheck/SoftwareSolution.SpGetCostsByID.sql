if OBJECT_ID('SoftwareSolution.SpGetCostsByID') is not null
    drop procedure [SoftwareSolution].SpGetCostsByID;
go

create procedure [SoftwareSolution].SpGetCostsByID(
    @approved       bit , 
    @id             bigint
)
as
begin


    --=== sla ==========================================================
    declare @digit  bigint;
    declare @avID  bigint;
    declare @yearID bigint;

    select  @digit = SwDigit
          , @yearID = da.YearId
          , @avID = da.AvailabilityId
    from SoftwareSolution.SwSpMaintenance m
    join Dependencies.Duration_Availability da on da.Id = m.DurationAvailability
    where m.id = @id;

    declare @diglist dbo.ListID; insert into @diglist(id) values(@digit);
    declare @avlist dbo.ListID; insert into @avlist(id) values(@avID);
    declare @yearlist dbo.ListID; insert into @yearlist(id) values(@yearID);

    select  top(1)
            m.rownum
          , m.Id
          , m.Fsp
          , d.Name as SwDigit
          , sog.Name as Sog
          , av.Name as Availability 
          , dr.Name as Duration
          , m.[1stLevelSupportCosts]
          , m.[2ndLevelSupportCosts]
          , m.InstalledBaseCountry
          , m.InstalledBaseSog
          , m.TotalInstalledBaseSog
          , m.Reinsurance
          , m.ServiceSupport
          , m.TransferPrice
          , m.MaintenanceListPrice
          , m.DealerPrice
          , m.DiscountDealerPrice
    from SoftwareSolution.GetCosts(@approved, @diglist, @avlist, @yearlist, null, null) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Duration dr on dr.Id = m.Year
    where m.Id = @id;

end
go

exec SoftwareSolution.SpGetCostsByID 0, 657;