IF OBJECT_ID('dbo.GetAvailabilityFeeCoverageCombination') IS NOT NULL
    DROP PROCEDURE dbo.GetAvailabilityFeeCoverageCombination
go

CREATE PROCEDURE [dbo].[GetAvailabilityFeeCoverageCombination]
	@pageSize int,
	@pageNumber int,
	@totalCount int OUTPUT
    AS
    BEGIN
        SET NOCOUNT ON;

		IF OBJECT_ID('tempdb..#Temp_AFR') IS NOT NULL DROP TABLE #Temp_AFR

		SELECT temp.* INTO #Temp_AFR FROM 
			(SELECT sla.CountryName, sla.CountryId, 
					sla.ReactionTimeName, sla.ReactionTimeId,
					sla.ReactionTypeName, sla.ReactionTypeId, 
					sla.ServiceLocatorName, sla.ServiceLocatorId, af.Id
				FROM
					(SELECT c.[Name] AS CountryName, c.[Id] AS CountryId, 
							rtime.[Name] AS ReactionTimeName, rtime.Id AS ReactionTimeId,
							rtype.[Name] AS ReactionTypeName, rtype.[Id] AS ReactionTypeId, 
							sl.[Name] AS ServiceLocatorName, sl.Id AS ServiceLocatorId FROM
							[InputAtoms].[Country] AS c CROSS JOIN 
							[Dependencies].[ReactionTime] AS rtime CROSS JOIN
							[Dependencies].[ReactionType] AS rtype CROSS JOIN
							[Dependencies].[ServiceLocation] AS sl) sla
							LEFT JOIN [Admin].[AvailabilityFee] af ON
						sla.CountryId = af.[CountryId] AND 
						sla.ReactionTimeId = af.[ReactionTimeId] AND
						sla.ReactionTypeId = af.[ReactionTypeId] AND
						sla.ServiceLocatorId = af.[ServiceLocationId]) AS temp

		SET @totalCount = (SELECT COUNT(*) FROM #Temp_AFR)

		SELECT temp.[CountryName], temp.[CountryId], temp.[ReactionTimeName],
			   temp.[ReactionTimeId], 
			   temp.[ReactionTypeName], temp.[ReactionTypeId],
			   temp.[ServiceLocatorName], temp.[ServiceLocatorId], temp.[Id]
		FROM (
				SELECT ROW_NUMBER() OVER (ORDER BY [CountryName]) AS RowNum, 
						[CountryName], [CountryId], [ReactionTimeName],
						[ReactionTimeId], 
						[ReactionTypeName], [ReactionTypeId],
						[ServiceLocatorName], [ServiceLocatorId], [Id]
				FROM #Temp_AFR
		) AS temp
		WHERE temp.RowNum > @pageSize * (@pageNumber - 1) AND temp.RowNum <= @pageSize * @pageNumber
			
    END
go


