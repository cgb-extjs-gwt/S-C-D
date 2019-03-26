--BACKUP OLD TABLES
sp_rename 'Hardware.MaterialCostWarranty', 'MaterialCostWarranty_Back'
GO
sp_rename 'History.Hardware_MaterialCostWarranty', 'Hardware_MaterialCostWarranty_Back'
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty_Back] DROP CONSTRAINT [PK_History_Hardware_MaterialCostWarranty_Id]
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty_Back] DROP CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyCostBlockHistory_HistoryCostBlockHistory]
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty_Back] DROP CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyWg_InputAtomsWg]
GO

--DROP OLD CALC TABLE
DROP TABLE [Hardware].[MaterialCostOowCalc]
GO


--CREATE MATERIAL COST TABLE FOR IN AND OUT WARRANTY (NON EMEIA)
CREATE TABLE [Hardware].[MaterialCostWarranty](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Wg] [bigint] NOT NULL,
	[NonEmeiaCountry] [bigint] NOT NULL,
	[MaterialCostOow] [float] NULL,
	[MaterialCostIw] [float] NULL,
	[MaterialCostOow_Approved] [float] NULL,
	[MaterialCostIw_Approved] [float] NULL,
	[CreatedDateTime] [datetime] NOT NULL,
	[DeactivatedDateTime] [datetime] NULL,
 CONSTRAINT [PK_Hardware_MaterialCostWarranty_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Hardware].[MaterialCostWarranty] ADD  DEFAULT (getutcdate()) FOR [CreatedDateTime]
GO

ALTER TABLE [Hardware].[MaterialCostWarranty]  WITH CHECK ADD  CONSTRAINT [FK_HardwareMaterialCostWarrantyNonEmeiaCountry_InputAtomsCountry] FOREIGN KEY([NonEmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostWarranty] CHECK CONSTRAINT [FK_HardwareMaterialCostWarrantyNonEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [Hardware].[MaterialCostWarranty]  WITH CHECK ADD  CONSTRAINT [FK_HardwareMaterialCostWarrantyWg_InputAtomsWg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostWarranty] CHECK CONSTRAINT [FK_HardwareMaterialCostWarrantyWg_InputAtomsWg]
GO

--CREATE TABLE FOR MATERIAL COST CALCULATION
CREATE TABLE [Hardware].[MaterialCostWarrantyCalc](
	[Country] [bigint] NOT NULL,
	[Wg] [bigint] NOT NULL,
	[MaterialCostOow] [float] NULL,
	[MaterialCostOow_Approved] [float] NULL,
	[MaterialCostIw] [float] NULL,
	[MaterialCostIw_Approved] [float] NULL,
 CONSTRAINT [PK_MaterialCostOowCalc] PRIMARY KEY NONCLUSTERED 
(
	[Country] ASC,
	[Wg] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyCalc]  WITH NOCHECK ADD FOREIGN KEY([Country])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyCalc]  WITH NOCHECK ADD FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

--CREATE MATERIAL COST TABLE FOR IN AND OUT WARRANTY (EMEIA)
CREATE TABLE [Hardware].[MaterialCostWarrantyEmeia](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedDateTime] [datetime2](7) NOT NULL,
	[DeactivatedDateTime] [datetime2](7) NULL,
	[MaterialCostIw] [float] NULL,
	[MaterialCostIw_Approved] [float] NULL,
	[MaterialCostOow] [float] NULL,
	[MaterialCostOow_Approved] [float] NULL,
	[ModifiedDateTime] [datetime2](7) NOT NULL,
	[Wg] [bigint] NULL,
 CONSTRAINT [PK_MaterialCostWarrantyEmeia] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyEmeia] ADD  DEFAULT (getutcdate()) FOR [CreatedDateTime]
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyEmeia] ADD  DEFAULT (getutcdate()) FOR [ModifiedDateTime]
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyEmeia]  WITH CHECK ADD  CONSTRAINT [FK_MaterialCostWarrantyEmeia_Wg_Wg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyEmeia] CHECK CONSTRAINT [FK_MaterialCostWarrantyEmeia_Wg_Wg]
GO

--CREATE HISTORY MATERIAL COST TABLE FOR IN AND OUT WARRANTY (NON EMEIA)
CREATE TABLE [History].[Hardware_MaterialCostWarranty](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Wg] [bigint] NULL,
	[NonEmeiaCountry] [bigint] NULL,
	[MaterialCostOow] [float] NULL,
	[MaterialCostIw] [float] NULL,
	[CostBlockHistory] [bigint] NOT NULL,
 CONSTRAINT [PK_History_Hardware_MaterialCostWarranty_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY([CostBlockHistory])
REFERENCES [History].[CostBlockHistory] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyCostBlockHistory_HistoryCostBlockHistory]
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyNonEmeiaCountry_InputAtomsCountry] FOREIGN KEY([NonEmeiaCountry])
REFERENCES [InputAtoms].[Country] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyNonEmeiaCountry_InputAtomsCountry]
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyWg_InputAtomsWg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostWarranty] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyWg_InputAtomsWg]
GO

--CREATE HISTORY MATERIAL COST TABLE FOR IN AND OUT WARRANTY (EMEIA)
CREATE TABLE [History].[Hardware_MaterialCostWarrantyEmeia](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Wg] [bigint] NULL,
	[MaterialCostOow] [float] NULL,
	[MaterialCostIw] [float] NULL,
	[CostBlockHistory] [bigint] NOT NULL,
 CONSTRAINT [PK_History_Hardware_MaterialCostWarrantyEmeia_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [History].[Hardware_MaterialCostWarrantyEmeia]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyEmeiaCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY([CostBlockHistory])
REFERENCES [History].[CostBlockHistory] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostWarrantyEmeia] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyEmeiaCostBlockHistory_HistoryCostBlockHistory]
GO

ALTER TABLE [History].[Hardware_MaterialCostWarrantyEmeia]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyEmeiaWg_InputAtomsWg] FOREIGN KEY([Wg])
REFERENCES [InputAtoms].[Wg] ([Id])
GO

ALTER TABLE [History].[Hardware_MaterialCostWarrantyEmeia] CHECK CONSTRAINT [FK_HistoryHardware_MaterialCostWarrantyEmeiaWg_InputAtomsWg]
GO

--DROP OLD TRIGGERS
DROP TRIGGER [Hardware].[MaterialCostOowUpdated]
GO

DROP TRIGGER [Hardware].[MaterialCostOowEmeiaUpdated]
GO

--DROP OLD PROCEDURE
DROP PROCEDURE [Hardware].[SpUpdateMaterialCostOowCalc]
GO

--CREATE NEW CALC PROCEDURE
CREATE PROCEDURE [Hardware].[SpUpdateMaterialCostCalc]
AS
BEGIN

    SET NOCOUNT ON;

    truncate table Hardware.MaterialCostWarrantyCalc;

    -- Disable all table constraints
    ALTER TABLE Hardware.MaterialCostWarrantyCalc NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.MaterialCostWarrantyCalc(Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved)
        select NonEmeiaCountry as Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved
        from Hardware.MaterialCostWarranty
        where DeactivatedDateTime is null

        union 

        SELECT cr.Id AS Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved 
		  FROM [Hardware].[MaterialCostWarrantyEmeia] mc
		  CROSS JOIN (SELECT c.[Id]
		  FROM [InputAtoms].[Country] c
		  INNER JOIN [InputAtoms].[CountryGroup] cg
		  ON c.CountryGroupId = cg.Id
		  INNER JOIN [InputAtoms].[Region] r
		  ON cg.RegionId = r.Id
		  INNER JOIN [InputAtoms].[ClusterRegion] cr
		  ON r.ClusterRegionId = cr.Id
		  WHERE cr.IsEmeia = 1 AND c.IsMaster = 1) AS cr
		  where DeactivatedDateTime is null

    -- Enable all table constraints
    ALTER TABLE Hardware.MaterialCostWarrantyCalc CHECK CONSTRAINT ALL;

END
GO

--CREATE NEW TRIGGERS FOR NON EMEIA
CREATE TRIGGER [Hardware].[MaterialCostUpdated]
ON [Hardware].[MaterialCostWarranty]
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostCalc;
END
GO

ALTER TABLE [Hardware].[MaterialCostWarranty] ENABLE TRIGGER [MaterialCostUpdated]
GO

--CREATE NEW TRIGGERS FOR EMEIA
CREATE TRIGGER [Hardware].[MaterialCostWarrantyEmeiaUpdated]
ON [Hardware].[MaterialCostWarrantyEmeia]
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostCalc;
END
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyEmeia] ENABLE TRIGGER [MaterialCostWarrantyEmeiaUpdated]
GO

--FILL TABLE MATERIAL COST EMEIA
SELECT  [Wg].[Id] AS [Wg] INTO [#Coordinates] FROM (SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = 1 AND [IsSoftware] = 0) AS [Wg];
INSERT INTO [Hardware].[MaterialCostWarrantyEmeia] ([Wg])
SELECT  [Wg] FROM [#Coordinates]
EXCEPT
SELECT  [Wg] FROM [Hardware].[MaterialCostWarrantyEmeia] WHERE [DeactivatedDateTime] IS NULL;
UPDATE [Hardware].[MaterialCostWarrantyEmeia] SET [DeactivatedDateTime] = '2018-03-22 13:06:50.760' FROM (SELECT  [Wg] FROM [Hardware].[MaterialCostWarrantyEmeia] WHERE [DeactivatedDateTime] IS NULL
EXCEPT
SELECT  [Wg] FROM [#Coordinates]) AS [DelectedCoordinate] WHERE [DelectedCoordinate].[Wg] = [MaterialCostWarrantyEmeia].[Wg];
DROP TABLE [#Coordinates]
GO

--FILL TABLE MATERIAL COST NON EMEIA
SELECT  [Wg].[Id] AS [Wg], [NonEmeiaCountry].[Id] AS [NonEmeiaCountry] INTO [#Coordinates] FROM (SELECT  * FROM [InputAtoms].[Wg] WHERE [DeactivatedDateTime] IS NULL AND [WgType] = 1 AND [IsSoftware] = 0) AS [Wg] CROSS JOIN (SELECT  * FROM [InputAtoms].[NonEmeiaCountry] WHERE [IsMaster] = 1) AS [NonEmeiaCountry];
INSERT INTO [Hardware].[MaterialCostWarranty] ([Wg], [NonEmeiaCountry])
SELECT  [Wg], [NonEmeiaCountry] FROM [#Coordinates]
EXCEPT
SELECT  [Wg], [NonEmeiaCountry] FROM [Hardware].[MaterialCostWarranty] WHERE [DeactivatedDateTime] IS NULL;
UPDATE [Hardware].[MaterialCostWarranty] SET [DeactivatedDateTime] = '2018-03-22 13:06:50.760' FROM (SELECT  [Wg], [NonEmeiaCountry] FROM [Hardware].[MaterialCostWarranty] WHERE [DeactivatedDateTime] IS NULL
EXCEPT
SELECT  [Wg], [NonEmeiaCountry] FROM [#Coordinates]) AS [DelectedCoordinate] WHERE [DelectedCoordinate].[Wg] = [MaterialCostWarranty].[Wg] AND [DelectedCoordinate].[NonEmeiaCountry] = [MaterialCostWarranty].[NonEmeiaCountry];
DROP TABLE [#Coordinates]
GO

--MOVE DATA FROM MATERIAL COST OOW NON EMEIA TABLE 
UPDATE mc
SET mc.[MaterialCostOow] = oow.MaterialCostOow, 
mc.[MaterialCostOow_Approved] = oow.MaterialCostOow_Approved, 
mc.DeactivatedDateTime = oow.DeactivatedDateTime,
mc.CreatedDateTime = oow.CreatedDateTime
FROM  [Hardware].[MaterialCostWarranty] mc
INNER JOIN [Hardware].[MaterialCostOow] oow ON
oow.NonEmeiaCountry = mc.[NonEmeiaCountry] AND oow.Wg = mc.Wg
GO

--MOVE DATA FROM MATERIAL COST OOW EMEIA
UPDATE mc
SET mc.MaterialCostOow = oow.MaterialCostOow,
mc.CreatedDateTime = oow.CreatedDateTime,
mc.[MaterialCostOow_Approved] = oow.MaterialCostOow_Approved, 
mc.DeactivatedDateTime = oow.DeactivatedDateTime
FROM [Hardware].[MaterialCostWarrantyEmeia] mc
INNER JOIN [Hardware].[MaterialCostOowEmeia] oow ON
oow.Wg = mc.Wg
GO

--MOVE DATA FROM MATERIAL COST IW EMEIA
UPDATE mc
  SET mc.[MaterialCostIw] = iw.[MaterialCostWarranty],
  mc.[MaterialCostIw_Approved] = iw.[MaterialCostWarranty_Approved],
  mc.[DeactivatedDateTime] = iw.[DeactivatedDateTime],
  mc.CreatedDateTime = iw.[CreatedDateTime]
  FROM [Hardware].[MaterialCostWarrantyEmeia] mc
  INNER JOIN [Hardware].[MaterialCostWarranty_Back] iw ON
  mc.Wg = iw.Wg
  WHERE iw.[ClusterRegion] = 2
GO

--MOVE HISTORY FOR MATERIAL COST OOW NON EMEIA
INSERT INTO [History].[Hardware_MaterialCostWarranty] ([Wg], [NonEmeiaCountry], [MaterialCostOow], [CostBlockHistory])
SELECT [Wg], [NonEmeiaCountry], [MaterialCostOow], [CostBlockHistory]
FROM [SCD_2].[History].[Hardware_MaterialCostOow]
GO

--MOVE HISTORY FOR MATERIAL COST OOW EMEIA
INSERT INTO [History].[Hardware_MaterialCostWarrantyEmeia] ([Wg], [MaterialCostOow], [CostBlockHistory])
SELECT [Wg], [MaterialCostOow], [CostBlockHistory]
FROM [History].[Hardware_MaterialCostOowEmeia]
GO

--MOVE HISTORY FOR MATERIAL COST IW EMEIA
INSERT INTO [History].[Hardware_MaterialCostWarrantyEmeia] ([Wg], [MaterialCostIw], [CostBlockHistory])
SELECT [Wg], [MaterialCostWarranty], [CostBlockHistory]
FROM [History].[Hardware_MaterialCostWarranty_Back]
GO

--CLEAR RELATED ITEMS CLUSTER REGION
DELETE FROM [History_RelatedItems].[ClusterRegion]
WHERE CostBlockHistory IN (SELECT CostBlockHistory FROM [History].[Hardware_MaterialCostWarranty_Back])
GO

--UPDATE COST BLOCK HISTORY TABLE
UPDATE [History].[CostBlockHistory]
SET [Context_CostBlockId]='MaterialCostWarrantyEmeia', [Context_CostElementId] = 'MaterialCostIw'
WHERE Id IN (SELECT CostBlockHistory FROM [History].[Hardware_MaterialCostWarranty_Back]) 
GO

UPDATE [History].[CostBlockHistory]
SET [Context_CostBlockId]='MaterialCostWarrantyEmeia'
WHERE Id IN (SELECT CostBlockHistory FROM [History].[Hardware_MaterialCostOowEmeia])
GO

UPDATE [History].[CostBlockHistory]
SET [Context_CostBlockId]='MaterialCostWarranty'
WHERE Id IN (SELECT CostBlockHistory FROM [History].[Hardware_MaterialCostOow])
GO