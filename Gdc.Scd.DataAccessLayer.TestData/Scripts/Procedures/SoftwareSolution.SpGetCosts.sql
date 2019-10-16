IF OBJECT_ID('SoftwareSolution.SpGetCosts') IS NOT NULL
  DROP PROCEDURE SoftwareSolution.SpGetCosts;
go

CREATE PROCEDURE [SoftwareSolution].[SpGetCosts]
    @approved bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

	declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
	declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
	declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

    SELECT @total = COUNT(m.id)

        FROM SoftwareSolution.SwSpMaintenance m 
        JOIN Dependencies.Duration_Availability dav on dav.Id = m.DurationAvailability

		WHERE (@isEmptyDigit = 1 or m.SwDigit in (select id from @digit))
			AND (@isEmptyAV = 1 or dav.AvailabilityId in (select id from @av))
			AND (@isEmptyYear = 1 or dav.YearId in (select id from @year))

    select  m.rownum
          , m.Id
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
    from SoftwareSolution.GetCosts(@approved, @digit, @av, @year, @lastid, @limit) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Duration dr on dr.Id = m.Year

    order by m.SwDigit, m.Availability, m.Year

END
GO