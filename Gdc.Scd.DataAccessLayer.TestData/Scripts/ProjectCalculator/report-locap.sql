USE [SCD_2]

IF OBJECT_ID('[ProjectCalculator].[spLocapReport]') IS NOT NULL
  DROP PROCEDURE [ProjectCalculator].[spLocapReport]
GO

CREATE PROCEDURE [ProjectCalculator].[spLocapReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	EXEC [Report].[spLocap] @cnt, @wg, NULL, NULL, NULL, @reactiontype, @loc, @lastid, @limit, @projectId 
END
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Locap'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 1, @reportName, 'ProjectCalculator.spLocapReport', 'LOCAP reports (for a specific country)')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

	declare @index int = 0;

	delete from Report.ReportColumn where ReportId = @reportId;
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLevel', 'Service Level', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarranty', 'Standard Warranty Duration', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarrantyLocation', 'Standard Warranty Service Location', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LocalServiceStandardWarranty', 'Standard Warranty costs', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTC', 'Service TC', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP_Released', 'Service TP (Released)', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('datetime'), 'ReleaseDate', 'Release date', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceType', 'Service type', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PlausiCheck', 'Plausi Check', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PortfolioType', 'Portfolio Type', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);


	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Duration', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service location', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'Pro Active SLA', 1, 1);
	set @index = @index + 1;
	insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PLA', 'PLA', 1, 1);


	set @index = 0;
	delete from Report.ReportFilter where ReportId = @reportId;
	set @index = @index + 1;
	insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
	set @index = @index + 1;
	insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgsog', 1), 'wg', 'Warranty Group');
	set @index = @index + 1;
	insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
	set @index = @index + 1;
	insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
END