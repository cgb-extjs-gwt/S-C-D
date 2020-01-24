USE Scd_2
GO

--Tables
PRINT N'Alter table started';

ALTER TABLE Dependencies.ReactionTime_Avalability			   ADD IsDisabled BIT NOT NULL DEFAULT 0
ALTER TABLE Dependencies.ReactionTime_ReactionType			   ADD IsDisabled BIT NOT NULL DEFAULT 0
ALTER TABLE Dependencies.ReactionTime_ReactionType_Avalability ADD IsDisabled BIT NOT NULL DEFAULT 0
ALTER TABLE Dependencies.Year_Availability					   ADD IsDisabled BIT NOT NULL DEFAULT 0

PRINT N'Alter tables finished';
GO

--Views
PRINT N'Alter views started';
GO

ALTER VIEW [Dependencies].[ReactionTimeAvailability] AS
SELECT  
	[ReactionTime_Avalability].[Id] AS [Id], 
	(([ReactionTime].[Name] + ' ' + [Availability].[Name])) AS [Name], 
	[ReactionTime_Avalability].[IsDisabled] 
FROM 
	[Dependencies].[ReactionTime_Avalability] 
INNER JOIN 
	[Dependencies].[ReactionTime] ON [ReactionTime_Avalability].[ReactionTimeId] = [ReactionTime].[Id] 
INNER JOIN 
	[Dependencies].[Availability] ON [ReactionTime_Avalability].[AvailabilityId] = [Availability].[Id]
GO

ALTER VIEW [Dependencies].[ReactionTimeType] AS
SELECT  
	[ReactionTime_ReactionType].[Id] AS [Id], 
	(([ReactionTime].[Name] + ' ' + [ReactionType].[Name])) AS [Name], 
	[ReactionTime_ReactionType].[IsDisabled] 
FROM 
	[Dependencies].[ReactionTime_ReactionType] 
INNER JOIN 
	[Dependencies].[ReactionTime] ON [ReactionTime_ReactionType].[ReactionTimeId] = [ReactionTime].[Id] 
INNER JOIN 
	[Dependencies].[ReactionType] ON [ReactionTime_ReactionType].[ReactionTypeId] = [ReactionType].[Id]
GO

ALTER VIEW [Dependencies].[ReactionTimeTypeAvailability] AS
SELECT  
	[ReactionTime_ReactionType_Avalability].[Id] AS [Id], 
	(([ReactionTime].[Name] + ' ' + [Availability].[Name] + ' ' + [ReactionType].[Name])) AS [Name], 
	[ReactionTime_ReactionType_Avalability].[IsDisabled] 
FROM 
	[Dependencies].[ReactionTime_ReactionType_Avalability] 
INNER JOIN 
	[Dependencies].[ReactionTime] ON [ReactionTime_ReactionType_Avalability].[ReactionTimeId] = [ReactionTime].[Id] 
INNER JOIN 
	[Dependencies].[Availability] ON [ReactionTime_ReactionType_Avalability].[AvailabilityId] = [Availability].[Id] 
INNER JOIN 
	[Dependencies].[ReactionType] ON [ReactionTime_ReactionType_Avalability].[ReactionTypeId] = [ReactionType].[Id]
GO

ALTER VIEW [Dependencies].[YearAvailability] AS
SELECT  
	[Year_Availability].[Id] AS [Id], 
	(([Year].[Name] + ' ' + [Availability].[Name])) AS [Name], 
	[Year_Availability].[IsDisabled] 
FROM 
	[Dependencies].[Year_Availability] 
INNER JOIN 
	[Dependencies].[Year] ON [Year_Availability].[YearId] = [Year].[Id] 
INNER JOIN 
	[Dependencies].[Availability] ON [Year_Availability].[AvailabilityId] = [Availability].[Id]
GO

PRINT N'Alter views finished';
GO

--Dependencies data
PRINT N'Dependencies data started';

INSERT INTO [Dependencies].[ReactionTime_ReactionType] ([ReactionTimeId], [ReactionTypeId], [IsDisabled])
SELECT  
	[ReactionTimeId] AS [ReactionTimeId], 
	[ReactionTypeId] AS [ReactionTypeId], 
	(1) AS [IsDisabled] 
FROM 
(
	SELECT  
		[ReactionTime].[Id] AS [ReactionTimeId], 
		[ReactionType].[Id] AS [ReactionTypeId] 
	FROM 
		[Dependencies].[ReactionTime] 
	CROSS JOIN 
		[Dependencies].[ReactionType]
	EXCEPT
	SELECT  
		[ReactionTimeId], 
		[ReactionTypeId] 
	FROM 
		[Dependencies].[ReactionTime_ReactionType]
) AS [t];

INSERT INTO [Dependencies].[ReactionTime_Avalability] ([ReactionTimeId], [AvailabilityId], [IsDisabled])
SELECT  
	[ReactionTimeId] AS [ReactionTimeId], 
	[AvailabilityId] AS [AvailabilityId], 
	(1) AS [IsDisabled] 
FROM 
(
	SELECT  
		[ReactionTime].[Id] AS [ReactionTimeId], 
		[Availability].[Id] AS [AvailabilityId] 
	FROM 
		[Dependencies].[ReactionTime] 
	CROSS JOIN 
		[Dependencies].[Availability]
	EXCEPT
	SELECT  
		[ReactionTimeId], 
		[AvailabilityId] 
	FROM 
		[Dependencies].[ReactionTime_Avalability]
) AS [t];

INSERT INTO [Dependencies].[ReactionTime_ReactionType_Avalability] ([ReactionTimeId], [ReactionTypeId], [AvailabilityId], [IsDisabled])
SELECT  
	[ReactionTimeId] AS [ReactionTimeId], 
	[ReactionTypeId] AS [ReactionTypeId], 
	[AvailabilityId] AS [AvailabilityId], 
	(1) AS [IsDisabled] 
FROM 
(
	SELECT  
		[ReactionTime].[Id] AS [ReactionTimeId], 
		[ReactionType].[Id] AS [ReactionTypeId], 
		[Availability].[Id] AS [AvailabilityId] 
	FROM 
		[Dependencies].[ReactionTime] 
	CROSS JOIN 
		[Dependencies].[ReactionType] 
	CROSS JOIN 
		[Dependencies].[Availability]
	EXCEPT
	SELECT  
		[ReactionTimeId], 
		[ReactionTypeId], 
		[AvailabilityId] 
	FROM 
		[Dependencies].[ReactionTime_ReactionType_Avalability]
) AS [t];

INSERT INTO [Dependencies].[Year_Availability] ([YearId], [AvailabilityId], [IsDisabled])
SELECT  
	[YearId] AS [YearId], 
	[AvailabilityId] AS [AvailabilityId], 
	(1) AS [IsDisabled] 
FROM 
(
	SELECT  
		[Year].[Id] AS [YearId], 
		[Availability].[Id] AS [AvailabilityId] 
	FROM 
		[Dependencies].[Year] 
	CROSS JOIN 
		[Dependencies].[Availability]
	EXCEPT
	SELECT  
		[YearId], 
		[AvailabilityId] 
	FROM 
		[Dependencies].[Year_Availability]
) AS [t]

PRINT N'Dependencies data finished';

--Cost block data
PRINT N'Cost block data started';

DECLARE @wgType INT = 1
DECLARE @isSoftware BIT = 0
DECLARE @isMaster BIT = 1
DECLARE @DeactivatedDateTime DATETIME = GetUtcDate()

--FieldServiceCost
PRINT N'FieldServiceCost started';

SELECT  
	[Country].[Id] AS [Country], 
	[Wg].[PlaId] AS [Pla], 
	[Wg].[Id] AS [Wg], 
	[ServiceLocation].[Id] AS [ServiceLocation], 
	[ReactionTimeType].[Id] AS [ReactionTimeType] 
INTO 
	[#FieldServiceCost_Coordinates] 
FROM 
(
	SELECT  * FROM [InputAtoms].[Country] WHERE [IsMaster] = @isMaster
) 
AS [Country] 
CROSS JOIN 
	(SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware) AS [Wg] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[ServiceLocation]) AS [ServiceLocation] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[ReactionTimeType]) AS [ReactionTimeType];

INSERT INTO [Hardware].[FieldServiceCost] ([Country], [Pla], [Wg], [ServiceLocation], [ReactionTimeType])
SELECT  [Country], [Pla], [Wg], [ServiceLocation], [ReactionTimeType] FROM [#FieldServiceCost_Coordinates]
EXCEPT
SELECT  [Country], [Pla], [Wg], [ServiceLocation], [ReactionTimeType] FROM [Hardware].[FieldServiceCost] WHERE [DeactivatedDateTime] IS NULL;

DROP TABLE [#FieldServiceCost_Coordinates]

PRINT N'FieldServiceCost finished';

--LogisticsCosts
PRINT N'LogisticsCosts started';

SELECT  
	[Country].[Id] AS [Country], 
	[Wg].[PlaId] AS [Pla], 
	[Wg].[Id] AS [Wg], 
	[ReactionTimeType].[Id] AS [ReactionTimeType] 
INTO 
	[#LogisticsCosts_Coordinates] 
FROM 
(
	SELECT  * FROM [InputAtoms].[Country] WHERE [IsMaster] = @isMaster
) 
AS [Country] 
CROSS JOIN 
	(SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware) AS [Wg] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[ReactionTimeType]) AS [ReactionTimeType];

INSERT INTO [Hardware].[LogisticsCosts] ([Country], [Pla], [Wg], [ReactionTimeType])
SELECT  [Country], [Pla], [Wg], [ReactionTimeType] FROM [#LogisticsCosts_Coordinates]
EXCEPT
SELECT  [Country], [Pla], [Wg], [ReactionTimeType] FROM [Hardware].[LogisticsCosts] WHERE [DeactivatedDateTime] IS NULL;

DROP TABLE [#LogisticsCosts_Coordinates]

PRINT N'LogisticsCosts finished';

--MarkupOtherCosts
PRINT N'MarkupOtherCosts started';

SELECT  
	[Country].[Id] AS [Country], 
	[Wg].[PlaId] AS [Pla], 
	[Wg].[Id] AS [Wg], 
	[ReactionTimeTypeAvailability].[Id] AS [ReactionTimeTypeAvailability] 
INTO 
	[#MarkupOtherCosts_Coordinates] 
FROM 
(
	SELECT  * FROM [InputAtoms].[Country] WHERE [IsMaster] = @isMaster
) 
AS [Country] 
CROSS JOIN 
	(SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware) AS [Wg] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[ReactionTimeTypeAvailability]) AS [ReactionTimeTypeAvailability];

INSERT INTO [Hardware].[MarkupOtherCosts] ([Country], [Pla], [Wg], [ReactionTimeTypeAvailability])
SELECT  [Country], [Pla], [Wg], [ReactionTimeTypeAvailability] FROM [#MarkupOtherCosts_Coordinates]
EXCEPT
SELECT  [Country], [Pla], [Wg], [ReactionTimeTypeAvailability] FROM [Hardware].[MarkupOtherCosts] WHERE [DeactivatedDateTime] IS NULL;

DROP TABLE [#MarkupOtherCosts_Coordinates]

PRINT N'MarkupOtherCosts finished';

--ProlongationMarkup
PRINT N'ProlongationMarkup started';

SELECT  
	[Country].[Id] AS [Country], 
	[Wg].[PlaId] AS [Pla], 
	[Wg].[Id] AS [Wg], 
	[ReactionTimeTypeAvailability].[Id] AS [ReactionTimeTypeAvailability] 
INTO 
	[#ProlongationMarkup_Coordinates] 
FROM 
(
	SELECT  * FROM [InputAtoms].[Country] WHERE [IsMaster] = @isMaster
) 
AS [Country] 
CROSS JOIN 
	(SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware) AS [Wg] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[ReactionTimeTypeAvailability]) AS [ReactionTimeTypeAvailability];

INSERT INTO [Hardware].[ProlongationMarkup] ([Country], [Pla], [Wg], [ReactionTimeTypeAvailability])
SELECT  [Country], [Pla], [Wg], [ReactionTimeTypeAvailability] FROM [#ProlongationMarkup_Coordinates]
EXCEPT
SELECT  [Country], [Pla], [Wg], [ReactionTimeTypeAvailability] FROM [Hardware].[ProlongationMarkup] WHERE [DeactivatedDateTime] IS NULL;

DROP TABLE [#ProlongationMarkup_Coordinates]

PRINT N'ProlongationMarkup finished';

--MarkupStandardWaranty
PRINT N'MarkupStandardWaranty started';

SELECT  
	[Country].[Id] AS [Country], 
	[Wg].[PlaId] AS [Pla], 
	[Wg].[Id] AS [Wg], 
	[ReactionTimeTypeAvailability].[Id] AS [ReactionTimeTypeAvailability] 
INTO 
	[#MarkupStandardWaranty_Coordinates] 
FROM 
(
	SELECT  * FROM [InputAtoms].[Country] WHERE [IsMaster] = @isMaster
) 
AS [Country] 
CROSS JOIN 
	(SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware) AS [Wg] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[ReactionTimeTypeAvailability]) AS [ReactionTimeTypeAvailability];

INSERT INTO [Hardware].[MarkupStandardWaranty] ([Country], [Pla], [Wg], [ReactionTimeTypeAvailability])
SELECT  [Country], [Pla], [Wg], [ReactionTimeTypeAvailability] FROM [#MarkupStandardWaranty_Coordinates]
EXCEPT
SELECT  [Country], [Pla], [Wg], [ReactionTimeTypeAvailability] FROM [Hardware].[MarkupStandardWaranty] WHERE [DeactivatedDateTime] IS NULL;

DROP TABLE [#MarkupStandardWaranty_Coordinates]

PRINT N'MarkupStandardWaranty finished';

--SwSpMaintenance
PRINT N'SwSpMaintenance started';

SELECT  
	[Sfab].[PlaId] AS [Pla], 
	[Sog].[SFabId] AS [Sfab], 
	[SwDigit].[SogId] AS [Sog], 
	[SwDigit].[Id] AS [SwDigit], 
	[YearAvailability].[Id] AS [YearAvailability], 
	[Availability].[Id] AS [Availability] 
INTO 
	[#SwSpMaintenance_Coordinates] 
FROM 
(
	SELECT  * FROM [InputAtoms].[SwDigit] WHERE [DeactivatedDateTime] IS NULL
) 
AS [SwDigit] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[YearAvailability]) AS [YearAvailability] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[Availability]) AS [Availability] 
INNER JOIN 
	(SELECT  * FROM [InputAtoms].[Sog] WHERE [DeactivatedDateTime] IS NULL) AS [Sog] ON [SwDigit].[SogId] = [Sog].[Id] 
INNER JOIN 
	(SELECT  * FROM [InputAtoms].[Sfab] WHERE [DeactivatedDateTime] IS NULL) AS [Sfab] ON [Sog].[SFabId] = [Sfab].[Id];

INSERT INTO [SoftwareSolution].[SwSpMaintenance] ([Pla], [Sfab], [Sog], [SwDigit], [YearAvailability], [Availability])
SELECT  [Pla], [Sfab], [Sog], [SwDigit], [YearAvailability], [Availability] FROM [#SwSpMaintenance_Coordinates]
EXCEPT
SELECT  [Pla], [Sfab], [Sog], [SwDigit], [YearAvailability], [Availability] FROM [SoftwareSolution].[SwSpMaintenance] WHERE [DeactivatedDateTime] IS NULL;

DROP TABLE [#SwSpMaintenance_Coordinates]

PRINT N'SwSpMaintenance finished';

PRINT N'Cost block data finished';
