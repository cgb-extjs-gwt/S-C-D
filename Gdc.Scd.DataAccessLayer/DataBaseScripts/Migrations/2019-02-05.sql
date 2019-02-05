USE [Scd_2]
GO

SELECT 
	[Id] 
INTO 
	#DeletedHistory
FROM
	[History].[CostBlockHistory] 
WHERE 
	[Context_ApplicationId] = 'Hardware' AND [Context_CostBlockId] = 'MaterialCostOow'

DELETE [History_RelatedItems].[Wg] WHERE [CostBlockHistory] IN (SELECT [Id] FROM  #DeletedHistory)
DELETE [History_RelatedItems].[ClusterRegion] WHERE [CostBlockHistory] IN (SELECT [Id] FROM  #DeletedHistory)
DELETE [History].[CostBlockHistory] WHERE [Id] IN (SELECT [Id] FROM  #DeletedHistory)

DROP TABLE [#DeletedHistory]
DROP TABLE [Hardware].[MaterialCostOow]
DROP TABLE [History].[Hardware_MaterialCostOow]
GO

ALTER TABLE [InputAtoms].[ClusterRegion] ADD [IsEmeia] bit NOT NULL DEFAULT (0)
GO

UPDATE [InputAtoms].[ClusterRegion] SET [IsEmeia] = 1 WHERE [Name] = 'EMEIA'
GO

CREATE VIEW [InputAtoms].[EmeiaCountry] AS
SELECT  
	[Country].[QualityGateGroup], 
	[Country].[ClusterRegionId], 
	[Country].[IsMaster], 
	[Country].[Id], 
	[Country].[Name] 
FROM 
	[InputAtoms].[Country] 
INNER JOIN 
	[InputAtoms].[ClusterRegion] ON [Country].[ClusterRegionId] = [ClusterRegion].[Id] 
WHERE 
	[ClusterRegion].[IsEmeia] = 1
GO

CREATE VIEW [InputAtoms].[NonEmeiaCountry] AS
SELECT  
	[Country].[QualityGateGroup], 
	[Country].[ClusterRegionId], 
	[Country].[IsMaster], 
	[Country].[Id], 
	[Country].[Name] 
FROM 
	[InputAtoms].[Country] 
INNER JOIN 
	[InputAtoms].[ClusterRegion] ON [Country].[ClusterRegionId] = [ClusterRegion].[Id] 
WHERE 
	[ClusterRegion].[IsEmeia] = 0
GO

CREATE TABLE [Hardware].[MaterialCostOow]
(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Wg] [bigint] NOT NULL,
	[NonEmeiaCountry] [bigint] NOT NULL,
	[MaterialCostOow] [float] NULL,
	[MaterialCostOow_Approved] [float] NULL,
	[CreatedDateTime] [datetime] NOT NULL,
	[DeactivatedDateTime] [datetime] NULL,
	CONSTRAINT [PK_Hardware_MaterialCostOow_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [Hardware].[MaterialCostOow] ADD  DEFAULT (getutcdate()) FOR [CreatedDateTime]
GO

ALTER TABLE [Hardware].[MaterialCostOow]  WITH CHECK ADD  CONSTRAINT [FK_HardwareMaterialCostOowNonEmeiaCountry_InputAtomsCountry] FOREIGN KEY([NonEmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostOow] CHECK CONSTRAINT [FK_HardwareMaterialCostOowNonEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [Hardware].[MaterialCostOow]  WITH CHECK ADD  CONSTRAINT [FK_HardwareMaterialCostOowWg_InputAtomsWg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostOow] CHECK CONSTRAINT [FK_HardwareMaterialCostOowWg_InputAtomsWg]
GO

CREATE TABLE [History].[Hardware_MaterialCostOow]
(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Wg] [bigint] NULL,
	[NonEmeiaCountry] [bigint] NULL,
	[MaterialCostOow] [float] NULL,
	[CostBlockHistory] [bigint] NOT NULL,
	CONSTRAINT [PK_History_Hardware_MaterialCostOow_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [History].[Hardware_MaterialCostOow]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostOowCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY([CostBlockHistory])
REFERENCES [History].[CostBlockHistory] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostOow] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostOowCostBlockHistory_HistoryCostBlockHistory]
GO

ALTER TABLE [History].[Hardware_MaterialCostOow]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostOowNonEmeiaCountry_InputAtomsCountry] FOREIGN KEY([NonEmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostOow] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostOowNonEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [History].[Hardware_MaterialCostOow]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostOowWg_InputAtomsWg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostOow] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostOowWg_InputAtomsWg]
GO

CREATE TABLE [History_RelatedItems].[NonEmeiaCountry]
(
	[CostBlockHistory] [bigint] NOT NULL,
	[NonEmeiaCountry] [bigint] NULL
) 
ON [PRIMARY]
GO

ALTER TABLE [History_RelatedItems].[NonEmeiaCountry]  WITH CHECK ADD  CONSTRAINT [FK_History_RelatedItemsNonEmeiaCountryCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY([CostBlockHistory])
REFERENCES [History].[CostBlockHistory] ([Id])
GO

ALTER TABLE [History_RelatedItems].[NonEmeiaCountry] CHECK CONSTRAINT [FK_History_RelatedItemsNonEmeiaCountryCostBlockHistory_HistoryCostBlockHistory]
GO

ALTER TABLE [History_RelatedItems].[NonEmeiaCountry]  WITH CHECK ADD  CONSTRAINT [FK_History_RelatedItemsNonEmeiaCountryNonEmeiaCountry_InputAtomsCountry] FOREIGN KEY([NonEmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [History_RelatedItems].[NonEmeiaCountry] CHECK CONSTRAINT [FK_History_RelatedItemsNonEmeiaCountryNonEmeiaCountry_InputAtomsCountry]
GO

CREATE TABLE [Hardware].[MaterialCostOowEmeia]
(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Wg] [bigint] NOT NULL,
	[EmeiaCountry] [bigint] NOT NULL,
	[MaterialCostOow] [float] NULL,
	[MaterialCostOow_Approved] [float] NULL,
	[CreatedDateTime] [datetime] NOT NULL,
	[DeactivatedDateTime] [datetime] NULL,
	CONSTRAINT [PK_Hardware_MaterialCostOowEmeia_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [Hardware].[MaterialCostOowEmeia] ADD  DEFAULT (getutcdate()) FOR [CreatedDateTime]
GO

ALTER TABLE [Hardware].[MaterialCostOowEmeia]  WITH CHECK ADD  CONSTRAINT [FK_HardwareMaterialCostOowEmeiaEmeiaCountry_InputAtomsCountry] FOREIGN KEY([EmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostOowEmeia] CHECK CONSTRAINT [FK_HardwareMaterialCostOowEmeiaEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [Hardware].[MaterialCostOowEmeia]  WITH CHECK ADD  CONSTRAINT [FK_HardwareMaterialCostOowEmeiaWg_InputAtomsWg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostOowEmeia] CHECK CONSTRAINT [FK_HardwareMaterialCostOowEmeiaWg_InputAtomsWg]
GO

CREATE TABLE [History].[Hardware_MaterialCostOowEmeia]
(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Wg] [bigint] NULL,
	[EmeiaCountry] [bigint] NULL,
	[MaterialCostOow] [float] NULL,
	[CostBlockHistory] [bigint] NOT NULL,
	CONSTRAINT [PK_History_Hardware_MaterialCostOowEmeia_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostOowEmeiaCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY([CostBlockHistory])
REFERENCES [History].[CostBlockHistory] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostOowEmeiaCostBlockHistory_HistoryCostBlockHistory]
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostOowEmeiaEmeiaCountry_InputAtomsCountry] FOREIGN KEY([EmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostOowEmeiaEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostOowEmeiaWg_InputAtomsWg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostOowEmeia] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostOowEmeiaWg_InputAtomsWg]
GO

CREATE TABLE [History_RelatedItems].[EmeiaCountry]
(
	[CostBlockHistory] [bigint] NOT NULL,
	[EmeiaCountry] [bigint] NULL
) 
ON [PRIMARY]
GO

ALTER TABLE [History_RelatedItems].[EmeiaCountry]  WITH CHECK ADD  CONSTRAINT [FK_History_RelatedItemsEmeiaCountryCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY([CostBlockHistory])
REFERENCES [History].[CostBlockHistory] ([Id])
GO

ALTER TABLE [History_RelatedItems].[EmeiaCountry] CHECK CONSTRAINT [FK_History_RelatedItemsEmeiaCountryCostBlockHistory_HistoryCostBlockHistory]
GO

ALTER TABLE [History_RelatedItems].[EmeiaCountry]  WITH CHECK ADD  CONSTRAINT [FK_History_RelatedItemsEmeiaCountryEmeiaCountry_InputAtomsCountry] FOREIGN KEY([EmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [History_RelatedItems].[EmeiaCountry] CHECK CONSTRAINT [FK_History_RelatedItemsEmeiaCountryEmeiaCountry_InputAtomsCountry]
GO

DECLARE @wgType INT = 1
DECLARE @isSoftware BIT = 0

SELECT  
	[Wg].[Id] AS [Wg], 
	[EmeiaCountry].[Id] AS [EmeiaCountry] 
INTO 
	[#EmeiaCoordinates] 
FROM 
	(SELECT * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware) AS [Wg] 
CROSS JOIN 
	(SELECT * FROM [InputAtoms].[EmeiaCountry]) AS [EmeiaCountry]

INSERT INTO [Hardware].[MaterialCostOowEmeia] ([Wg], [EmeiaCountry])
SELECT  [Wg], [EmeiaCountry] FROM [#EmeiaCoordinates]
EXCEPT
SELECT  [Wg], [EmeiaCountry] FROM [Hardware].[MaterialCostOowEmeia] WHERE [DeactivatedDateTime] IS NULL

DROP TABLE [#EmeiaCoordinates] 
               
SELECT  
	[Wg].[Id] AS [Wg], 
	[NonEmeiaCountry].[Id] AS [NonEmeiaCountry] 
INTO 
	[#NonEmeiaCoordinates] 
FROM 
	(SELECT * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = @wgType AND [IsSoftware] = @isSoftware) AS [Wg] 
CROSS JOIN 
	(SELECT * FROM [InputAtoms].[NonEmeiaCountry]) AS [NonEmeiaCountry]

INSERT INTO [Hardware].[MaterialCostOow] ([Wg], [NonEmeiaCountry])
SELECT  [Wg], [NonEmeiaCountry] FROM [#NonEmeiaCoordinates]
EXCEPT
SELECT  [Wg], [NonEmeiaCountry] FROM [Hardware].[MaterialCostOow] WHERE [DeactivatedDateTime] IS NULL

DROP TABLE [#NonEmeiaCoordinates] 