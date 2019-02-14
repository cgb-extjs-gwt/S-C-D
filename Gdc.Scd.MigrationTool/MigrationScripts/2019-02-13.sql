IF OBJECT_ID('Report.SwCalcResult') IS NOT NULL
  DROP FUNCTION Report.SwCalcResult;
go 

CREATE FUNCTION Report.SwCalcResult
(
	@approved bit,
    @digit dbo.ListID readonly,
    @availability dbo.ListID readonly,
    @year dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select    d.Name as SwDigit
            , sog.Name as Sog
            , av.Name as Availability 
            , dr.Name as Duration
            , m.ServiceSupport
            , m.Reinsurance
            , m.TransferPrice
            , m.MaintenanceListPrice
            , m.DealerPrice
    from SoftwareSolution.GetCosts(@approved, @digit, @availability, @year, -1, -1) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Duration dr on dr.Id = m.Year
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SW-CALC-RESULT');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SwDigit', 'SW digit', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'SOG(Asset)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Duration', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceSupport', 'Service support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TransferPrice', 'Transer price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'MaintenanceListPrice', 'Maintenance list price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer reference price', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'swdigit' and MultiSelect=1), 'digit', 'SW digit');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'availability' and MultiSelect=1), 'availability', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'duration' and MultiSelect=1), 'year', 'Duration');

GO

ALTER PROCEDURE [SoftwareSolution].[SpGetCosts]
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

