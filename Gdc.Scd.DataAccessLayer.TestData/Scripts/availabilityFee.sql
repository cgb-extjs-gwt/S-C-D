IF OBJECT_ID('dbo.GetAvailabilityFeeCoverageCombination') IS NOT NULL
    DROP PROCEDURE dbo.GetAvailabilityFeeCoverageCombination
go

CREATE PROCEDURE [dbo].[GetAvailabilityFeeCoverageCombination]

    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT sla.CountryName, sla.CountryId, 
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
                    sla.ServiceLocatorId = af.[ServiceLocationId]
    END

go


