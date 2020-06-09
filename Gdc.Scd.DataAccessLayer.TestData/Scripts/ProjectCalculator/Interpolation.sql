USE [SCD_2]

IF OBJECT_ID('[ProjectCalculator].[InterpolateProjects]') IS NOT NULL
    DROP PROCEDURE [ProjectCalculator].[InterpolateProjects]
GO

IF OBJECT_ID('[ProjectCalculator].[GetY]') IS NOT NULL
    DROP PROCEDURE [ProjectCalculator].[GetY]
GO

IF OBJECT_ID('[ProjectCalculator].[InterpolateY]') IS NOT NULL
    DROP PROCEDURE [ProjectCalculator].[InterpolateY]
GO

IF OBJECT_ID('[ProjectCalculator].[InterpolateAfr]') IS NOT NULL
    DROP PROCEDURE [ProjectCalculator].[InterpolateAfr]
GO

IF OBJECT_ID('[ProjectCalculator].[NotCalculatedProjects]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[NotCalculatedProjects]
GO

IF OBJECT_ID('[ProjectCalculator].[FieldServiceReactionTimeType]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[FieldServiceReactionTimeType]
GO

IF OBJECT_ID('[ProjectCalculator].[FieldServiceAvailability]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[FieldServiceAvailability]
GO

IF OBJECT_ID('[ProjectCalculator].[LogisticsCosts]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[LogisticsCosts]
GO

IF OBJECT_ID('[ProjectCalculator].[MarkupOtherCosts]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[MarkupOtherCosts]
GO

IF OBJECT_ID('[ProjectCalculator].[Reinsurance]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[Reinsurance]
GO

IF OBJECT_ID('[ProjectCalculator].[ReinsuranceDuration]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[ReinsuranceDuration]
GO

IF OBJECT_ID('[ProjectCalculator].[ReinsuranceReactionTimeAvailability]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[ReinsuranceReactionTimeAvailability]
GO

IF OBJECT_ID('[ProjectCalculator].[AfrNotCalculatedProjects]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[AfrNotCalculatedProjects]
GO

IF OBJECT_ID('[ProjectCalculator].[InterpolatedOohUpliftFactor]') IS NOT NULL
    DROP VIEW [ProjectCalculator].[InterpolatedOohUpliftFactor]
GO

IF OBJECT_ID('[ProjectCalculator].[CalcReactionTimeAvailabilityCoeff]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[CalcReactionTimeAvailabilityCoeff]
GO

IF OBJECT_ID('[ProjectCalculator].[CalcReactionTimeTypeAvailabilityCoeff]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[CalcReactionTimeTypeAvailabilityCoeff]
GO

IF OBJECT_ID('[ProjectCalculator].[GetRecursiveAfrMonths]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetRecursiveAfrMonths]
GO

IF TYPE_ID('[ProjectCalculator].[XY]') IS NOT NULL
    DROP TYPE [ProjectCalculator].[XY]
GO

CREATE TYPE [ProjectCalculator].[XY] AS TABLE
(
	ProjectId BIGINT,
	X FLOAT,
	X_Result FLOAT,
	Y_Result FLOAT
)
GO

CREATE FUNCTION [ProjectCalculator].[CalcReactionTimeAvailabilityCoeff]
(
	@reactionTimeMinutes FLOAT,
	@availabilityValue FLOAT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT

	IF @availabilityValue = 0
		SET @result = @reactionTimeMinutes
	ELSE
		SET @result = @reactionTimeMinutes * @availabilityValue

	RETURN @result
END
GO

CREATE FUNCTION [ProjectCalculator].[CalcReactionTimeTypeAvailabilityCoeff]
(
	@reactionTimeMinutes FLOAT,
	@availabilityValue FLOAT,
	@typeCoeff FLOAT
)
RETURNS FLOAT
AS
BEGIN
	RETURN [ProjectCalculator].[CalcReactionTimeAvailabilityCoeff](@reactionTimeMinutes, @availabilityValue) * @typeCoeff
END
GO

CREATE FUNCTION [ProjectCalculator].[GetRecursiveAfrMonths]
(
	@maxAfrMonths INT,
	@projectMonths INT
)
RETURNS INT
AS
BEGIN
	DECLARE @months INT
	DECLARE @reaminder INT = @projectMonths % 12

	IF @reaminder = 0
		SET @months = @projectMonths - 12
	ELSE
		SET @months = @projectMonths - @reaminder

	RETURN @months
END
GO

CREATE VIEW [ProjectCalculator].[NotCalculatedProjects]
AS
SELECT 
	[Project].*,
	[ProjectCalculator].[CalcAvailabilityCoeff](
		[Availability_Start_Day], 
		[Availability_Start_Hour],
		[Availability_End_Day],
		[Availability_End_Hour]) AS Availability_Value_New
FROM
	[ProjectCalculator].[Project]
WHERE
	[IsCalculated] = 0
GO

CREATE VIEW [ProjectCalculator].[FieldServiceReactionTimeType]
AS
SELECT
	[FieldServiceReactionTimeType].*,
	[ReactionTime].[Name] AS ReactionTimeName,
	[ReactionTime].[Minutes] AS X,
	[NotCalculatedProjects].[Id] AS ProjectId
FROM
	[Hardware].[FieldServiceReactionTimeType]
INNER JOIN
	[Dependencies].[ReactionTimeType] ON [FieldServiceReactionTimeType].[ReactionTimeType] = [ReactionTimeType].[Id]
INNER JOIN
	[Dependencies].[ReactionTime] ON [ReactionTimeType].[ReactionTimeId] = [ReactionTime].[Id]
INNER JOIN
	[ProjectCalculator].[NotCalculatedProjects] ON 
		[NotCalculatedProjects].[CountryId] = [FieldServiceReactionTimeType].[Country] AND
		[NotCalculatedProjects].[WgId] = [FieldServiceReactionTimeType].[Wg] AND
		[NotCalculatedProjects].[ReactionTypeId] = [ReactionTimeType].[ReactionTypeId]
WHERE
	[FieldServiceReactionTimeType].[DeactivatedDateTime] IS NULL
GO

CREATE VIEW [ProjectCalculator].[FieldServiceAvailability]
AS
SELECT
	[FieldServiceAvailability].*,
	[NotCalculatedProjects].[Id] AS ProjectId,
	[NotCalculatedProjects].[Availability_Start_Day] AS ProjectStartDay,
	[NotCalculatedProjects].[Availability_Start_Hour] AS ProjectStartHour,
	[NotCalculatedProjects].[Availability_End_Day] AS ProjectEndDay,
	[NotCalculatedProjects].[Availability_End_Hour] AS ProjectEndHour,
	[Availability].[Name] AS AvailabilityName,
	[Availability].[Value] AS AvailabilityValue,
	[Availability].[IsMax] AS AvailabilityIsMax,
	[Availability].[Value] AS X
FROM
	[Hardware].[FieldServiceAvailability]
INNER JOIN
	[ProjectCalculator].[NotCalculatedProjects] ON 
		[NotCalculatedProjects].[CountryId] = [FieldServiceAvailability].[Country] AND
		[NotCalculatedProjects].[WgId] = [FieldServiceAvailability].[Wg] 
INNER JOIN
	[Dependencies].[Availability] ON [FieldServiceAvailability].[Availability] = [Availability].[Id]
WHERE
	[FieldServiceAvailability].[DeactivatedDateTime] IS NULL
GO

CREATE VIEW [ProjectCalculator].[InterpolatedOohUpliftFactor]
AS
WITH [UpliftMax] AS
(
	SELECT [ProjectCalculator].[CalcAvailabilityCoeff](0, 0, 6, 23) AS [Value]
),
[Uplift] AS
(
	SELECT
		[FieldServiceAvailability].[ProjectId],
		[FieldServiceAvailability].[OohUpliftFactor],
		[UpliftMax].[Value] AS [UpliftMax],
		[ProjectCalculator].[CalcAvailabilityCoeff]([ProjectStartDay], [ProjectStartHour], [ProjectEndDay], [ProjectEndHour]) AS Uplift
	FROM
		[ProjectCalculator].[FieldServiceAvailability], [UpliftMax]
	WHERE
		[FieldServiceAvailability].[AvailabilityIsMax] = 1
)
SELECT
	[ProjectId],
	[OohUpliftFactor] * [Uplift] / [UpliftMax] AS OohUpliftFactor
FROM
	[Uplift]
GO

CREATE VIEW [ProjectCalculator].[LogisticsCosts]
AS
SELECT
	[LogisticsCosts].*,
	[NotCalculatedProjects].[Id] AS ProjectId,
	[ReactionTime].[Name] AS ReactionTimeName,
	[ReactionTime].[Minutes] AS X
FROM
	[Hardware].[LogisticsCosts]
INNER JOIN
	[Dependencies].[ReactionTimeType] ON [LogisticsCosts].[ReactionTimeType] = [ReactionTimeType].[Id]
INNER JOIN
	[Dependencies].[ReactionTime] ON [ReactionTimeType].[ReactionTimeId] = [ReactionTime].[Id]
INNER JOIN
	[ProjectCalculator].[NotCalculatedProjects] ON 
		[NotCalculatedProjects].[CountryId] = [LogisticsCosts].[Country] AND
		[NotCalculatedProjects].[WgId] = [LogisticsCosts].[Wg] AND
		[NotCalculatedProjects].[ReactionTypeId] = [ReactionTimeType].[ReactionTypeId]
WHERE
	[LogisticsCosts].[Deactivated] = 0
GO

CREATE VIEW [ProjectCalculator].[MarkupOtherCosts]
AS
SELECT
	[MarkupOtherCosts].*,
	[NotCalculatedProjects].[Id] AS ProjectId,
	[ReactionTimeTypeAvailability].[Name] AS ReactionTimeTypeAvailabilityName,
	[ProjectCalculator].[CalcReactionTimeTypeAvailabilityCoeff]([ReactionTime].[Minutes], [Availability].[Value], [ReactionType].[Coeff]) AS X
FROM
	[Hardware].[MarkupOtherCosts]
INNER JOIN
	[Dependencies].[ReactionTimeTypeAvailability] ON [MarkupOtherCosts].[ReactionTimeTypeAvailability] = [ReactionTimeTypeAvailability].[Id]
INNER JOIN
	[Dependencies].[ReactionTime] ON [ReactionTimeTypeAvailability].[ReactionTimeId] = [ReactionTime].[Id]
INNER JOIN
	[Dependencies].[ReactionType] ON [ReactionTimeTypeAvailability].[ReactionTypeId] = [ReactionType].[Id]
INNER JOIN
	[Dependencies].[Availability] ON [ReactionTimeTypeAvailability].[AvailabilityId] = [Availability].[Id]
INNER JOIN
	[ProjectCalculator].[NotCalculatedProjects] ON 
		[NotCalculatedProjects].[CountryId] = [MarkupOtherCosts].[Country] AND
		[NotCalculatedProjects].[WgId] = [MarkupOtherCosts].[Wg] AND
		[NotCalculatedProjects].[ReactionTypeId] = [ReactionTimeTypeAvailability].[ReactionTypeId]
WHERE
	[MarkupOtherCosts].[Deactivated] = 0
GO

CREATE VIEW [ProjectCalculator].[Reinsurance]
AS
SELECT
	[Reinsurance].*,
	[NotCalculatedProjects].[Id] AS ProjectId
FROM
	[Hardware].[Reinsurance]
INNER JOIN
	[ProjectCalculator].[NotCalculatedProjects] ON [NotCalculatedProjects].[WgId] = [Reinsurance].[Wg]
WHERE
	[Reinsurance].[Deactivated] = 0
GO

CREATE VIEW [ProjectCalculator].[ReinsuranceDuration]
AS
SELECT
	t.[ProjectId],
	t.[ReinsuranceFlatfee],
	[Duration].[Name] AS DurationName,
	[Duration].[Value] * 12 AS X
FROM
(
	SELECT
		[ProjectId],
		[Duration],
		MIN([ReinsuranceFlatfee]) AS [ReinsuranceFlatfee]
	FROM
		[ProjectCalculator].[Reinsurance]
	GROUP BY
		[Wg],
		[Duration],
		[ProjectId]
) AS t
INNER JOIN
	[Dependencies].[Duration] ON t.[Duration] = [Duration].[Id]
GO

CREATE VIEW [ProjectCalculator].[ReinsuranceReactionTimeAvailability]
AS
SELECT
	t.[ProjectId],
	t.[ReinsuranceUpliftFactor],
	[ReactionTimeAvailability].[Name] AS ReactionTimeAvailabilityName,
	[ProjectCalculator].[CalcReactionTimeAvailabilityCoeff]([ReactionTime].[Minutes], [Availability].[Value]) AS X
FROM
(
	SELECT
		[ProjectId],
		[ReactionTimeAvailability],
		MIN([ReinsuranceUpliftFactor]) AS [ReinsuranceUpliftFactor]
	FROM
		[ProjectCalculator].[Reinsurance]
	GROUP BY
		[Wg],
		[ReactionTimeAvailability],
		[ProjectId]
) AS t
INNER JOIN
	[Dependencies].[ReactionTimeAvailability] ON t.[ReactionTimeAvailability] = [ReactionTimeAvailability].[Id]
INNER JOIN
	[Dependencies].[ReactionTime] ON [ReactionTimeAvailability].[ReactionTimeId] = [ReactionTime].[Id]
INNER JOIN
	[Dependencies].[Availability] ON [ReactionTimeAvailability].[AvailabilityId] = [Availability].[Id]
GO

CREATE VIEW [ProjectCalculator].[AfrNotCalculatedProjects]
AS
SELECT
	[NotCalculatedProjects].[Id] AS ProjectId,
	[NotCalculatedProjects].[Duration_Months] AS ProjectMonths,
	[AFR].[AFR],
	[Year].[Value] * 12 AS AfrMonths
FROM
	[ProjectCalculator].[NotCalculatedProjects]
INNER JOIN
	[Hardware].[AFR] ON [NotCalculatedProjects].[WgId] = [AFR].[Wg]
INNER JOIN
	[Dependencies].[Year] ON [AFR].[Year] = [Year].[Id]
WHERE
	[AFR].[Deactivated] = 0 AND [Year].[IsProlongation] = 0
GO

CREATE PROCEDURE [ProjectCalculator].[GetY]
(
	@xySql NVARCHAR(MAX),
	@conditionOperator NVARCHAR(2),
	@xy [ProjectCalculator].[XY] READONLY,
	@skip INT = 0,
	@isDescending BIT = 0
)
AS 
BEGIN
	DECLARE @y FLOAT
	DECLARE @orderByType NVARCHAR(4)

	IF @isDescending = 1
		SET @orderByType = 'DESC'
	ELSE
		SET @orderByType = 'ASC'

	DECLARE @sql NVARCHAR(MAX) = 
		N'SELECT
			ProjectId,
			X,
			X_Result,
			Y_Result
		  FROM
		  (
			SELECT
				ROW_NUMBER() OVER(PARTITION BY XY.ProjectId, XY.X ORDER BY ResultQuery.X ' + @orderByType + ') AS RowNumber,
				XY.ProjectId,
				XY.X,
				ResultQuery.X AS X_Result,
				ResultQuery.Y AS Y_Result
			FROM
				@xy AS XY
			LEFT JOIN
				(' + @xySql + ') AS ResultQuery ON ResultQuery.X ' + @conditionOperator + 'XY.X
		  ) AS t
		  WHERE
			RowNumber = @skip + 1'

	EXEC sp_executesql @sql, N'@xy [ProjectCalculator].[XY] READONLY, @skip INT', @xy, @skip
END
GO

CREATE PROCEDURE [ProjectCalculator].[InterpolateY]
(
	@xySql NVARCHAR(MAX),
	@xy [ProjectCalculator].[XY] READONLY,
	@tempResultTable NVARCHAR(MAX) = NULL
)
AS
BEGIN
	DECLARE @fullCompliance [ProjectCalculator].[XY]
	INSERT INTO @fullCompliance EXEC [ProjectCalculator].[GetY] @xySql, '=', @xy

	DECLARE @notFullCompliance [ProjectCalculator].[XY]
	INSERT INTO @notFullCompliance(ProjectId, X, X_Result, Y_Result) SELECT ProjectId, X, X_Result, Y_Result FROM @fullCompliance AS t WHERE Y_Result IS NULL

	DECLARE @less [ProjectCalculator].[XY]
	INSERT INTO @less EXEC [ProjectCalculator].[GetY] @xySql, '<', @notFullCompliance, 0, 1

	DECLARE @more [ProjectCalculator].[XY]
	INSERT INTO @more EXEC [ProjectCalculator].[GetY] @xySql, '>', @notFullCompliance

	DECLARE @moreThanMoreParam [ProjectCalculator].[XY]
	INSERT INTO 
		@moreThanMoreParam(ProjectId, X) 
	SELECT
		Less.ProjectId,
		Less.X
	FROM 
		@less AS Less
	INNER JOIN
		@more AS More ON Less.ProjectId = More.ProjectId AND Less.X = More.X
	WHERE
		Less.Y_Result IS NULL AND More.Y_Result IS NOT NULL

	DECLARE @moreThanMore [ProjectCalculator].[XY]
	INSERT INTO @moreThanMore EXEC [ProjectCalculator].[GetY] @xySql, '>', @moreThanMoreParam, 1

	DECLARE @lessThenLessParam [ProjectCalculator].[XY]
	INSERT INTO 
		@lessThenLessParam(ProjectId, X) 
	SELECT 
		Less.ProjectId,
		Less.X
	FROM 
		@less AS Less
	INNER JOIN
		@more AS More ON Less.ProjectId = More.ProjectId AND Less.X = More.X
	WHERE
		Less.Y_Result IS NOT NULL AND More.Y_Result IS NULL

	DECLARE @lessThenLess [ProjectCalculator].[XY]
	INSERT INTO @lessThenLess EXEC [ProjectCalculator].[GetY] @xySql, '<', @lessThenLessParam, 1, 1

	IF OBJECT_ID('tempdb..#InterpolateResults') IS NOT NULL 
		DROP TABLE #InterpolateResults;

	WITH UseType AS 
	(
		SELECT
			FullCompliance.ProjectId,
			FullCompliance.X,

			FullCompliance.X_Result AS FullCompliance_X_Result,
			FullCompliance.Y_Result AS FullCompliance_Y_Result,

			Less.X_Result AS Less_X_Result,
			Less.Y_Result AS Less_Y_Result,

			More.X_Result AS More_X_Result,
			More.Y_Result AS More_Y_Result,

			MoreThanMore.X_Result AS MoreThanMore_X_Result,
			MoreThanMore.Y_Result AS MoreThanMore_Y_Result,

			LessThenLess.X_Result AS LessThenLess_X_Result,
			LessThenLess.Y_Result AS LessThenLess_Y_Result,

			CASE 
				WHEN FullCompliance.X_Result  IS NOT NULL THEN 'FullCompliance'

				WHEN Less.X_Result IS NOT NULL THEN CASE 
					WHEN More.X_Result			IS NOT NULL THEN 'Less_More'
					WHEN LessThenLess.X_Result  IS NOT NULL THEN 'LessThenLess_Less'
				END

				WHEN More.X_Result IS NOT NULL AND MoreThanMore.X_Result IS NOT NULL THEN 'More_MoreThanMore'
			END AS UseType
		FROM
			@fullCompliance AS FullCompliance
		LEFT JOIN
			@less AS Less ON Less.X = FullCompliance.X
		LEFT JOIN
			@more AS More ON More.X = FullCompliance.X
		LEFT JOIN
			@moreThanMore AS MoreThanMore ON MoreThanMore.X = FullCompliance.X
		LEFT JOIN
			@lessThenLess AS LessThenLess ON LessThenLess.X = FullCompliance.X
	),
	XY AS 
	(
		SELECT
			ProjectId,
			X,
			CASE UseType WHEN 'FullCompliance' THEN FullCompliance_Y_Result END AS Y,

			CASE UseType
				WHEN 'Less_More'		 THEN Less_X_Result
				WHEN 'LessThenLess_Less' THEN LessThenLess_X_Result
				WHEN 'More_MoreThanMore' THEN More_X_Result
			END AS X1,

			CASE UseType
				WHEN 'Less_More'		 THEN Less_Y_Result
				WHEN 'LessThenLess_Less' THEN LessThenLess_Y_Result
				WHEN 'More_MoreThanMore' THEN More_Y_Result
			END AS Y1,

			CASE UseType
				WHEN 'Less_More'		 THEN More_X_Result
				WHEN 'LessThenLess_Less' THEN Less_X_Result
				WHEN 'More_MoreThanMore' THEN MoreThanMore_X_Result
			END AS X2,

			CASE UseType
				WHEN 'Less_More'		 THEN More_Y_Result
				WHEN 'LessThenLess_Less' THEN Less_Y_Result
				WHEN 'More_MoreThanMore' THEN MoreThanMore_Y_Result
			END AS Y2
		FROM
			UseType
	)
	SELECT
		ProjectId,
		X,
		CASE 
			WHEN XY.Y IS NOT NULL THEN XY.Y
			ELSE
				(X - X1)/(X2 - X1) * (Y2 - Y1) + Y1
		END AS Y
	INTO
		#InterpolateResults
	FROM
		XY

	IF @tempResultTable IS NULL
		SELECT * FROM #InterpolateResults
	ELSE BEGIN
		DECLARE @insertIntoResultTablSql NVARCHAR(MAX) = 
			N'INSERT INTO ' + @tempResultTable + '( ProjectId, X, Y) SELECT ProjectId, X, Y FROM #InterpolateResults'

		EXEC sp_executesql @insertIntoResultTablSql
	END
END
GO

CREATE PROCEDURE [ProjectCalculator].[InterpolateAfr]
AS
BEGIN
	DECLARE @xy [ProjectCalculator].[XY];

	WITH [NullAfr] AS 
	(
		SELECT 
			* 
		FROM 
			[ProjectCalculator].[AfrNotCalculatedProjects] 
		WHERE 
			[AFR] IS NULL AND [AfrMonths] < [ProjectMonths]
	),
	[NotExistingAfr] AS
	(
		SELECT
			*
		FROM
		(
			SELECT
				[ProjectId],
				[ProjectMonths],
				MAX([AfrMonths]) AS MaxAfrMonths
			FROM
				[ProjectCalculator].[AfrNotCalculatedProjects] 
			GROUP BY
				[ProjectId],
				[ProjectMonths]
		) AS t
		WHERE
			[MaxAfrMonths] < [ProjectMonths] OR
			[ProjectMonths] NOT IN(SELECT [AfrMonths] FROM [ProjectCalculator].[AfrNotCalculatedProjects])
	),
	[RecurciveAfr] AS 
	(
		SELECT 
			[ProjectId],
			[ProjectMonths] AS AfrMonths
		FROM	
			[NotExistingAfr]
		UNION ALL
		SELECT
			[RecurciveAfr].[ProjectId],
			[ProjectCalculator].[GetRecursiveAfrMonths]([NotExistingAfr].[MaxAfrMonths], [RecurciveAfr].[AfrMonths])
		FROM
			[RecurciveAfr], [NotExistingAfr]
		WHERE
			[RecurciveAfr].[ProjectId] = [NotExistingAfr].[ProjectId] AND
			[NotExistingAfr].[MaxAfrMonths] < [AfrMonths]
	)
	INSERT INTO @xy(ProjectId, X) 
	SELECT [ProjectId], [AfrMonths] FROM [NullAfr]
	UNION ALL
	SELECT [ProjectId], [AfrMonths] FROM [RecurciveAfr]

	CREATE TABLE #interpolatedAfr(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, AfrMonths AS X, Afr AS Y FROM [ProjectCalculator].[AfrNotCalculatedProjects]',
		@xy,
		'#interpolatedAfr'

	DELETE FROM [ProjectCalculator].[Afr] WHERE [ProjectId] IN (SELECT [ProjectId] FROM #interpolatedAfr)

	INSERT INTO [ProjectCalculator].[Afr]([ProjectId], [AFR], [Months], [IsProlongation])
	SELECT
		[ProjectId],
		
		CASE 
			WHEN [AFR] < [PreviousAfr] THEN [PreviousAfr] 
			ELSE [AFR] 
		END AS [AFR],

		[Months],
		0 AS [IsProlongation]
	FROM
	(
		SELECT 
			[InterpolatedAfr].[ProjectId],
			[InterpolatedAfr].[X] AS [Months],
			[InterpolatedAfr].[Y] AS [AFR],
			(
				SELECT TOP 1
					[AfrNotCalculatedProjects].[AFR]
				FROM
					[ProjectCalculator].[AfrNotCalculatedProjects]
				WHERE
					[AfrNotCalculatedProjects].[ProjectId] = [InterpolatedAfr].[ProjectId] AND 
					[AfrNotCalculatedProjects].[AfrMonths] < [InterpolatedAfr].[X]
				ORDER BY
					[AfrNotCalculatedProjects].[AfrMonths] DESC
			) AS PreviousAfr
		FROM
			#interpolatedAfr AS [InterpolatedAfr]
	) AS t

	DROP TABLE #interpolatedAfr
END
GO

CREATE PROCEDURE [ProjectCalculator].[InterpolateProjects]
AS
BEGIN
	DECLARE @projectReactionTime [ProjectCalculator].[XY] 
	INSERT INTO @projectReactionTime(ProjectId, X) SELECT [Id], [ReactionTime_Minutes] FROM [ProjectCalculator].[NotCalculatedProjects]

	DECLARE @projectAvailability [ProjectCalculator].[XY] 
	INSERT INTO @projectAvailability(ProjectId, X) SELECT [Id], [Availability_Value_New] FROM [ProjectCalculator].[NotCalculatedProjects]

	DECLARE @projectReactionTimeAvailability [ProjectCalculator].[XY] 
	INSERT INTO @projectReactionTimeAvailability(ProjectId, X) 
	SELECT 
		[Id], 
		[ProjectCalculator].[CalcReactionTimeAvailabilityCoeff]([ReactionTime_Minutes], [Availability_Value_New]) 
	FROM 
		[ProjectCalculator].[NotCalculatedProjects]

	DECLARE @projectReactionTimeTypeAvailability [ProjectCalculator].[XY] 
	INSERT INTO @projectReactionTimeTypeAvailability(ProjectId, X) 
	SELECT 
		[NotCalculatedProjects].[Id], 
		[ProjectCalculator].[CalcReactionTimeTypeAvailabilityCoeff]([ReactionTime_Minutes], [Availability_Value_New], [ReactionType].[Coeff]) 
	FROM 
		[ProjectCalculator].[NotCalculatedProjects]
	INNER JOIN
		[Dependencies].[ReactionType] ON [NotCalculatedProjects].[ReactionTypeId] = [ReactionType].[Id]

	DECLARE @projectDuration [ProjectCalculator].[XY] 
	INSERT INTO @projectDuration(ProjectId, X) SELECT [Id], [Duration_Months] FROM [ProjectCalculator].[NotCalculatedProjects]

	CREATE TABLE #PerformanceRate(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, PerformanceRate AS Y FROM [ProjectCalculator].[FieldServiceReactionTimeType]',
		@projectReactionTime,
		'#PerformanceRate'

	CREATE TABLE #TimeAndMaterialShare(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, TimeAndMaterialShare AS Y FROM [ProjectCalculator].[FieldServiceReactionTimeType]',
		@projectReactionTime,
		'#TimeAndMaterialShare'

	CREATE TABLE #ExpressDelivery(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, ExpressDelivery AS Y FROM [ProjectCalculator].[LogisticsCosts]',
		@projectReactionTime,
		'#ExpressDelivery'

	CREATE TABLE #HighAvailabilityHandling(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, HighAvailabilityHandling AS Y FROM [ProjectCalculator].[LogisticsCosts]',
		@projectReactionTime,
		'#HighAvailabilityHandling'

	CREATE TABLE #ReturnDeliveryFactory(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, ReturnDeliveryFactory AS Y FROM [ProjectCalculator].[LogisticsCosts]',
		@projectReactionTime,
		'#ReturnDeliveryFactory'

	CREATE TABLE #StandardDelivery(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, StandardDelivery AS Y FROM [ProjectCalculator].[LogisticsCosts]',
		@projectReactionTime,
		'#StandardDelivery'

	CREATE TABLE #StandardHandling(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, StandardHandling AS Y FROM [ProjectCalculator].[LogisticsCosts]',
		@projectReactionTime,
		'#StandardHandling'

	CREATE TABLE #TaxiCourierDelivery(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, TaxiCourierDelivery AS Y FROM [ProjectCalculator].[LogisticsCosts]',
		@projectReactionTime,
		'#TaxiCourierDelivery'

	CREATE TABLE #Markup(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, Markup AS Y FROM [ProjectCalculator].[MarkupOtherCosts]',
		@projectReactionTimeTypeAvailability,
		'#Markup'

	CREATE TABLE #MarkupFactor(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, MarkupFactor AS Y FROM [ProjectCalculator].[MarkupOtherCosts]',
		@projectReactionTimeTypeAvailability,
		'#MarkupFactor'

	CREATE TABLE #ProlongationMarkup(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, ProlongationMarkup AS Y FROM [ProjectCalculator].[MarkupOtherCosts]',
		@projectReactionTimeTypeAvailability,
		'#ProlongationMarkup'

	CREATE TABLE #ProlongationMarkupFactor(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, ProlongationMarkupFactor AS Y FROM [ProjectCalculator].[MarkupOtherCosts]',
		@projectReactionTimeTypeAvailability,
		'#ProlongationMarkupFactor'

	CREATE TABLE #ReinsuranceFlatfee(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, ReinsuranceFlatfee AS Y FROM [ProjectCalculator].[ReinsuranceDuration]',
		@projectDuration,
		'#ReinsuranceFlatfee';

	CREATE TABLE #ReinsuranceUpliftFactor(ProjectId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectId, X, ReinsuranceUpliftFactor AS Y FROM [ProjectCalculator].[ReinsuranceReactionTimeAvailability]',
		@projectReactionTimeAvailability,
		'#ReinsuranceUpliftFactor'

	EXEC [ProjectCalculator].[InterpolateAfr];

	WITH [AvailabilityFeeCountryCompany] AS
	(
		SELECT
			[Wg].[Id] AS [Wg],
			[Wg].[RoleCodeId],
			[AvailabilityFeeCountryCompany].*
		FROM
			[Hardware].[AvailabilityFeeCountryCompany]
		INNER JOIN
			[InputAtoms].[Wg] ON [Wg].[CompanyId] = [AvailabilityFeeCountryCompany].[Company]
		WHERE
			[AvailabilityFeeCountryCompany].[DeactivatedDateTime] IS NULL 
	),
	[Reinsurance_Currency] AS
	(
		SELECT
			[Wg],
			MIN([CurrencyReinsurance]) AS [Currencyid]
		FROM
			[ProjectCalculator].[Reinsurance]
		GROUP BY
			[Wg]
	)
	UPDATE 
		[ProjectCalculator].[Project]
	SET
		[IsCalculated] = 1,

		[Availability_Value] = [NotCalculatedProjects].[Availability_Value_New],

		[AvailabilityFee_AverageContractDuration] = [AvailabilityFeeCountryCompany].[AverageContractDuration],
		[AvailabilityFee_StockValueFj] = [AvailabilityFeeCountryCompany].[StockValueFj],
		[AvailabilityFee_StockValueMv] = [AvailabilityFeeCountryCompany].[StockValueMv],
		[AvailabilityFee_TotalLogisticsInfrastructureCost] = [AvailabilityFeeCountryCompany].[TotalLogisticsInfrastructureCost],

		[OnsiteHourlyRates] = [RoleCodeHourlyRates].[OnsiteHourlyRates],

		[FieldServiceCost_LabourCost] = [FieldServiceLocation].[LabourCost],
		[FieldServiceCost_PerformanceRate] = [PerformanceRate].[Y],
		[FieldServiceCost_TimeAndMaterialShare] = [TimeAndMaterialShare].[Y],
		[FieldServiceCost_TravelCost] = [FieldServiceLocation].[TravelCost],
		[FieldServiceCost_TravelTime] = [FieldServiceLocation].[TravelTime],
		[FieldServiceCost_OohUpliftFactor] = [InterpolatedOohUpliftFactor].[OohUpliftFactor],

		[LogisticsCosts_ExpressDelivery] = [ExpressDelivery].[Y],
		[LogisticsCosts_HighAvailabilityHandling] = [HighAvailabilityHandling].[Y],
		[LogisticsCosts_ReturnDeliveryFactory] = [ReturnDeliveryFactory].[Y],
		[LogisticsCosts_StandardDelivery] = [StandardDelivery].[Y],
		[LogisticsCosts_StandardHandling] = [StandardHandling].[Y],
		[LogisticsCosts_TaxiCourierDelivery] = [TaxiCourierDelivery].[Y],

		[MarkupOtherCosts_Markup] = [Markup].[Y],
		[MarkupOtherCosts_MarkupFactor] = [MarkupFactor].[Y],
		[MarkupOtherCosts_ProlongationMarkup] = [ProlongationMarkup].[Y],
		[MarkupOtherCosts_ProlongationMarkupFactor] = [ProlongationMarkupFactor].[Y],

		[Reinsurance_CurrencyId] = [Reinsurance_Currency].[Currencyid],
		[Reinsurance_Flatfee] = [ReinsuranceFlatfee].[Y],
		[Reinsurance_UpliftFactor] = [ReinsuranceUpliftFactor].[Y]
	FROM
		[ProjectCalculator].[NotCalculatedProjects]
	LEFT JOIN
		[AvailabilityFeeCountryCompany] ON 
			[NotCalculatedProjects].[CountryId] = [AvailabilityFeeCountryCompany].[Country] AND
			[NotCalculatedProjects].[WgId] = [AvailabilityFeeCountryCompany].[Wg]
	LEFT JOIN
		[Hardware].[RoleCodeHourlyRates] ON 
			[NotCalculatedProjects].[CountryId] = [RoleCodeHourlyRates].[Country] AND
			[AvailabilityFeeCountryCompany].[RoleCodeId] = [RoleCodeHourlyRates].[RoleCode]
	LEFT JOIN
		[Hardware].[FieldServiceLocation] ON 
			[NotCalculatedProjects].[CountryId] = [FieldServiceLocation].[Country] AND
			[NotCalculatedProjects].[WgId] = [FieldServiceLocation].[Wg] AND
			[NotCalculatedProjects].[ServiceLocationId] = [FieldServiceLocation].[ServiceLocation]
	LEFT JOIN 
		#PerformanceRate AS [PerformanceRate] ON [PerformanceRate].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#TimeAndMaterialShare AS [TimeAndMaterialShare] ON [TimeAndMaterialShare].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		[ProjectCalculator].[InterpolatedOohUpliftFactor] ON [InterpolatedOohUpliftFactor].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#ExpressDelivery AS [ExpressDelivery] ON [ExpressDelivery].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#HighAvailabilityHandling AS [HighAvailabilityHandling] ON [HighAvailabilityHandling].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#ReturnDeliveryFactory AS [ReturnDeliveryFactory] ON [ReturnDeliveryFactory].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#StandardDelivery AS [StandardDelivery] ON [StandardDelivery].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN 
		#StandardHandling AS [StandardHandling] ON [StandardHandling].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#TaxiCourierDelivery AS [TaxiCourierDelivery] ON [TaxiCourierDelivery].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#Markup AS [Markup] ON [Markup].[ProjectId] = [NotCalculatedProjects].[Id] 
	LEFT JOIN
		#MarkupFactor AS [MarkupFactor] ON [MarkupFactor].[ProjectId] = [NotCalculatedProjects].[Id] 
	LEFT JOIN
		#ProlongationMarkup AS [ProlongationMarkup] ON [ProlongationMarkup].[ProjectId] = [NotCalculatedProjects].[Id] 
	LEFT JOIN
		#ProlongationMarkupFactor AS [ProlongationMarkupFactor] ON [ProlongationMarkupFactor].[ProjectId] = [NotCalculatedProjects].[Id] 
	LEFT JOIN
		[Reinsurance_Currency] ON [NotCalculatedProjects].[WgId] = [Reinsurance_Currency].[Wg]
	LEFT JOIN
		#ReinsuranceFlatfee AS [ReinsuranceFlatfee] ON [ReinsuranceFlatfee].[ProjectId] = [NotCalculatedProjects].[Id]
	LEFT JOIN
		#ReinsuranceUpliftFactor AS [ReinsuranceUpliftFactor] ON [ReinsuranceUpliftFactor].[ProjectId] = [NotCalculatedProjects].[Id]
	WHERE
		[Project].[Id] = [NotCalculatedProjects].[Id] AND
		[RoleCodeHourlyRates].[Deactivated] = 0 AND
		[FieldServiceLocation].[DeactivatedDateTime] IS NULL

	DROP TABLE #PerformanceRate
	DROP TABLE #TimeAndMaterialShare
	DROP TABLE #ExpressDelivery
	DROP TABLE #HighAvailabilityHandling
	DROP TABLE #ReturnDeliveryFactory
	DROP TABLE #StandardDelivery
	DROP TABLE #StandardHandling
	DROP TABLE #TaxiCourierDelivery
	DROP TABLE #Markup
	DROP TABLE #MarkupFactor
	DROP TABLE #ProlongationMarkup
	DROP TABLE #ProlongationMarkupFactor
	DROP TABLE #ReinsuranceFlatfee
	DROP TABLE #ReinsuranceUpliftFactor
END
GO
