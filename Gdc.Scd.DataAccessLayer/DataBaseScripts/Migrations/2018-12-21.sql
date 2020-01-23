IF EXISTS (select top 1 1 from [Hardware].[RoleCodeHourlyRates])
    RAISERROR (N'Rows were detected. The schema update is terminating because data loss might occur.', 16, 127) WITH NOWAIT

GO
PRINT N'Dropping unnamed constraint on [Hardware].[RoleCodeHourlyRates]...';


GO
ALTER TABLE [Hardware].[RoleCodeHourlyRates] DROP CONSTRAINT [DF__RoleCodeH__Creat__681373AD];


GO
PRINT N'Dropping [Hardware].[FK_HardwareRoleCodeHourlyRatesRoleCode_InputAtomsRoleCode]...';


GO
ALTER TABLE [Hardware].[RoleCodeHourlyRates] DROP CONSTRAINT [FK_HardwareRoleCodeHourlyRatesRoleCode_InputAtomsRoleCode];


GO
PRINT N'Dropping [History].[FK_HistoryHardware_RoleCodeHourlyRatesRoleCode_InputAtomsRoleCode]...';


GO
ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] DROP CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesRoleCode_InputAtomsRoleCode];


GO
PRINT N'Dropping [History].[FK_HistoryHardware_RoleCodeHourlyRatesCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] DROP CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesCostBlockHistory_HistoryCostBlockHistory];


GO
PRINT N'Starting rebuilding table [Hardware].[RoleCodeHourlyRates]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [Hardware].[tmp_ms_xx_RoleCodeHourlyRates] (
    [Id]                         BIGINT     IDENTITY (1, 1) NOT NULL,
    [Country]                    BIGINT     NOT NULL,
    [RoleCode]                   BIGINT     NOT NULL,
    [OnsiteHourlyRates]          FLOAT (53) NULL,
    [OnsiteHourlyRates_Approved] FLOAT (53) NULL,
    [CreatedDateTime]            DATETIME   DEFAULT (getutcdate()) NOT NULL,
    [DeactivatedDateTime]        DATETIME   NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_Hardware_RoleCodeHourlyRates_Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [Hardware].[RoleCodeHourlyRates])
    BEGIN
        INSERT INTO [Hardware].[tmp_ms_xx_RoleCodeHourlyRates] ([Country], [RoleCode], [OnsiteHourlyRates], [OnsiteHourlyRates_Approved], [CreatedDateTime], [DeactivatedDateTime])
        SELECT   [Country].[Id], 
                 rates.[RoleCode],
                 rates.[OnsiteHourlyRates],
                 rates.[OnsiteHourlyRates_Approved],
                 rates.[CreatedDateTime],
                 rates.[DeactivatedDateTime]
        FROM     [Hardware].[RoleCodeHourlyRates] as rates
		CROSS JOIN (SELECT [Id] FROM [InputAtoms].[Country] where IsMaster=1) AS [Country]
        ORDER BY rates.[Id] ASC;
    END

DROP TABLE [Hardware].[RoleCodeHourlyRates];

EXECUTE sp_rename N'[Hardware].[tmp_ms_xx_RoleCodeHourlyRates]', N'RoleCodeHourlyRates';

EXECUTE sp_rename N'[Hardware].[tmp_ms_xx_constraint_PK_Hardware_RoleCodeHourlyRates_Id1]', N'PK_Hardware_RoleCodeHourlyRates_Id', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [History].[Hardware_RoleCodeHourlyRates]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [History].[tmp_ms_xx_Hardware_RoleCodeHourlyRates] (
    [Id]                BIGINT     IDENTITY (1, 1) NOT NULL,
    [Country]           BIGINT     NULL,
    [RoleCode]          BIGINT     NULL,
    [OnsiteHourlyRates] FLOAT (53) NULL,
    [CostBlockHistory]  BIGINT     NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_History_Hardware_RoleCodeHourlyRates_Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [History].[Hardware_RoleCodeHourlyRates])
    BEGIN
        INSERT INTO [History].[tmp_ms_xx_Hardware_RoleCodeHourlyRates] ([Country], [RoleCode], [OnsiteHourlyRates], [CostBlockHistory])
        SELECT   [Country].[Id],
                 rates.[RoleCode],
                 rates.[OnsiteHourlyRates],
                 rates.[CostBlockHistory]
        FROM     [History].[Hardware_RoleCodeHourlyRates] as rates
		CROSS JOIN (SELECT [Id] FROM [InputAtoms].[Country] where IsMaster=1) AS [Country]
        ORDER BY rates.[Id] ASC;
    END

DROP TABLE [History].[Hardware_RoleCodeHourlyRates];

EXECUTE sp_rename N'[History].[tmp_ms_xx_Hardware_RoleCodeHourlyRates]', N'Hardware_RoleCodeHourlyRates';

EXECUTE sp_rename N'[History].[tmp_ms_xx_constraint_PK_History_Hardware_RoleCodeHourlyRates_Id1]', N'PK_History_Hardware_RoleCodeHourlyRates_Id', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Creating [Hardware].[FK_HardwareRoleCodeHourlyRatesRoleCode_InputAtomsRoleCode]...';


GO
ALTER TABLE [Hardware].[RoleCodeHourlyRates] WITH NOCHECK
    ADD CONSTRAINT [FK_HardwareRoleCodeHourlyRatesRoleCode_InputAtomsRoleCode] FOREIGN KEY ([RoleCode]) REFERENCES [InputAtoms].[RoleCode] ([Id]);


GO
PRINT N'Creating [Hardware].[FK_HardwareRoleCodeHourlyRatesCountry_InputAtomsCountry]...';


GO
ALTER TABLE [Hardware].[RoleCodeHourlyRates] WITH NOCHECK
    ADD CONSTRAINT [FK_HardwareRoleCodeHourlyRatesCountry_InputAtomsCountry] FOREIGN KEY ([Country]) REFERENCES [InputAtoms].[Country] ([Id]);


GO
PRINT N'Creating [History].[FK_HistoryHardware_RoleCodeHourlyRatesRoleCode_InputAtomsRoleCode]...';


GO
ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] WITH NOCHECK
    ADD CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesRoleCode_InputAtomsRoleCode] FOREIGN KEY ([RoleCode]) REFERENCES [InputAtoms].[RoleCode] ([Id]);


GO
PRINT N'Creating [History].[FK_HistoryHardware_RoleCodeHourlyRatesCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] WITH NOCHECK
    ADD CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY ([CostBlockHistory]) REFERENCES [History].[CostBlockHistory] ([Id]);


GO
PRINT N'Creating [History].[FK_HistoryHardware_RoleCodeHourlyRatesCountry_InputAtomsCountry]...';


GO
ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] WITH NOCHECK
    ADD CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesCountry_InputAtomsCountry] FOREIGN KEY ([Country]) REFERENCES [InputAtoms].[Country] ([Id]);


GO
PRINT N'Checking existing data against newly created constraints';


GO
USE Scd_2;


GO
ALTER TABLE [Hardware].[RoleCodeHourlyRates] WITH CHECK CHECK CONSTRAINT [FK_HardwareRoleCodeHourlyRatesRoleCode_InputAtomsRoleCode];

ALTER TABLE [Hardware].[RoleCodeHourlyRates] WITH CHECK CHECK CONSTRAINT [FK_HardwareRoleCodeHourlyRatesCountry_InputAtomsCountry];

ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] WITH CHECK CHECK CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesRoleCode_InputAtomsRoleCode];

ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] WITH CHECK CHECK CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesCostBlockHistory_HistoryCostBlockHistory];

ALTER TABLE [History].[Hardware_RoleCodeHourlyRates] WITH CHECK CHECK CONSTRAINT [FK_HistoryHardware_RoleCodeHourlyRatesCountry_InputAtomsCountry];


GO
PRINT N'Update complete.';


GO
