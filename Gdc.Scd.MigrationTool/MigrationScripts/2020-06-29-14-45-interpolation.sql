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

IF OBJECT_ID('[ProjectCalculator].[GetCalculatingProjectItems]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetCalculatingProjectItems]
GO

IF OBJECT_ID('[ProjectCalculator].[GetFieldServiceReactionTimeType]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetFieldServiceReactionTimeType]
GO

IF OBJECT_ID('[ProjectCalculator].[GetFieldServiceAvailability]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetFieldServiceAvailability]
GO

IF OBJECT_ID('[ProjectCalculator].[GetInterpolatedOohUpliftFactor]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetInterpolatedOohUpliftFactor]
GO

IF OBJECT_ID('[ProjectCalculator].[GetLogisticsCosts]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetLogisticsCosts]
GO

IF OBJECT_ID('[ProjectCalculator].[GetMarkupOtherCosts]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetMarkupOtherCosts]
GO

IF OBJECT_ID('[ProjectCalculator].[GetReinsurance]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetReinsurance]
GO

IF OBJECT_ID('[ProjectCalculator].[GetReinsuranceDuration]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetReinsuranceDuration]
GO

IF OBJECT_ID('[ProjectCalculator].[GetReinsuranceReactionTimeAvailability]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetReinsuranceReactionTimeAvailability]
GO

IF OBJECT_ID('[ProjectCalculator].[GetAfrCalculatingProjectItems]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetAfrCalculatingProjectItems]
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
	ProjectItemId BIGINT,
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

CREATE FUNCTION [ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT 
	[ProjectItem].*,
	[ProjectCalculator].[CalcAvailabilityCoeff](
		[Availability_Start_Day], 
		[Availability_Start_Hour],
		[Availability_End_Day],
		[Availability_End_Hour]) AS Availability_Value_New
FROM
	[ProjectCalculator].[ProjectItem]
WHERE
	[Id] IN (SELECT [Id] FROM @projectItemIds)
GO

CREATE FUNCTION [ProjectCalculator].[GetFieldServiceReactionTimeType](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	[FieldServiceReactionTimeType].*,
	[ReactionTime].[Name] AS ReactionTimeName,
	[ReactionTime].[Minutes] AS X,
	[CalculatingProjectItems].[Id] AS ProjectItemId
FROM
	[Hardware].[FieldServiceReactionTimeType]
INNER JOIN
	[Dependencies].[ReactionTimeType] ON [FieldServiceReactionTimeType].[ReactionTimeType] = [ReactionTimeType].[Id]
INNER JOIN
	[Dependencies].[ReactionTime] ON [ReactionTimeType].[ReactionTimeId] = [ReactionTime].[Id]
INNER JOIN
	[ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds) AS [CalculatingProjectItems] ON 
		[CalculatingProjectItems].[CountryId] = [FieldServiceReactionTimeType].[Country] AND
		[CalculatingProjectItems].[WgId] = [FieldServiceReactionTimeType].[Wg] AND
		[CalculatingProjectItems].[ReactionTypeId] = [ReactionTimeType].[ReactionTypeId]
WHERE
	[FieldServiceReactionTimeType].[DeactivatedDateTime] IS NULL
GO

CREATE FUNCTION [ProjectCalculator].[GetFieldServiceAvailability](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	[FieldServiceAvailability].*,
	[CalculatingProjectItems].[Id] AS ProjectItemId,
	[CalculatingProjectItems].[Availability_Start_Day] AS ProjectStartDay,
	[CalculatingProjectItems].[Availability_Start_Hour] AS ProjectStartHour,
	[CalculatingProjectItems].[Availability_End_Day] AS ProjectEndDay,
	[CalculatingProjectItems].[Availability_End_Hour] AS ProjectEndHour,
	[Availability].[Name] AS AvailabilityName,
	[Availability].[Value] AS AvailabilityValue,
	[Availability].[IsMax] AS AvailabilityIsMax,
	[Availability].[Value] AS X
FROM
	[Hardware].[FieldServiceAvailability]
INNER JOIN
	[ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds) AS [CalculatingProjectItems] ON 
		[CalculatingProjectItems].[CountryId] = [FieldServiceAvailability].[Country] AND
		[CalculatingProjectItems].[WgId] = [FieldServiceAvailability].[Wg] 
INNER JOIN
	[Dependencies].[Availability] ON [FieldServiceAvailability].[Availability] = [Availability].[Id]
WHERE
	[FieldServiceAvailability].[DeactivatedDateTime] IS NULL
GO

CREATE FUNCTION [ProjectCalculator].[GetInterpolatedOohUpliftFactor](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
WITH [UpliftMax] AS
(
	SELECT [ProjectCalculator].[CalcAvailabilityCoeff](0, 0, 6, 23) AS [Value]
),
[Uplift] AS
(
	SELECT
		[FieldServiceAvailability].[ProjectItemId],
		[FieldServiceAvailability].[OohUpliftFactor],
		[UpliftMax].[Value] AS [UpliftMax],
		[ProjectCalculator].[CalcAvailabilityCoeff]([ProjectStartDay], [ProjectStartHour], [ProjectEndDay], [ProjectEndHour]) AS Uplift
	FROM
		[ProjectCalculator].[GetFieldServiceAvailability](@projectItemIds) AS[FieldServiceAvailability], [UpliftMax]
	WHERE
		[FieldServiceAvailability].[AvailabilityIsMax] = 1
)
SELECT
	[ProjectItemId],
	[OohUpliftFactor] * [Uplift] / [UpliftMax] AS OohUpliftFactor
FROM
	[Uplift]
GO

CREATE FUNCTION [ProjectCalculator].[GetLogisticsCosts](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	[LogisticsCosts].*,
	[CalculatingProjectItems].[Id] AS ProjectItemId,
	[ReactionTime].[Name] AS ReactionTimeName,
	[ReactionTime].[Minutes] AS X
FROM
	[Hardware].[LogisticsCosts]
INNER JOIN
	[Dependencies].[ReactionTimeType] ON [LogisticsCosts].[ReactionTimeType] = [ReactionTimeType].[Id]
INNER JOIN
	[Dependencies].[ReactionTime] ON [ReactionTimeType].[ReactionTimeId] = [ReactionTime].[Id]
INNER JOIN
	[ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds) AS [CalculatingProjectItems] ON 
		[CalculatingProjectItems].[CountryId] = [LogisticsCosts].[Country] AND
		[CalculatingProjectItems].[WgId] = [LogisticsCosts].[Wg] AND
		[CalculatingProjectItems].[ReactionTypeId] = [ReactionTimeType].[ReactionTypeId]
WHERE
	[LogisticsCosts].[Deactivated] = 0
GO

CREATE FUNCTION [ProjectCalculator].[GetMarkupOtherCosts](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	[MarkupOtherCosts].*,
	[CalculatingProjectItems].[Id] AS ProjectItemId,
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
	[ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds) AS [CalculatingProjectItems] ON 
		[CalculatingProjectItems].[CountryId] = [MarkupOtherCosts].[Country] AND
		[CalculatingProjectItems].[WgId] = [MarkupOtherCosts].[Wg] AND
		[CalculatingProjectItems].[ReactionTypeId] = [ReactionTimeTypeAvailability].[ReactionTypeId]
WHERE
	[MarkupOtherCosts].[Deactivated] = 0
GO

CREATE FUNCTION [ProjectCalculator].[GetReinsurance](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	[Reinsurance].*,
	[CalculatingProjectItems].[Id] AS ProjectItemId
FROM
	[Hardware].[Reinsurance]
INNER JOIN
	[ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds) AS [CalculatingProjectItems] ON [CalculatingProjectItems].[WgId] = [Reinsurance].[Wg]
WHERE
	[Reinsurance].[Deactivated] = 0
GO

CREATE FUNCTION [ProjectCalculator].[GetReinsuranceDuration](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	t.[ProjectItemId],
	t.[ReinsuranceFlatfee],
	[Duration].[Name] AS DurationName,
	[Duration].[Value] * 12 AS X
FROM
(
	SELECT
		[ProjectItemId],
		[Duration],
		MIN([ReinsuranceFlatfee]) AS [ReinsuranceFlatfee]
	FROM
		[ProjectCalculator].[GetReinsurance](@projectItemIds) AS [Reinsurance]
	GROUP BY
		[Wg],
		[Duration],
		[ProjectItemId]
) AS t
INNER JOIN
	[Dependencies].[Duration] ON t.[Duration] = [Duration].[Id]
GO

CREATE FUNCTION [ProjectCalculator].[GetReinsuranceReactionTimeAvailability](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	t.[ProjectItemId],
	t.[ReinsuranceUpliftFactor],
	[ReactionTimeAvailability].[Name] AS ReactionTimeAvailabilityName,
	[ProjectCalculator].[CalcReactionTimeAvailabilityCoeff]([ReactionTime].[Minutes], [Availability].[Value]) AS X
FROM
(
	SELECT
		[ProjectItemId],
		[ReactionTimeAvailability],
		MIN([ReinsuranceUpliftFactor]) AS [ReinsuranceUpliftFactor]
	FROM
		[ProjectCalculator].[GetReinsurance](@projectItemIds) AS [Reinsurance]
	GROUP BY
		[Wg],
		[ReactionTimeAvailability],
		[ProjectItemId]
) AS t
INNER JOIN
	[Dependencies].[ReactionTimeAvailability] ON t.[ReactionTimeAvailability] = [ReactionTimeAvailability].[Id]
INNER JOIN
	[Dependencies].[ReactionTime] ON [ReactionTimeAvailability].[ReactionTimeId] = [ReactionTime].[Id]
INNER JOIN
	[Dependencies].[Availability] ON [ReactionTimeAvailability].[AvailabilityId] = [Availability].[Id]
GO

CREATE FUNCTION [ProjectCalculator].[GetAfrCalculatingProjectItems](@projectItemIds [dbo].[ListID] READONLY)
RETURNS TABLE
AS
RETURN
SELECT
	[CalculatingProjectItems].[Id] AS ProjectItemId,
	[CalculatingProjectItems].[Duration_Months] AS ProjectMonths,
	[AFR].[AFR],
	[Year].[Value] * 12 AS AfrMonths
FROM
	[ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds) AS [CalculatingProjectItems]
INNER JOIN
	[Hardware].[AFR] ON [CalculatingProjectItems].[WgId] = [AFR].[Wg]
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
			ProjectItemId,
			X,
			X_Result,
			Y_Result
		  FROM
		  (
			SELECT
				ROW_NUMBER() OVER(PARTITION BY XY.ProjectItemId, XY.X ORDER BY ResultQuery.X ' + @orderByType + ') AS RowNumber,
				XY.ProjectItemId,
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
	INSERT INTO @notFullCompliance(ProjectItemId, X, X_Result, Y_Result) SELECT ProjectItemId, X, X_Result, Y_Result FROM @fullCompliance AS t WHERE Y_Result IS NULL

	DECLARE @less [ProjectCalculator].[XY]
	INSERT INTO @less EXEC [ProjectCalculator].[GetY] @xySql, '<', @notFullCompliance, 0, 1

	DECLARE @more [ProjectCalculator].[XY]
	INSERT INTO @more EXEC [ProjectCalculator].[GetY] @xySql, '>', @notFullCompliance

	DECLARE @moreThanMoreParam [ProjectCalculator].[XY]
	INSERT INTO 
		@moreThanMoreParam(ProjectItemId, X) 
	SELECT
		Less.ProjectItemId,
		Less.X
	FROM 
		@less AS Less
	INNER JOIN
		@more AS More ON Less.ProjectItemId = More.ProjectItemId AND Less.X = More.X
	WHERE
		Less.Y_Result IS NULL AND More.Y_Result IS NOT NULL

	DECLARE @moreThanMore [ProjectCalculator].[XY]
	INSERT INTO @moreThanMore EXEC [ProjectCalculator].[GetY] @xySql, '>', @moreThanMoreParam, 1

	DECLARE @lessThenLessParam [ProjectCalculator].[XY]
	INSERT INTO 
		@lessThenLessParam(ProjectItemId, X) 
	SELECT 
		Less.ProjectItemId,
		Less.X
	FROM 
		@less AS Less
	INNER JOIN
		@more AS More ON Less.ProjectItemId = More.ProjectItemId AND Less.X = More.X
	WHERE
		Less.Y_Result IS NOT NULL AND More.Y_Result IS NULL

	DECLARE @lessThenLess [ProjectCalculator].[XY]
	INSERT INTO @lessThenLess EXEC [ProjectCalculator].[GetY] @xySql, '<', @lessThenLessParam, 1, 1

	IF OBJECT_ID('tempdb..#InterpolateResults') IS NOT NULL 
		DROP TABLE #InterpolateResults;

	WITH UseType AS 
	(
		SELECT
			FullCompliance.ProjectItemId,
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
			ProjectItemId,
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
		ProjectItemId,
		X,
		CASE 
			WHEN XY.Y IS NOT NULL THEN XY.Y
			WHEN XY.X1 <> XY.X2 THEN (X - X1)/(X2 - X1) * (Y2 - Y1) + Y1
		END AS Y
	INTO
		#InterpolateResults
	FROM
		XY

	IF @tempResultTable IS NULL
		SELECT * FROM #InterpolateResults
	ELSE BEGIN
		DECLARE @insertIntoResultTablSql NVARCHAR(MAX) = 
			N'INSERT INTO ' + @tempResultTable + '( ProjectItemId, X, Y) SELECT ProjectItemId, X, Y FROM #InterpolateResults'

		EXEC sp_executesql @insertIntoResultTablSql
	END
END
GO

CREATE PROCEDURE [ProjectCalculator].[InterpolateAfr] @projectItemIds [dbo].[ListID] READONLY
AS
BEGIN
	DECLARE @xy [ProjectCalculator].[XY]

	SELECT * INTO #AfrCalculatingProjectItems FROM [ProjectCalculator].[GetAfrCalculatingProjectItems](@projectItemIds);

	WITH [NullAfr] AS 
	(
		SELECT 
			* 
		FROM 
			#AfrCalculatingProjectItems 
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
				[ProjectItemId],
				[ProjectMonths],
				MAX([AfrMonths]) AS MaxAfrMonths
			FROM
				#AfrCalculatingProjectItems 
			GROUP BY
				[ProjectItemId],
				[ProjectMonths]
		) AS t
		WHERE
			[MaxAfrMonths] < [ProjectMonths] OR
			[ProjectMonths] NOT IN(SELECT [AfrMonths] FROM #AfrCalculatingProjectItems)
	),
	[RecurciveAfr] AS 
	(
		SELECT 
			[ProjectItemId],
			[ProjectMonths] AS AfrMonths
		FROM	
			[NotExistingAfr]
		UNION ALL
		SELECT
			[RecurciveAfr].[ProjectItemId],
			[ProjectCalculator].[GetRecursiveAfrMonths]([NotExistingAfr].[MaxAfrMonths], [RecurciveAfr].[AfrMonths])
		FROM
			[RecurciveAfr], [NotExistingAfr]
		WHERE
			[RecurciveAfr].[ProjectItemId] = [NotExistingAfr].[ProjectItemId] AND
			[NotExistingAfr].[MaxAfrMonths] < [AfrMonths]
	)
	INSERT INTO @xy(ProjectItemId, X) 
	SELECT [ProjectItemId], [AfrMonths] FROM [NullAfr]
	UNION ALL
	SELECT [ProjectItemId], [AfrMonths] FROM [RecurciveAfr]

	CREATE TABLE #interpolatedAfr(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, AfrMonths AS X, Afr AS Y FROM #AfrCalculatingProjectItems',
		@xy,
		'#interpolatedAfr'

	DELETE FROM [ProjectCalculator].[Afr] WHERE [ProjectItemId] IN (SELECT [ProjectItemId] FROM #interpolatedAfr)

	INSERT INTO [ProjectCalculator].[Afr]([ProjectItemId], [AFR], [Months], [IsProlongation])
	SELECT
		[ProjectItemId],
		
		CASE 
			WHEN [AFR] < [PreviousAfr] THEN [PreviousAfr] 
			ELSE [AFR] 
		END AS [AFR],

		[Months],
		0 AS [IsProlongation]
	FROM
	(
		SELECT 
			[InterpolatedAfr].[ProjectItemId],
			[InterpolatedAfr].[X] AS [Months],
			[InterpolatedAfr].[Y] AS [AFR],
			(
				SELECT TOP 1
					[AfrCalculatingProjectItems].[AFR]
				FROM
					#AfrCalculatingProjectItems AS [AfrCalculatingProjectItems]
				WHERE
					[AfrCalculatingProjectItems].[ProjectItemId] = [InterpolatedAfr].[ProjectItemId] AND 
					[AfrCalculatingProjectItems].[AfrMonths] < [InterpolatedAfr].[X]
				ORDER BY
					[AfrCalculatingProjectItems].[AfrMonths] DESC
			) AS PreviousAfr
		FROM
			#interpolatedAfr AS [InterpolatedAfr]
	) AS t

	DROP TABLE #interpolatedAfr
	DROP TABLE #AfrCalculatingProjectItems
END
GO

CREATE PROCEDURE [ProjectCalculator].[InterpolateProjects] @projectItemIds [dbo].[ListID] READONLY
AS
BEGIN
	SELECT * INTO #CalculatingProjectItems FROM [ProjectCalculator].[GetCalculatingProjectItems](@projectItemIds)

	DECLARE @projectReactionTime [ProjectCalculator].[XY] 
	INSERT INTO @projectReactionTime(ProjectItemId, X) SELECT [Id], [ReactionTime_Minutes] FROM #CalculatingProjectItems

	DECLARE @projectAvailability [ProjectCalculator].[XY] 
	INSERT INTO @projectAvailability(ProjectItemId, X) SELECT [Id], [Availability_Value_New] FROM #CalculatingProjectItems

	DECLARE @projectReactionTimeAvailability [ProjectCalculator].[XY] 
	INSERT INTO @projectReactionTimeAvailability(ProjectItemId, X) 
	SELECT 
		[Id], 
		[ProjectCalculator].[CalcReactionTimeAvailabilityCoeff]([ReactionTime_Minutes], [Availability_Value_New]) 
	FROM 
		#CalculatingProjectItems

	DECLARE @projectReactionTimeTypeAvailability [ProjectCalculator].[XY] 
	INSERT INTO @projectReactionTimeTypeAvailability(ProjectItemId, X) 
	SELECT 
		[CalculatingProjectItems].[Id], 
		[ProjectCalculator].[CalcReactionTimeTypeAvailabilityCoeff]([ReactionTime_Minutes], [Availability_Value_New], [ReactionType].[Coeff]) 
	FROM 
		#CalculatingProjectItems AS [CalculatingProjectItems]
	INNER JOIN
		[Dependencies].[ReactionType] ON [CalculatingProjectItems].[ReactionTypeId] = [ReactionType].[Id]

	DECLARE @projectDuration [ProjectCalculator].[XY] 
	INSERT INTO @projectDuration(ProjectItemId, X) SELECT [Id], [Duration_Months] FROM #CalculatingProjectItems

	SELECT * INTO #FieldServiceReactionTimeType FROM [ProjectCalculator].[GetFieldServiceReactionTimeType](@projectItemIds)

	CREATE TABLE #PerformanceRate(ProjectItemId BIGINT, X FLOAT, Y FLOAT)	
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, PerformanceRate AS Y FROM #FieldServiceReactionTimeType',
		@projectReactionTime,
		'#PerformanceRate'

	CREATE TABLE #TimeAndMaterialShare(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, TimeAndMaterialShare AS Y FROM #FieldServiceReactionTimeType',
		@projectReactionTime,
		'#TimeAndMaterialShare'

	SELECT * INTO #LogisticsCosts FROM [ProjectCalculator].[GetLogisticsCosts](@projectItemIds)

	CREATE TABLE #ExpressDelivery(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, ExpressDelivery AS Y FROM #LogisticsCosts',
		@projectReactionTime,
		'#ExpressDelivery'

	CREATE TABLE #HighAvailabilityHandling(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, HighAvailabilityHandling AS Y FROM #LogisticsCosts',
		@projectReactionTime,
		'#HighAvailabilityHandling'

	CREATE TABLE #ReturnDeliveryFactory(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, ReturnDeliveryFactory AS Y FROM #LogisticsCosts',
		@projectReactionTime,
		'#ReturnDeliveryFactory'

	CREATE TABLE #StandardDelivery(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, StandardDelivery AS Y FROM #LogisticsCosts',
		@projectReactionTime,
		'#StandardDelivery'

	CREATE TABLE #StandardHandling(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, StandardHandling AS Y FROM #LogisticsCosts',
		@projectReactionTime,
		'#StandardHandling'

	CREATE TABLE #TaxiCourierDelivery(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, TaxiCourierDelivery AS Y FROM #LogisticsCosts',
		@projectReactionTime,
		'#TaxiCourierDelivery'

	SELECT * INTO #MarkupOtherCosts FROM [ProjectCalculator].[GetMarkupOtherCosts](@projectItemIds)

	CREATE TABLE #Markup(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, Markup AS Y FROM #MarkupOtherCosts',
		@projectReactionTimeTypeAvailability,
		'#Markup'

	CREATE TABLE #MarkupFactor(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, MarkupFactor AS Y FROM #MarkupOtherCosts',
		@projectReactionTimeTypeAvailability,
		'#MarkupFactor'

	CREATE TABLE #ProlongationMarkup(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, ProlongationMarkup AS Y FROM #MarkupOtherCosts',
		@projectReactionTimeTypeAvailability,
		'#ProlongationMarkup'

	CREATE TABLE #ProlongationMarkupFactor(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, ProlongationMarkupFactor AS Y FROM #MarkupOtherCosts',
		@projectReactionTimeTypeAvailability,
		'#ProlongationMarkupFactor'

	SELECT * INTO #ReinsuranceDuration FROM [ProjectCalculator].[GetReinsuranceDuration](@projectItemIds)
	CREATE TABLE #ReinsuranceFlatfee(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, ReinsuranceFlatfee AS Y FROM #ReinsuranceDuration',
		@projectDuration,
		'#ReinsuranceFlatfee';

	SELECT * INTO #ReinsuranceReactionTimeAvailability FROM [ProjectCalculator].[GetReinsuranceReactionTimeAvailability](@projectItemIds)
	CREATE TABLE #ReinsuranceUpliftFactor(ProjectItemId BIGINT, X FLOAT, Y FLOAT)
	EXEC [ProjectCalculator].[InterpolateY] 
		'SELECT ProjectItemId, X, ReinsuranceUpliftFactor AS Y FROM #ReinsuranceReactionTimeAvailability',
		@projectReactionTimeAvailability,
		'#ReinsuranceUpliftFactor'

	EXEC [ProjectCalculator].[InterpolateAfr] @projectItemIds;

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
			[ProjectCalculator].[GetReinsurance](@projectItemIds)
		GROUP BY
			[Wg]
	)
	UPDATE 
		[ProjectCalculator].[ProjectItem]
	SET
		[Availability_Value] = [CalculatingProjectItems].[Availability_Value_New],

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
		#CalculatingProjectItems AS [CalculatingProjectItems]
	LEFT JOIN
		[AvailabilityFeeCountryCompany] ON 
			[CalculatingProjectItems].[CountryId] = [AvailabilityFeeCountryCompany].[Country] AND
			[CalculatingProjectItems].[WgId] = [AvailabilityFeeCountryCompany].[Wg]
	LEFT JOIN
		[Hardware].[RoleCodeHourlyRates] ON 
			[CalculatingProjectItems].[CountryId] = [RoleCodeHourlyRates].[Country] AND
			[AvailabilityFeeCountryCompany].[RoleCodeId] = [RoleCodeHourlyRates].[RoleCode]
	LEFT JOIN
		[Hardware].[FieldServiceLocation] ON 
			[CalculatingProjectItems].[CountryId] = [FieldServiceLocation].[Country] AND
			[CalculatingProjectItems].[WgId] = [FieldServiceLocation].[Wg] AND
			[CalculatingProjectItems].[ServiceLocationId] = [FieldServiceLocation].[ServiceLocation]
	LEFT JOIN 
		#PerformanceRate AS [PerformanceRate] ON [PerformanceRate].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#TimeAndMaterialShare AS [TimeAndMaterialShare] ON [TimeAndMaterialShare].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		[ProjectCalculator].[GetInterpolatedOohUpliftFactor](@projectItemIds) AS [InterpolatedOohUpliftFactor] ON [InterpolatedOohUpliftFactor].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#ExpressDelivery AS [ExpressDelivery] ON [ExpressDelivery].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#HighAvailabilityHandling AS [HighAvailabilityHandling] ON [HighAvailabilityHandling].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#ReturnDeliveryFactory AS [ReturnDeliveryFactory] ON [ReturnDeliveryFactory].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#StandardDelivery AS [StandardDelivery] ON [StandardDelivery].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN 
		#StandardHandling AS [StandardHandling] ON [StandardHandling].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#TaxiCourierDelivery AS [TaxiCourierDelivery] ON [TaxiCourierDelivery].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#Markup AS [Markup] ON [Markup].[ProjectItemId] = [CalculatingProjectItems].[Id] 
	LEFT JOIN
		#MarkupFactor AS [MarkupFactor] ON [MarkupFactor].[ProjectItemId] = [CalculatingProjectItems].[Id] 
	LEFT JOIN
		#ProlongationMarkup AS [ProlongationMarkup] ON [ProlongationMarkup].[ProjectItemId] = [CalculatingProjectItems].[Id] 
	LEFT JOIN
		#ProlongationMarkupFactor AS [ProlongationMarkupFactor] ON [ProlongationMarkupFactor].[ProjectItemId] = [CalculatingProjectItems].[Id] 
	LEFT JOIN
		[Reinsurance_Currency] ON [CalculatingProjectItems].[WgId] = [Reinsurance_Currency].[Wg]
	LEFT JOIN
		#ReinsuranceFlatfee AS [ReinsuranceFlatfee] ON [ReinsuranceFlatfee].[ProjectItemId] = [CalculatingProjectItems].[Id]
	LEFT JOIN
		#ReinsuranceUpliftFactor AS [ReinsuranceUpliftFactor] ON [ReinsuranceUpliftFactor].[ProjectItemId] = [CalculatingProjectItems].[Id]
	WHERE
		[ProjectItem].[Id] = [CalculatingProjectItems].[Id] AND
		[RoleCodeHourlyRates].[Deactivated] = 0 AND
		[FieldServiceLocation].[DeactivatedDateTime] IS NULL

	DROP TABLE #CalculatingProjectItems
	DROP TABLE #FieldServiceReactionTimeType
	DROP TABLE #LogisticsCosts
	DROP TABLE #MarkupOtherCosts
	DROP TABLE #ReinsuranceDuration
	DROP TABLE #ReinsuranceReactionTimeAvailability

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
