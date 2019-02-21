USE [Scd_2]

SELECT * INTO [Hardware].[MarkupStandardWaranty_MigrationBackup] FROM [Hardware].[MarkupStandardWaranty]

TRUNCATE TABLE [Hardware].[MarkupStandardWaranty]

DROP INDEX [ix_Atom_MarkupStandardWaranty] ON [Hardware].[MarkupStandardWaranty]

CREATE NONCLUSTERED INDEX [ix_Atom_MarkupStandardWaranty]
ON [Hardware].[MarkupStandardWaranty] ([Country],[Wg])
INCLUDE ([MarkupFactorStandardWarranty],[MarkupStandardWarranty])

ALTER TABLE [Hardware].[MarkupStandardWaranty] DROP CONSTRAINT [FK_HardwareMarkupStandardWarantyReactionTimeTypeAvailability_DependenciesReactionTime_ReactionType_Avalability]
ALTER TABLE [Hardware].[MarkupStandardWaranty] DROP COLUMN [ReactionTimeTypeAvailability]

INSERT INTO [Hardware].[MarkupStandardWaranty] 
(
	[Country], 
	[Pla], 
	[CentralContractGroup], 
	[Wg], 
	[MarkupFactorStandardWarranty], 
	[MarkupFactorStandardWarranty_Approved], 
	[MarkupStandardWarranty], 
	[MarkupStandardWarranty_Approved],
	[CreatedDateTime],
	[DeactivatedDateTime]
)
SELECT 
	[Country],
	[Pla],
	[CentralContractGroup],
	[Wg],
	(
		IIF(
			[MarkupFactorStandardWarranty_Count] > 1, 
			IIF([MarkupFactorStandardWarranty_Approved_Count] = 1, [MarkupFactorStandardWarranty_Approved], NULL), 
			[MarkupFactorStandardWarranty])
	)	
	AS [MarkupFactorStandardWarranty],
	IIF([MarkupFactorStandardWarranty_Approved_Count] > 1, NULL, [MarkupFactorStandardWarranty_Approved]) AS [MarkupFactorStandardWarranty_Approved],
	(
		IIF(
			[MarkupStandardWarranty_Count] > 1, 
			IIF([MarkupStandardWarranty_Approved_Count]	= 1, [MarkupStandardWarranty_Approved], NULL), 
			[MarkupStandardWarranty])
	) 
	AS [MarkupStandardWarranty],
	IIF([MarkupStandardWarranty_Approved_Count]	> 1, NULL, [MarkupStandardWarranty_Approved]) AS [MarkupStandardWarranty_Approved],
	[CreatedDateTime],
	[DeactivatedDateTime]
FROM
(
	SELECT 
		[Country],
		[Pla],
		[CentralContractGroup],
		[Wg],
		MAX([MarkupFactorStandardWarranty])					    AS [MarkupFactorStandardWarranty],
		MAX([MarkupStandardWarranty])						    AS [MarkupStandardWarranty],
		MAX([MarkupFactorStandardWarranty_Approved])		    AS [MarkupFactorStandardWarranty_Approved],
		MAX([MarkupStandardWarranty_Approved])				    AS [MarkupStandardWarranty_Approved],
		COUNT(DISTINCT [MarkupFactorStandardWarranty])		    AS [MarkupFactorStandardWarranty_Count],
		COUNT(DISTINCT [MarkupStandardWarranty])				AS [MarkupStandardWarranty_Count],
		COUNT(DISTINCT [MarkupFactorStandardWarranty_Approved]) AS [MarkupFactorStandardWarranty_Approved_Count],
		COUNT(DISTINCT [MarkupStandardWarranty_Approved])	    AS [MarkupStandardWarranty_Approved_Count],
		MIN([CreatedDateTime])									AS [CreatedDateTime],
		MAX([DeactivatedDateTime])								AS [DeactivatedDateTime]
	FROM 
		[Hardware].[MarkupStandardWaranty_MigrationBackup]
	GROUP BY
		[Country],
		[Pla],
		[CentralContractGroup],
		[Wg]
) AS t


SELECT
	*
INTO 
	[History].[CostBlockHistory_MigrationBackup]
FROM
	[History].[CostBlockHistory]
WHERE
	[CostBlockHistory].[Context_ApplicationId] = 'Hardware' AND
	[CostBlockHistory].[Context_CostBlockId] = 'MarkupStandardWaranty' AND
	(
		SELECT 
			COUNT(*)
		FROM
		(
			SELECT 
				[Id] 
			FROM 
				[Dependencies].[ReactionTimeTypeAvailability] 
			WHERE 
				[IsDisabled] = 0
			EXCEPT
			SELECT 
				[ReactionTimeTypeAvailability] 
			FROM 
				[History_RelatedItems].[ReactionTimeTypeAvailability] 
			WHERE 
				[ReactionTimeTypeAvailability].[CostBlockHistory] = [CostBlockHistory].[Id]
		) AS t
	) > 1

SELECT * INTO [History].[Hardware_MarkupStandardWaranty_MigrationBackup]			FROM [History].[Hardware_MarkupStandardWaranty]			   WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup]) 
SELECT * INTO [History_RelatedItems].[Country_MigrationBackup]						FROM [History_RelatedItems].[Country]					   WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup]) 
SELECT * INTO [History_RelatedItems].[Pla_MigrationBackup]							FROM [History_RelatedItems].[Pla]						   WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup]) 
SELECT * INTO [History_RelatedItems].[CentralContractGroup_MigrationBackup]			FROM [History_RelatedItems].[CentralContractGroup]		   WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup]) 
SELECT * INTO [History_RelatedItems].[Wg_MigrationBackup]						    FROM [History_RelatedItems].[Wg]						   WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup]) 
SELECT * INTO [History_RelatedItems].[ReactionTimeTypeAvailability_MigrationBackup] FROM [History_RelatedItems].[ReactionTimeTypeAvailability] WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup]) 

DELETE FROM [History].[Hardware_MarkupStandardWaranty]			  WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup])
DELETE FROM [History_RelatedItems].[Country]					  WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup])
DELETE FROM [History_RelatedItems].[Pla]						  WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup])
DELETE FROM [History_RelatedItems].[CentralContractGroup]		  WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup])
DELETE FROM [History_RelatedItems].[Wg]							  WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup])
DELETE FROM [History_RelatedItems].[ReactionTimeTypeAvailability] WHERE [CostBlockHistory] IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup])
DELETE FROM [History].[CostBlockHistory]						  WHERE [Id]			   IN (SELECT [Id] FROM [History].[CostBlockHistory_MigrationBackup])

ALTER TABLE [History].[Hardware_MarkupStandardWaranty] DROP COLUMN [ReactionTimeTypeAvailability]