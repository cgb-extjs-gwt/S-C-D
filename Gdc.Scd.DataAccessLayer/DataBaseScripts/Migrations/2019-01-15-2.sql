<<<<<<< HEAD
/*
The column [Hardware].[InstallBase].[CentralContractGroup] is being dropped, data loss could occur.

The column [Hardware].[InstallBase].[Pla] is being dropped, data loss could occur.
*/

IF EXISTS (select top 1 1 from [Hardware].[InstallBase])
    RAISERROR (N'Rows were detected. The schema update is terminating because data loss might occur.', 16, 127) WITH NOWAIT

GO
/*
The column [History].[Hardware_InstallBase].[CentralContractGroup] is being dropped, data loss could occur.

The column [History].[Hardware_InstallBase].[Pla] is being dropped, data loss could occur.
*/

IF EXISTS (select top 1 1 from [History].[Hardware_InstallBase])
    RAISERROR (N'Rows were detected. The schema update is terminating because data loss might occur.', 16, 127) WITH NOWAIT

GO

PRINT N'Dropping [Hardware].[InstallBase].[IX_InstallBase_CentralContractGroup]...';


GO
DROP INDEX [IX_InstallBase_CentralContractGroup]
    ON [Hardware].[InstallBase];


GO
PRINT N'Dropping [Hardware].[InstallBase].[IX_InstallBase_Pla]...';


GO
DROP INDEX [IX_InstallBase_Pla]
    ON [Hardware].[InstallBase];


GO
PRINT N'Dropping [History].[FK_HistoryHardware_InstallBaseCountry_InputAtomsCountry]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] DROP CONSTRAINT [FK_HistoryHardware_InstallBaseCountry_InputAtomsCountry];


GO
PRINT N'Dropping [History].[FK_HistoryHardware_InstallBasePla_InputAtomsPla]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] DROP CONSTRAINT [FK_HistoryHardware_InstallBasePla_InputAtomsPla];


GO
PRINT N'Dropping [History].[FK_HistoryHardware_InstallBaseCentralContractGroup_InputAtomsCentralContractGroup]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] DROP CONSTRAINT [FK_HistoryHardware_InstallBaseCentralContractGroup_InputAtomsCentralContractGroup];


GO
PRINT N'Dropping [History].[FK_HistoryHardware_InstallBaseWg_InputAtomsWg]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] DROP CONSTRAINT [FK_HistoryHardware_InstallBaseWg_InputAtomsWg];


GO
PRINT N'Dropping [History].[FK_HistoryHardware_InstallBaseCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] DROP CONSTRAINT [FK_HistoryHardware_InstallBaseCostBlockHistory_HistoryCostBlockHistory];


GO
PRINT N'Dropping [Hardware].[FK_InstallBase_CentralContractGroup_CentralContractGroup]...';


GO
ALTER TABLE [Hardware].[InstallBase] DROP CONSTRAINT [FK_InstallBase_CentralContractGroup_CentralContractGroup];


GO
PRINT N'Dropping [Hardware].[FK_InstallBase_Pla_Pla]...';


GO
ALTER TABLE [Hardware].[InstallBase] DROP CONSTRAINT [FK_InstallBase_Pla_Pla];


GO
PRINT N'Altering [Hardware].[InstallBase]...';


GO
ALTER TABLE [Hardware].[InstallBase] DROP COLUMN [CentralContractGroup], COLUMN [Pla];


GO
PRINT N'Starting rebuilding table [History].[Hardware_InstallBase]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [History].[tmp_ms_xx_Hardware_InstallBase] (
    [Id]                   BIGINT     IDENTITY (1, 1) NOT NULL,
    [Wg]                   BIGINT     NULL,
    [Country]              BIGINT     NULL,
    [InstalledBaseCountry] FLOAT (53) NULL,
    [CostBlockHistory]     BIGINT     NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_History_Hardware_InstallBase_Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [History].[Hardware_InstallBase])
    BEGIN
        SET IDENTITY_INSERT [History].[tmp_ms_xx_Hardware_InstallBase] ON;
        INSERT INTO [History].[tmp_ms_xx_Hardware_InstallBase] ([Id], [Country], [Wg], [InstalledBaseCountry], [CostBlockHistory])
        SELECT   [Id],
                 [Country],
                 [Wg],
                 [InstalledBaseCountry],
                 [CostBlockHistory]
        FROM     [History].[Hardware_InstallBase]
        ORDER BY [Id] ASC;
        SET IDENTITY_INSERT [History].[tmp_ms_xx_Hardware_InstallBase] OFF;
    END

DROP TABLE [History].[Hardware_InstallBase];

EXECUTE sp_rename N'[History].[tmp_ms_xx_Hardware_InstallBase]', N'Hardware_InstallBase';

EXECUTE sp_rename N'[History].[tmp_ms_xx_constraint_PK_History_Hardware_InstallBase_Id1]', N'PK_History_Hardware_InstallBase_Id', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Creating [History].[FK_HistoryHardware_InstallBaseCountry_InputAtomsCountry]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] WITH NOCHECK
    ADD CONSTRAINT [FK_HistoryHardware_InstallBaseCountry_InputAtomsCountry] FOREIGN KEY ([Country]) REFERENCES [InputAtoms].[Country] ([Id]);


GO
PRINT N'Creating [History].[FK_HistoryHardware_InstallBaseWg_InputAtomsWg]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] WITH NOCHECK
    ADD CONSTRAINT [FK_HistoryHardware_InstallBaseWg_InputAtomsWg] FOREIGN KEY ([Wg]) REFERENCES [InputAtoms].[Wg] ([Id]);


GO
PRINT N'Creating [History].[FK_HistoryHardware_InstallBaseCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[Hardware_InstallBase] WITH NOCHECK
    ADD CONSTRAINT [FK_HistoryHardware_InstallBaseCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY ([CostBlockHistory]) REFERENCES [History].[CostBlockHistory] ([Id]);


GO
PRINT N'Refreshing [SoftwareSolution].[SwSpMaintenanceCostView]...';


GO
EXECUTE sp_refreshsqlmodule N'[SoftwareSolution].[SwSpMaintenanceCostView]';


GO
PRINT N'Refreshing [SoftwareSolution].[ServiceCostCalculationView]...';


GO
EXECUTE sp_refreshsqlmodule N'[SoftwareSolution].[ServiceCostCalculationView]';


GO
PRINT N'Refreshing [Report].[CalcOutputVsFREEZE]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[CalcOutputVsFREEZE]';


GO
PRINT N'Refreshing [Hardware].[GetCalcMember]...';


GO
EXECUTE sp_refreshsqlmodule N'[Hardware].[GetCalcMember]';


GO
PRINT N'Refreshing [Hardware].[GetCostsFull]...';


GO
EXECUTE sp_refreshsqlmodule N'[Hardware].[GetCostsFull]';


GO
PRINT N'Refreshing [Report].[SolutionPackPriceList]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SolutionPackPriceList]';


GO
PRINT N'Refreshing [Report].[SolutionPackPriceListDetail]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SolutionPackPriceListDetail]';


GO
PRINT N'Refreshing [Report].[SolutionPackProActiveCosting]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SolutionPackProActiveCosting]';


GO
PRINT N'Refreshing [Report].[SwServicePriceList]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SwServicePriceList]';


GO
PRINT N'Refreshing [Report].[SwServicePriceListDetail]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SwServicePriceListDetail]';


GO
PRINT N'Refreshing [Hardware].[GetCosts]...';


GO
EXECUTE sp_refreshsqlmodule N'[Hardware].[GetCosts]';


GO
PRINT N'Refreshing [Report].[CalcOutputNewVsOld]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[CalcOutputNewVsOld]';


GO
PRINT N'Refreshing [Report].[Contract]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[Contract]';


GO
PRINT N'Refreshing [Report].[GetCostsFull]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[GetCostsFull]';


GO
PRINT N'Refreshing [Report].[LogisticCostCalcCentral]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LogisticCostCalcCentral]';


GO
PRINT N'Refreshing [Report].[GetSwResultBySla]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[GetSwResultBySla]';


GO
PRINT N'Refreshing [Report].[GetCosts]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[GetCosts]';


GO
PRINT N'Refreshing [Report].[LogisticCostCalcCountry]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LogisticCostCalcCountry]';


GO
PRINT N'Refreshing [Report].[HddRetentionByCountry]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[HddRetentionByCountry]';


GO
PRINT N'Refreshing [Report].[Locap]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[Locap]';


GO
PRINT N'Refreshing [Report].[LocapDetailed]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LocapDetailed]';


GO
PRINT N'Refreshing [Report].[ProActive]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[ProActive]';


GO
PRINT N'Refreshing [Report].[GetServiceCostsBySla]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[GetServiceCostsBySla]';


GO
PRINT N'Refreshing [Hardware].[SpGetCosts]...';


GO
EXECUTE sp_refreshsqlmodule N'[Hardware].[SpGetCosts]';


GO
PRINT N'Checking existing data against newly created constraints';


GO
USE Scd_2;


GO
ALTER TABLE [History].[Hardware_InstallBase] WITH CHECK CHECK CONSTRAINT [FK_HistoryHardware_InstallBaseCountry_InputAtomsCountry];

ALTER TABLE [History].[Hardware_InstallBase] WITH CHECK CHECK CONSTRAINT [FK_HistoryHardware_InstallBaseWg_InputAtomsWg];

ALTER TABLE [History].[Hardware_InstallBase] WITH CHECK CHECK CONSTRAINT [FK_HistoryHardware_InstallBaseCostBlockHistory_HistoryCostBlockHistory];


GO
PRINT N'Update complete.';


GO
=======
  sp_rename '[InputAtoms].[CentralContractGroup].Name', 'Code', 'COLUMN';
  GO
  sp_rename '[InputAtoms].[CentralContractGroup].Description', 'Name', 'COLUMN';
  GO
>>>>>>> origin/master
