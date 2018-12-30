USE Scd_2

DECLARE @wgType INT = 1
DECLARE @isSoftware BIT = 0

SELECT  
	[Wg].[Id] AS [Wg], 
	[Year].[Id] AS [Year], 
	[ReactionTimeAvailability].[Id] AS [ReactionTimeAvailability] 
INTO 
	[#Coordinates] 
FROM 
(
	SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware
) 
AS [Wg] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[Year]) AS [Year] 
CROSS JOIN 
	(SELECT  * FROM [Dependencies].[ReactionTimeAvailability]) AS [ReactionTimeAvailability];

INSERT INTO [Hardware].[Reinsurance] ([Wg], [Year], [ReactionTimeAvailability])
SELECT  [Wg], [Year], [ReactionTimeAvailability] FROM [#Coordinates]
EXCEPT
SELECT  [Wg], [Year], [ReactionTimeAvailability] FROM [Hardware].[Reinsurance] WHERE [DeactivatedDateTime] IS NULL;

DROP TABLE [#Coordinates]