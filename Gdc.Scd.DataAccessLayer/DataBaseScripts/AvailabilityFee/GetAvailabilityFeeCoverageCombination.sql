USE [Scd_2]
GO
/****** Object:  StoredProcedure [dbo].[GetAvailabilityFeeCoverageCombination]    Script Date: 16.11.2018 16:06:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetAvailabilityFeeCoverageCombination]
	@cnt int=null,
	@rtime int=null,
	@rtype int=null,
	@serloc int=null,
	@isapp bit=null,

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
					(SELECT cnt.[Name] AS CountryName, cnt.[Id] AS CountryId, 
							rtime.[Name] AS ReactionTimeName, rtime.Id AS ReactionTimeId,
							rtype.[Name] AS ReactionTypeName, rtype.[Id] AS ReactionTypeId, 
							sl.[Name] AS ServiceLocatorName, sl.Id AS ServiceLocatorId 
					 FROM
							[InputAtoms].[Country] AS cnt 
							CROSS JOIN (select * from [Dependencies].[ReactionTime] where (@rtime is null or Id=@rtime)) AS rtime 
							CROSS JOIN (select * from [Dependencies].[ReactionType] where (@rtype is null or Id=@rtype)) AS rtype 
							CROSS JOIN (select * from [Dependencies].[ServiceLocation] where (@serloc is null or Id=@serloc)) AS sl) sla
							LEFT JOIN [Admin].[AvailabilityFee] af ON
								sla.CountryId = af.[CountryId] AND 
								sla.ReactionTimeId = af.[ReactionTimeId] AND
								sla.ReactionTypeId = af.[ReactionTypeId] AND
								sla.ServiceLocatorId = af.[ServiceLocationId] 
							where (@cnt is null or sla.CountryId=@cnt) and (@isapp is null or (@isapp=0 and af.Id is null) or (@isapp=1 and af.Id is not null))) AS temp

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