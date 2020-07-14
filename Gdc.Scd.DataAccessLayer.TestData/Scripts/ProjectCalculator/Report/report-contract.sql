USE [SCD_2]

IF OBJECT_ID('[ProjectCalculator].[spContractReport]') is not null
    drop procedure [ProjectCalculator].[spContractReport]
GO

CREATE PROCEDURE [ProjectCalculator].[spContractReport]
(
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @reactiontype BIGINT,
    @loc          BIGINT,
    @pro          BIGINT,
    @lastid       BIGINT,
    @limit        INT,
	@projectId    BIGINT
)
AS
BEGIN
	declare @avTable dbo.ListId

    declare @durTable dbo.ListId; insert into @durTable(id) select id from Dependencies.Duration where IsProlongation = 0 and Value = 5;

    declare @rtimeTable dbo.ListId

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; if @pro is not null insert into @proTable(id) values(@pro);	

	WITH [Costs] AS
	(
		SELECT
			t.[Id],
			t.[Country],
			t.[Wg],
			Wg.[Description] AS WgDescription,
			t.[ServiceLocation],
			t.[ReactionTime],
			t.[ReactionType],
			t.[Availability],

			t.[ServiceTP],
			t.[StdMonths],
			t.[StdIsProlongation],
			t.[StdMonths] % 12 AS RemainderMonths,

			t.[Currency],
			wg.[ServiceTypes] AS PortfolioType,
			wg.[Sog] AS Sog
		FROM 
			[Hardware].[GetCostsYear](
				1, 
				@cnt, 
				@wg, 
				@avTable, 
				@durTable, 
				@rtimeTable, 
				@rtypeTable, 
				@locTable, 
				@proTable,
				@lastid,
				@limit,
				@projectId) AS t
		INNER JOIN
			[InputAtoms].[WgSogView] AS Wg on Wg.id = t.WgId
	),
	[CostMonths] AS 
	(
		SELECT
			[Costs].*,
			(
				CASE
					WHEN [Costs].[RemainderMonths] = 0 
					THEN [Costs].[ServiceTP] / 12
					ELSE [Costs].[ServiceTP] / [Costs].[RemainderMonths]
				END
			) 
			AS ServiceTPMonthly
		FROM
			[Costs]
	)
	SELECT
		MIN([Id]) AS Id,
		[Country],
		[Wg],
		[WgDescription],
		[ServiceLocation],
		[ReactionTime],
		[ReactionType],
		[Availability],

		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [ServiceTP] END) AS ServiceTP1,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [ServiceTP] END) AS ServiceTP2,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [ServiceTP] END) AS ServiceTP3,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [ServiceTP] END) AS ServiceTP4,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [ServiceTP] END) AS ServiceTP5,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [ServiceTP] END) AS ServiceTP6,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [ServiceTP] END) AS ServiceTP7,

		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly1,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly2,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly3,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly4,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly5,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly6,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly7,

		[Currency],
		[PortfolioType],
		[Sog]
	FROM
		[CostMonths]
	GROUP BY
		[Country],
		[Wg],
		[WgDescription],
		[ServiceLocation],
		[ReactionTime],
		[ReactionType],
		[Availability],

		[Currency],
		[PortfolioType],
		[Sog]
END
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Contract'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 1, @reportName, 'ProjectCalculator.spContractReport', 'Contract reports')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP1', 'Service Tranfer Price yearly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP2', 'Service Tranfer Price yearly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP3', 'Service Tranfer Price yearly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP4', 'Service Tranfer Price yearly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP5', 'Service Tranfer Price yearly - year5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly1', 'Service Tranfer Price monthly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly2', 'Service Tranfer Price monthly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly3', 'Service Tranfer Price monthly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly4', 'Service Tranfer Price monthly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly5', 'Service Tranfer Price monthly - year5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WarrantyLevel', 'Warranty Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PortfolioType', 'Portfolio Type', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 1), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg', 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');
