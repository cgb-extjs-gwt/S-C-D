USE [SCD_2]

IF NOT EXISTS(SELECT * FROM sys.schemas WHERE name = 'ProjectCalculator')
	EXEC('CREATE SCHEMA [ProjectCalculator]')
GO

IF OBJECT_ID('[ProjectCalculator].[CalcAvailabilityCoeff]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[CalcAvailabilityCoeff]
GO

IF OBJECT_ID('[ProjectCalculator].[Afr]') IS NOT NULL
	DROP TABLE [ProjectCalculator].[Afr]
GO

IF OBJECT_ID('[ProjectCalculator].[ProjectItem]') IS NOT NULL
	DROP TABLE [ProjectCalculator].[ProjectItem]
GO

IF OBJECT_ID('[ProjectCalculator].[Project]') IS NOT NULL
	DROP TABLE [ProjectCalculator].[Project]
GO

IF OBJECT_ID('[ProjectCalculator].[AvailabilityWeight]') IS NOT NULL
	DROP TABLE [ProjectCalculator].[AvailabilityWeight]
GO

IF OBJECT_ID('[ProjectCalculator].[AvailabilityWeightView]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[AvailabilityWeightView]
GO

CREATE TABLE [ProjectCalculator].[Afr](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AFR] [float] NULL,
	[IsProlongation] [bit] NOT NULL,
	[Months] [int] NOT NULL,
	[ProjectItemId] [bigint] NULL,
 CONSTRAINT [PK_Afr] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [ProjectCalculator].[Project](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreationDate] [datetime2](7) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[UserId] [bigint] NOT NULL,
 CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [ProjectCalculator].[ProjectItem](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CountryId] [bigint] NOT NULL,
	[OnsiteHourlyRates] [float] NULL,
	[ProjectId] [bigint] NULL,
	[ReactionTypeId] [bigint] NOT NULL,
	[ServiceLocationId] [bigint] NOT NULL,
	[WgId] [bigint] NOT NULL,
	[AvailabilityFee_AverageContractDuration] [float] NULL,
	[AvailabilityFee_StockValueFj] [float] NULL,
	[AvailabilityFee_StockValueMv] [float] NULL,
	[AvailabilityFee_TotalLogisticsInfrastructureCost] [float] NULL,
	[Availability_Name] [nvarchar](max) NULL,
	[Availability_Value] [int] NOT NULL,
	[Availability_End_Day] [tinyint] NOT NULL,
	[Availability_End_Hour] [tinyint] NOT NULL,
	[Availability_Start_Day] [tinyint] NOT NULL,
	[Availability_Start_Hour] [tinyint] NOT NULL,
	[Duration_Months] [int] NOT NULL,
	[Duration_Name] [nvarchar](max) NULL,
	[Duration_PeriodType] [tinyint] NOT NULL,
	[FieldServiceCost_LabourCost] [float] NULL,
	[FieldServiceCost_OohUpliftFactor] [float] NULL,
	[FieldServiceCost_PerformanceRate] [float] NULL,
	[FieldServiceCost_TimeAndMaterialShare] [float] NULL,
	[FieldServiceCost_TravelCost] [float] NULL,
	[FieldServiceCost_TravelTime] [float] NULL,
	[LogisticsCosts_ExpressDelivery] [float] NULL,
	[LogisticsCosts_HighAvailabilityHandling] [float] NULL,
	[LogisticsCosts_ReturnDeliveryFactory] [float] NULL,
	[LogisticsCosts_StandardDelivery] [float] NULL,
	[LogisticsCosts_StandardHandling] [float] NULL,
	[LogisticsCosts_TaxiCourierDelivery] [float] NULL,
	[MarkupOtherCosts_MarkupFactor] [float] NULL,
	[MarkupOtherCosts_ProlongationMarkup] [float] NULL,
	[MarkupOtherCosts_ProlongationMarkupFactor] [float] NULL,
	[ReactionTime_Minutes] [int] NULL,
	[ReactionTime_Name] [nvarchar](max) NULL,
	[ReactionTime_PeriodType] [tinyint] NULL,
	[Reinsurance_CurrencyId] [bigint] NULL,
	[Reinsurance_Flatfee] [float] NULL,
	[Reinsurance_UpliftFactor] [float] NULL,
 CONSTRAINT [PK_ProjectItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [ProjectCalculator].[Afr]  WITH CHECK ADD  CONSTRAINT [FK_Afr_ProjectItem_ProjectItemId] FOREIGN KEY([ProjectItemId])
REFERENCES [ProjectCalculator].[ProjectItem] ([Id])
GO
ALTER TABLE [ProjectCalculator].[Afr] CHECK CONSTRAINT [FK_Afr_ProjectItem_ProjectItemId]
GO
ALTER TABLE [ProjectCalculator].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [ProjectCalculator].[Project] CHECK CONSTRAINT [FK_Project_User_UserId]
GO
ALTER TABLE [ProjectCalculator].[ProjectItem]  WITH CHECK ADD  CONSTRAINT [FK_ProjectItem_Country_CountryId] FOREIGN KEY([CountryId])
REFERENCES [InputAtoms].[Country] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [ProjectCalculator].[ProjectItem] CHECK CONSTRAINT [FK_ProjectItem_Country_CountryId]
GO
ALTER TABLE [ProjectCalculator].[ProjectItem]  WITH CHECK ADD  CONSTRAINT [FK_ProjectItem_Currency_Reinsurance_CurrencyId] FOREIGN KEY([Reinsurance_CurrencyId])
REFERENCES [References].[Currency] ([Id])
GO
ALTER TABLE [ProjectCalculator].[ProjectItem] CHECK CONSTRAINT [FK_ProjectItem_Currency_Reinsurance_CurrencyId]
GO
ALTER TABLE [ProjectCalculator].[ProjectItem]  WITH CHECK ADD  CONSTRAINT [FK_ProjectItem_Project_ProjectId] FOREIGN KEY([ProjectId])
REFERENCES [ProjectCalculator].[Project] ([Id])
GO
ALTER TABLE [ProjectCalculator].[ProjectItem] CHECK CONSTRAINT [FK_ProjectItem_Project_ProjectId]
GO
ALTER TABLE [ProjectCalculator].[ProjectItem]  WITH CHECK ADD  CONSTRAINT [FK_ProjectItem_ReactionType_ReactionTypeId] FOREIGN KEY([ReactionTypeId])
REFERENCES [Dependencies].[ReactionType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [ProjectCalculator].[ProjectItem] CHECK CONSTRAINT [FK_ProjectItem_ReactionType_ReactionTypeId]
GO
ALTER TABLE [ProjectCalculator].[ProjectItem]  WITH CHECK ADD  CONSTRAINT [FK_ProjectItem_ServiceLocation_ServiceLocationId] FOREIGN KEY([ServiceLocationId])
REFERENCES [Dependencies].[ServiceLocation] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [ProjectCalculator].[ProjectItem] CHECK CONSTRAINT [FK_ProjectItem_ServiceLocation_ServiceLocationId]
GO
ALTER TABLE [ProjectCalculator].[ProjectItem]  WITH CHECK ADD  CONSTRAINT [FK_ProjectItem_Wg_WgId] FOREIGN KEY([WgId])
REFERENCES [InputAtoms].[Wg] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [ProjectCalculator].[ProjectItem] CHECK CONSTRAINT [FK_ProjectItem_Wg_WgId]
GO

IF COL_LENGTH('[Dependencies].[ReactionTime]', 'Minutes') IS NULL
	ALTER TABLE [Dependencies].[ReactionTime] ADD [Minutes] INT NOT NULL DEFAULT(0)
GO

IF COL_LENGTH('[Dependencies].[Availability]', 'Value') IS NULL
	ALTER TABLE [Dependencies].[Availability] ADD [Value] INT NOT NULL DEFAULT(0)
GO

IF COL_LENGTH('[Dependencies].[Availability]', 'IsMax') IS NULL
	ALTER TABLE [Dependencies].[Availability] ADD [IsMax] BIT NOT NULL DEFAULT(0)
GO

IF COL_LENGTH('[Dependencies].[ReactionType]', 'Coeff') IS NULL
	ALTER TABLE [Dependencies].[ReactionType] ADD [Coeff] INT NOT NULL DEFAULT(0)
GO

CREATE TABLE [ProjectCalculator].[AvailabilityWeight]
(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Day] [tinyint] NOT NULL,
	[Hour] [tinyint] NOT NULL,
	[Weight] [float] NOT NULL,
	CONSTRAINT [PK_AvailabilityWeight] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)
GO

CREATE FUNCTION [ProjectCalculator].[CalcAvailabilityCoeff]
(
	@startDay TINYINT,
	@startHour TINYINT,
	@endDay TINYINT,
	@endHour TINYINT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT

	SELECT
		@result = SUM([Weight])
	FROM
		[ProjectCalculator].[AvailabilityWeight]
	WHERE
		@startDay <= [Day] AND [Day] <= @endDay AND
		@startHour <= [Hour] AND [Hour] <= @endHour

	RETURN @result
END
GO

CREATE VIEW [ProjectCalculator].[AvailabilityWeightView]
AS
SELECT 
	*
FROM
(
	SELECT 
		[Hour],
		MIN(CASE WHEN [Day] = 0 THEN [Weight] END) AS Monday,
		MIN(CASE WHEN [Day] = 1 THEN [Weight] END) AS Tuesday,
		MIN(CASE WHEN [Day] = 2 THEN [Weight] END) AS Wednesday,
		MIN(CASE WHEN [Day] = 3 THEN [Weight] END) AS Thursday,
		MIN(CASE WHEN [Day] = 4 THEN [Weight] END) AS Friday,
		MIN(CASE WHEN [Day] = 5 THEN [Weight] END) AS Saturday,
		MIN(CASE WHEN [Day] = 6 THEN [Weight] END) AS Sunday
	FROM
		[ProjectCalculator].[AvailabilityWeight]
	GROUP BY
		[Hour]
) AS t
GO

INSERT INTO [ProjectCalculator].[AvailabilityWeight]([Day], [Hour], [Weight])
VALUES 
	(0, 0, 5),
	(0, 1, 5),
	(0, 2, 5),
	(0, 3, 5),
	(0, 4, 5),
	(0, 5, 5),
	(0, 6, 5),
	(0, 7, 5),
	(0, 8, 0),
	(0, 9, 0),
	(0, 10, 0),
	(0, 11, 0),
	(0, 12, 0),
	(0, 13, 0),
	(0, 14, 0),
	(0, 15, 0),
	(0, 16, 0),
	(0, 17, 0),
	(0, 18, 5),
	(0, 19, 5),
	(0, 20, 5),
	(0, 21, 5),
	(0, 22, 5),
	(0, 23, 5),

	(1, 0, 5),
	(1, 1, 5),
	(1, 2, 5),
	(1, 3, 5),
	(1, 4, 5),
	(1, 5, 5),
	(1, 6, 5),
	(1, 7, 5),
	(1, 8, 0),
	(1, 9, 0),
	(1, 10, 0),
	(1, 11, 0),
	(1, 12, 0),
	(1, 13, 0),
	(1, 14, 0),
	(1, 15, 0),
	(1, 16, 0),
	(1, 17, 0),
	(1, 18, 5),
	(1, 19, 5),
	(1, 20, 5),
	(1, 21, 5),
	(1, 22, 5),
	(1, 23, 5),

	(2, 0, 5),
	(2, 1, 5),
	(2, 2, 5),
	(2, 3, 5),
	(2, 4, 5),
	(2, 5, 5),
	(2, 6, 5),
	(2, 7, 5),
	(2, 8, 0),
	(2, 9, 0),
	(2, 10, 0),
	(2, 11, 0),
	(2, 12, 0),
	(2, 13, 0),
	(2, 14, 0),
	(2, 15, 0),
	(2, 16, 0),
	(2, 17, 0),
	(2, 18, 5),
	(2, 19, 5),
	(2, 20, 5),
	(2, 21, 5),
	(2, 22, 5),
	(2, 23, 5),

	(3, 0, 5),
	(3, 1, 5),
	(3, 2, 5),
	(3, 3, 5),
	(3, 4, 5),
	(3, 5, 5),
	(3, 6, 5),
	(3, 7, 5),
	(3, 8, 0),
	(3, 9, 0),
	(3, 10, 0),
	(3, 11, 0),
	(3, 12, 0),
	(3, 13, 0),
	(3, 14, 0),
	(3, 15, 0),
	(3, 16, 0),
	(3, 17, 0),
	(3, 18, 5),
	(3, 19, 5),
	(3, 20, 5),
	(3, 21, 5),
	(3, 22, 5),
	(3, 23, 5),

	(4, 0, 5),
	(4, 1, 5),
	(4, 2, 5),
	(4, 3, 5),
	(4, 4, 5),
	(4, 5, 5),
	(4, 6, 5),
	(4, 7, 5),
	(4, 8, 0),
	(4, 9, 0),
	(4, 10, 0),
	(4, 11, 0),
	(4, 12, 0),
	(4, 13, 0),
	(4, 14, 0),
	(4, 15, 0),
	(4, 16, 0),
	(4, 17, 0),
	(4, 18, 5),
	(4, 19, 5),
	(4, 20, 5),
	(4, 21, 5),
	(4, 22, 5),
	(4, 23, 5),

	(5, 0, 5),
	(5, 1, 5),
	(5, 2, 5),
	(5, 3, 5),
	(5, 4, 5),
	(5, 5, 5),
	(5, 6, 5),
	(5, 7, 5),
	(5, 8, 5),
	(5, 9, 5),
	(5, 10, 5),
	(5, 11, 5),
	(5, 12, 5),
	(5, 13, 5),
	(5, 14, 5),
	(5, 15, 5),
	(5, 16, 5),
	(5, 17, 5),
	(5, 18, 5),
	(5, 19, 5),
	(5, 20, 5),
	(5, 21, 5),
	(5, 22, 5),
	(5, 23, 5),

	(6, 0, 10),
	(6, 1, 10),
	(6, 2, 10),
	(6, 3, 10),
	(6, 4, 10),
	(6, 5, 10),
	(6, 6, 10),
	(6, 7, 10),
	(6, 8, 10),
	(6, 9, 10),
	(6, 10, 10),
	(6, 11, 10),
	(6, 12, 10),
	(6, 13, 10),
	(6, 14, 10),
	(6, 15, 10),
	(6, 16, 10),
	(6, 17, 10),
	(6, 18, 10),
	(6, 19, 10),
	(6, 20, 10),
	(6, 21, 10),
	(6, 22, 10),
	(6, 23, 10)

UPDATE [Dependencies].[ReactionTime] SET [Minutes] = -1			WHERE [Name] = 'none'
UPDATE [Dependencies].[ReactionTime] SET [Minutes] = 4 * 60     WHERE [Name] = '4h'
UPDATE [Dependencies].[ReactionTime] SET [Minutes] = 8 * 60     WHERE [Name] = '8h'
UPDATE [Dependencies].[ReactionTime] SET [Minutes] = 24 * 60    WHERE [Name] = '24h'
UPDATE [Dependencies].[ReactionTime] SET [Minutes] = 2 *24 * 60 WHERE [Name] = 'NBD'
UPDATE [Dependencies].[ReactionTime] SET [Minutes] = 3 *24 * 60 WHERE [Name] = '2nd Business Day'

UPDATE [Dependencies].[Availability] SET [Value] = [ProjectCalculator].[CalcAvailabilityCoeff](0, 8, 4, 17) WHERE [Name] = '9x5'
UPDATE [Dependencies].[Availability] SET [Value] = [ProjectCalculator].[CalcAvailabilityCoeff](0, 0, 6, 23), [IsMax] = 1 WHERE [Name] = '24x7'

UPDATE [Dependencies].[ReactionType] SET [Coeff] = -1  WHERE [Name] = 'none'
UPDATE [Dependencies].[ReactionType] SET [Coeff] = 100 WHERE [Name] = 'response'
UPDATE [Dependencies].[ReactionType] SET [Coeff] = 200 WHERE [Name] = 'recovery'