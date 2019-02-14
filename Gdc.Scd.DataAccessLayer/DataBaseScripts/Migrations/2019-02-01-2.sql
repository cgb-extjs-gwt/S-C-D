USE [Scd_2]
GO

ALTER VIEW [Dependencies].[ReactionTimeAvailability] AS
SELECT  
	[Id], 
	(CASE [Name] WHEN '' THEN 'none' ELSE [Name] END) AS [Name], 
	[ReactionTimeId], 
	[AvailabilityId], 
	[IsDisabled] 
FROM 
(
	SELECT  
		[ReactionTime_Avalability].[Id] AS [Id], 
		(
			(CASE [ReactionTime].[Name] WHEN 'none' THEN '' ELSE [ReactionTime].[Name] + ' ' END) + 
			(CASE [Availability].[Name] WHEN 'none' THEN '' ELSE [Availability].[Name] END)
		) 
		AS [Name], 
		[ReactionTime_Avalability].[ReactionTimeId] AS [ReactionTimeId], 
		[ReactionTime_Avalability].[AvailabilityId] AS [AvailabilityId], 
		[ReactionTime_Avalability].[IsDisabled] AS [IsDisabled] 
	FROM 
		[Dependencies].[ReactionTime_Avalability] 
	INNER JOIN 
		[Dependencies].[ReactionTime] ON [ReactionTime_Avalability].[ReactionTimeId] = [ReactionTime].[Id] 
	INNER JOIN 
		[Dependencies].[Availability] ON [ReactionTime_Avalability].[AvailabilityId] = [Availability].[Id]
) 
AS [t]
GO

ALTER VIEW [Dependencies].[ReactionTimeType] AS
SELECT  
	[Id], 
	(CASE [Name] WHEN '' THEN 'none' ELSE [Name] END) AS [Name], 
	[ReactionTimeId], 
	[ReactionTypeId], 
	[IsDisabled] 
FROM 
(
	SELECT  
		[ReactionTime_ReactionType].[Id] AS [Id], 
		(
			(CASE [ReactionTime].[Name] WHEN 'none' THEN '' ELSE [ReactionTime].[Name] + ' ' END) + 
			(CASE [ReactionType].[Name] WHEN 'none' THEN '' ELSE [ReactionType].[Name] END)
		) 
		AS [Name], 
		[ReactionTime_ReactionType].[ReactionTimeId] AS [ReactionTimeId], 
		[ReactionTime_ReactionType].[ReactionTypeId] AS [ReactionTypeId], 
		[ReactionTime_ReactionType].[IsDisabled] AS [IsDisabled] 
	FROM 
		[Dependencies].[ReactionTime_ReactionType] 
	INNER JOIN 
		[Dependencies].[ReactionTime] ON [ReactionTime_ReactionType].[ReactionTimeId] = [ReactionTime].[Id] 
	INNER JOIN 
		[Dependencies].[ReactionType] ON [ReactionTime_ReactionType].[ReactionTypeId] = [ReactionType].[Id]
) 
AS [t]
GO

ALTER VIEW [Dependencies].[ReactionTimeTypeAvailability] AS
SELECT  
	[Id], 
	(CASE [Name] WHEN '' THEN 'none' ELSE [Name] END) AS [Name], 
	[ReactionTimeId], 
	[AvailabilityId], 
	[ReactionTypeId], 
	[IsDisabled] 
FROM 
(
	SELECT  
		[ReactionTime_ReactionType_Avalability].[Id] AS [Id], 
		(
			(CASE [ReactionTime].[Name] WHEN 'none' THEN '' ELSE [ReactionTime].[Name] + ' ' END) + 
			(CASE [Availability].[Name] WHEN 'none' THEN '' ELSE [Availability].[Name] + ' ' END) + 
			(CASE [ReactionType].[Name] WHEN 'none' THEN '' ELSE [ReactionType].[Name] END)
		) 
		AS [Name], 
		[ReactionTime_ReactionType_Avalability].[ReactionTimeId] AS [ReactionTimeId], 
		[ReactionTime_ReactionType_Avalability].[AvailabilityId] AS [AvailabilityId], 
		[ReactionTime_ReactionType_Avalability].[ReactionTypeId] AS [ReactionTypeId], 
		[ReactionTime_ReactionType_Avalability].[IsDisabled] AS [IsDisabled] 
	FROM 
		[Dependencies].[ReactionTime_ReactionType_Avalability] 
	INNER JOIN 
		[Dependencies].[ReactionTime] ON [ReactionTime_ReactionType_Avalability].[ReactionTimeId] = [ReactionTime].[Id] 
	INNER JOIN 
		[Dependencies].[Availability] ON [ReactionTime_ReactionType_Avalability].[AvailabilityId] = [Availability].[Id] 
	INNER JOIN 
		[Dependencies].[ReactionType] ON [ReactionTime_ReactionType_Avalability].[ReactionTypeId] = [ReactionType].[Id]
) 
AS [t]
GO

ALTER VIEW [Dependencies].[YearAvailability] AS
SELECT  
	[Id], 
	(CASE [Name] WHEN '' THEN 'none' ELSE [Name] END) AS [Name], 
	[YearId], 
	[AvailabilityId], 
	[IsDisabled] 
FROM 
(
	SELECT  
		[Year_Availability].[Id] AS [Id], 
		(
			(CASE [Year].[Name] WHEN 'none' THEN '' ELSE [Year].[Name] + ' ' END) + 
			(CASE [Availability].[Name] WHEN 'none' THEN '' ELSE [Availability].[Name] END)
		) 
		AS [Name], 
		[Year_Availability].[YearId] AS [YearId], 
		[Year_Availability].[AvailabilityId] AS [AvailabilityId], 
		[Year_Availability].[IsDisabled] AS [IsDisabled] 
	FROM 
		[Dependencies].[Year_Availability] 
	INNER JOIN 
		[Dependencies].[Year] ON [Year_Availability].[YearId] = [Year].[Id] 
	INNER JOIN 
		[Dependencies].[Availability] ON [Year_Availability].[AvailabilityId] = [Availability].[Id]
) 
AS [t]
GO

DELETE 
	[SoftwareSolution].[SwSpMaintenance]
FROM 
	[SoftwareSolution].[SwSpMaintenance]
INNER JOIN 
	[Dependencies].[Year_Availability] ON [Year_Availability].[Id] = [SwSpMaintenance].[YearAvailability]
INNER JOIN
	[Dependencies].[Year] ON [Year].[Id] = [Year_Availability].[YearId]
INNER JOIN
	[Dependencies].[Availability] ON [Availability].[Id] = [Year_Availability].[AvailabilityId]
WHERE 
	[Availability].[Id] <> [SwSpMaintenance].[Availability]