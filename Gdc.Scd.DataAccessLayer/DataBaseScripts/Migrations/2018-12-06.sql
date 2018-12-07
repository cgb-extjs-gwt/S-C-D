USE SCD_2;


GO
PRINT N'Dropping unnamed constraint on [SoftwareSolution].[ProActiveSw]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] DROP CONSTRAINT [DF__ProActive__Creat__6FB49575];


GO
PRINT N'Dropping unnamed constraint on [SoftwareSolution].[SwSpMaintenance]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [DF__SwSpMaint__Creat__719CDDE7];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_ProActiveSwCountry_InputAtomsCountry]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] DROP CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwCountry_InputAtomsCountry];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_ProActiveSwPla_InputAtomsPla]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] DROP CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwPla_InputAtomsPla];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_ProActiveSwSog_InputAtomsSog]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] DROP CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwSog_InputAtomsSog];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_ProActiveSwCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] DROP CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwCostBlockHistory_HistoryCostBlockHistory];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_SwSpMaintenancePla_InputAtomsPla]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] DROP CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenancePla_InputAtomsPla];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_SwSpMaintenanceSfab_InputAtomsSfab]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] DROP CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSfab_InputAtomsSfab];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_SwSpMaintenanceSog_InputAtomsSog]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] DROP CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSog_InputAtomsSog];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_SwSpMaintenanceAvailability_DependenciesAvailability]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] DROP CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceAvailability_DependenciesAvailability];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_SwSpMaintenanceCurrencyReinsurance_ReferencesCurrency]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] DROP CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceCurrencyReinsurance_ReferencesCurrency];


GO
PRINT N'Dropping [History].[FK_HistorySoftwareSolution_SwSpMaintenanceCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] DROP CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceCostBlockHistory_HistoryCostBlockHistory];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionProActiveSwCountry_InputAtomsCountry]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] DROP CONSTRAINT [FK_SoftwareSolutionProActiveSwCountry_InputAtomsCountry];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionProActiveSwPla_InputAtomsPla]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] DROP CONSTRAINT [FK_SoftwareSolutionProActiveSwPla_InputAtomsPla];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionProActiveSwSog_InputAtomsSog]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] DROP CONSTRAINT [FK_SoftwareSolutionProActiveSwSog_InputAtomsSog];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenancePla_InputAtomsPla]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [FK_SoftwareSolutionSwSpMaintenancePla_InputAtomsPla];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceSfab_InputAtomsSfab]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSfab_InputAtomsSfab];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceSog_InputAtomsSog]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSog_InputAtomsSog];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceAvailability_DependenciesAvailability]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceAvailability_DependenciesAvailability];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_ReferencesCurrency]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_ReferencesCurrency];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_Approved_ReferencesCurrency]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_Approved_ReferencesCurrency];


GO
PRINT N'Dropping [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceYearAvailability_DependenciesYear_Availability]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] DROP CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceYearAvailability_DependenciesYear_Availability];


GO
PRINT N'Starting rebuilding table [History].[SoftwareSolution_ProActiveSw]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [History].[tmp_ms_xx_SoftwareSolution_ProActiveSw] (
    [Id]                                      BIGINT     IDENTITY (1, 1) NOT NULL,
    [Country]                                 BIGINT     NULL,
    [Pla]                                     BIGINT     NULL,
    [Sog]                                     BIGINT     NULL,
    [SwDigit]                                 BIGINT     NULL,
    [LocalRemoteAccessSetupPreparationEffort] FLOAT (53) NULL,
    [LocalRegularUpdateReadyEffort]           FLOAT (53) NULL,
    [LocalPreparationShcEffort]               FLOAT (53) NULL,
    [CentralExecutionShcReportCost]           FLOAT (53) NULL,
    [LocalRemoteShcCustomerBriefingEffort]    FLOAT (53) NULL,
    [LocalOnSiteShcCustomerBriefingEffort]    FLOAT (53) NULL,
    [TravellingTime]                          FLOAT (53) NULL,
    [OnSiteHourlyRate]                        FLOAT (53) NULL,
    [CostBlockHistory]                        BIGINT     NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_History_SoftwareSolution_ProActiveSw_Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [History].[SoftwareSolution_ProActiveSw])
    BEGIN
        SET IDENTITY_INSERT [History].[tmp_ms_xx_SoftwareSolution_ProActiveSw] ON;
        INSERT INTO [History].[tmp_ms_xx_SoftwareSolution_ProActiveSw] ([Id], [Country], [Pla], [Sog], [LocalRemoteAccessSetupPreparationEffort], [LocalRegularUpdateReadyEffort], [LocalPreparationShcEffort], [CentralExecutionShcReportCost], [LocalRemoteShcCustomerBriefingEffort], [LocalOnSiteShcCustomerBriefingEffort], [TravellingTime], [OnSiteHourlyRate], [CostBlockHistory])
        SELECT   [Id],
                 [Country],
                 [Pla],
                 [Sog],
                 [LocalRemoteAccessSetupPreparationEffort],
                 [LocalRegularUpdateReadyEffort],
                 [LocalPreparationShcEffort],
                 [CentralExecutionShcReportCost],
                 [LocalRemoteShcCustomerBriefingEffort],
                 [LocalOnSiteShcCustomerBriefingEffort],
                 [TravellingTime],
                 [OnSiteHourlyRate],
                 [CostBlockHistory]
        FROM     [History].[SoftwareSolution_ProActiveSw]
        ORDER BY [Id] ASC;
        SET IDENTITY_INSERT [History].[tmp_ms_xx_SoftwareSolution_ProActiveSw] OFF;
    END

DROP TABLE [History].[SoftwareSolution_ProActiveSw];

EXECUTE sp_rename N'[History].[tmp_ms_xx_SoftwareSolution_ProActiveSw]', N'SoftwareSolution_ProActiveSw';

EXECUTE sp_rename N'[History].[tmp_ms_xx_constraint_PK_History_SoftwareSolution_ProActiveSw_Id1]', N'PK_History_SoftwareSolution_ProActiveSw_Id', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Starting rebuilding table [History].[SoftwareSolution_SwSpMaintenance]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [History].[tmp_ms_xx_SoftwareSolution_SwSpMaintenance] (
    [Id]                                       BIGINT     IDENTITY (1, 1) NOT NULL,
    [Pla]                                      BIGINT     NULL,
    [Sfab]                                     BIGINT     NULL,
    [Sog]                                      BIGINT     NULL,
    [SwDigit]                                  BIGINT     NULL,
    [YearAvailability]                         BIGINT     NULL,
    [Availability]                             BIGINT     NULL,
    [2ndLevelSupportCosts]                     FLOAT (53) NULL,
    [InstalledBaseSog]                         FLOAT (53) NULL,
    [ReinsuranceFlatfee]                       FLOAT (53) NULL,
    [CurrencyReinsurance]                      BIGINT     NULL,
    [RecommendedSwSpMaintenanceListPrice]      FLOAT (53) NULL,
    [MarkupForProductMarginSwLicenseListPrice] FLOAT (53) NULL,
    [ShareSwSpMaintenanceListPrice]            FLOAT (53) NULL,
    [DiscountDealerPrice]                      FLOAT (53) NULL,
    [CostBlockHistory]                         BIGINT     NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_History_SoftwareSolution_SwSpMaintenance_Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [History].[SoftwareSolution_SwSpMaintenance])
    BEGIN
        SET IDENTITY_INSERT [History].[tmp_ms_xx_SoftwareSolution_SwSpMaintenance] ON;
        INSERT INTO [History].[tmp_ms_xx_SoftwareSolution_SwSpMaintenance] ([Id], [Pla], [Sfab], [Sog], [YearAvailability], [Availability], [2ndLevelSupportCosts], [InstalledBaseSog], [ReinsuranceFlatfee], [CurrencyReinsurance], [RecommendedSwSpMaintenanceListPrice], [MarkupForProductMarginSwLicenseListPrice], [ShareSwSpMaintenanceListPrice], [DiscountDealerPrice], [CostBlockHistory])
        SELECT   [Id],
                 [Pla],
                 [Sfab],
                 [Sog],
                 [YearAvailability],
                 [Availability],
                 [2ndLevelSupportCosts],
                 [InstalledBaseSog],
                 [ReinsuranceFlatfee],
                 [CurrencyReinsurance],
                 [RecommendedSwSpMaintenanceListPrice],
                 [MarkupForProductMarginSwLicenseListPrice],
                 [ShareSwSpMaintenanceListPrice],
                 [DiscountDealerPrice],
                 [CostBlockHistory]
        FROM     [History].[SoftwareSolution_SwSpMaintenance]
        ORDER BY [Id] ASC;
        SET IDENTITY_INSERT [History].[tmp_ms_xx_SoftwareSolution_SwSpMaintenance] OFF;
    END

DROP TABLE [History].[SoftwareSolution_SwSpMaintenance];

EXECUTE sp_rename N'[History].[tmp_ms_xx_SoftwareSolution_SwSpMaintenance]', N'SoftwareSolution_SwSpMaintenance';

EXECUTE sp_rename N'[History].[tmp_ms_xx_constraint_PK_History_SoftwareSolution_SwSpMaintenance_Id1]', N'PK_History_SoftwareSolution_SwSpMaintenance_Id', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Altering [InputAtoms].[Sog]...';


GO
ALTER TABLE [InputAtoms].[Sog] DROP COLUMN [ResponsiblePerson];


GO
/*
The column [SoftwareSolution].[ProActiveSw].[SwDigit] on table [SoftwareSolution].[ProActiveSw] must be added, but the column has no default value and does not allow NULL values. If the table contains data, the ALTER script will not work. To avoid this issue you must either: add a default value to the column, mark it as allowing NULL values, or enable the generation of smart-defaults as a deployment option.
*/
GO
PRINT N'Starting rebuilding table [SoftwareSolution].[ProActiveSw]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [SoftwareSolution].[tmp_ms_xx_ProActiveSw] (
    [Id]                                               BIGINT     IDENTITY (1, 1) NOT NULL,
    [Country]                                          BIGINT     NOT NULL,
    [Pla]                                              BIGINT     NOT NULL,
    [Sog]                                              BIGINT     NOT NULL,
    [SwDigit]                                          BIGINT     NOT NULL,
    [LocalRemoteAccessSetupPreparationEffort]          FLOAT (53) NULL,
    [LocalRegularUpdateReadyEffort]                    FLOAT (53) NULL,
    [LocalPreparationShcEffort]                        FLOAT (53) NULL,
    [CentralExecutionShcReportCost]                    FLOAT (53) NULL,
    [LocalRemoteShcCustomerBriefingEffort]             FLOAT (53) NULL,
    [LocalOnSiteShcCustomerBriefingEffort]             FLOAT (53) NULL,
    [TravellingTime]                                   FLOAT (53) NULL,
    [OnSiteHourlyRate]                                 FLOAT (53) NULL,
    [LocalRemoteAccessSetupPreparationEffort_Approved] FLOAT (53) NULL,
    [LocalRegularUpdateReadyEffort_Approved]           FLOAT (53) NULL,
    [LocalPreparationShcEffort_Approved]               FLOAT (53) NULL,
    [CentralExecutionShcReportCost_Approved]           FLOAT (53) NULL,
    [LocalRemoteShcCustomerBriefingEffort_Approved]    FLOAT (53) NULL,
    [LocalOnSiteShcCustomerBriefingEffort_Approved]    FLOAT (53) NULL,
    [TravellingTime_Approved]                          FLOAT (53) NULL,
    [OnSiteHourlyRate_Approved]                        FLOAT (53) NULL,
    [CreatedDateTime]                                  DATETIME   DEFAULT (getutcdate()) NOT NULL,
    [DeactivatedDateTime]                              DATETIME   NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_SoftwareSolution_ProActiveSw_Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [SoftwareSolution].[ProActiveSw])
    BEGIN
        SET IDENTITY_INSERT [SoftwareSolution].[tmp_ms_xx_ProActiveSw] ON;
        INSERT INTO [SoftwareSolution].[tmp_ms_xx_ProActiveSw] ([Id], [Country], [Pla], [Sog], [SwDigit], [LocalRemoteAccessSetupPreparationEffort], [LocalRegularUpdateReadyEffort], [LocalPreparationShcEffort], [CentralExecutionShcReportCost], [LocalRemoteShcCustomerBriefingEffort], [LocalOnSiteShcCustomerBriefingEffort], [TravellingTime], [OnSiteHourlyRate], [LocalRemoteAccessSetupPreparationEffort_Approved], [LocalRegularUpdateReadyEffort_Approved], [LocalPreparationShcEffort_Approved], [CentralExecutionShcReportCost_Approved], [LocalRemoteShcCustomerBriefingEffort_Approved], [LocalOnSiteShcCustomerBriefingEffort_Approved], [TravellingTime_Approved], [OnSiteHourlyRate_Approved], [CreatedDateTime], [DeactivatedDateTime])
        SELECT   [SoftwareSolution].[ProActiveSw].[Id],
                 [SoftwareSolution].[ProActiveSw].[Country],
                 [SoftwareSolution].[ProActiveSw].[Pla],
                 [SoftwareSolution].[ProActiveSw].[Sog],
				 [SwDigit].[Id],
                 [SoftwareSolution].[ProActiveSw].[LocalRemoteAccessSetupPreparationEffort],
                 [SoftwareSolution].[ProActiveSw].[LocalRegularUpdateReadyEffort],
                 [SoftwareSolution].[ProActiveSw].[LocalPreparationShcEffort],
                 [SoftwareSolution].[ProActiveSw].[CentralExecutionShcReportCost],
                 [SoftwareSolution].[ProActiveSw].[LocalRemoteShcCustomerBriefingEffort],
                 [SoftwareSolution].[ProActiveSw].[LocalOnSiteShcCustomerBriefingEffort],
                 [SoftwareSolution].[ProActiveSw].[TravellingTime],
                 [SoftwareSolution].[ProActiveSw]. [OnSiteHourlyRate],
                 [SoftwareSolution].[ProActiveSw].[LocalRemoteAccessSetupPreparationEffort_Approved],
                 [SoftwareSolution].[ProActiveSw].[LocalRegularUpdateReadyEffort_Approved],
                 [SoftwareSolution].[ProActiveSw].[LocalPreparationShcEffort_Approved],
                 [SoftwareSolution].[ProActiveSw].[CentralExecutionShcReportCost_Approved],
                 [SoftwareSolution].[ProActiveSw].[LocalRemoteShcCustomerBriefingEffort_Approved],
                 [SoftwareSolution].[ProActiveSw].[LocalOnSiteShcCustomerBriefingEffort_Approved],
                 [SoftwareSolution].[ProActiveSw].[TravellingTime_Approved],
                 [SoftwareSolution].[ProActiveSw].[OnSiteHourlyRate_Approved],
                 [SoftwareSolution].[ProActiveSw].[CreatedDateTime],
                 [SoftwareSolution].[ProActiveSw].[DeactivatedDateTime]
        FROM     [SoftwareSolution].[ProActiveSw]
		INNER JOIN (SELECT  * FROM [InputAtoms].[SwDigit] WHERE [DeactivatedDateTime] IS NULL) AS [SwDigit] ON [SoftwareSolution].[ProActiveSw].[Sog] = [SwDigit].[SogId] 
        ORDER BY [SoftwareSolution].[ProActiveSw].[Id] ASC;
        SET IDENTITY_INSERT [SoftwareSolution].[tmp_ms_xx_ProActiveSw] OFF;
    END

DROP TABLE [SoftwareSolution].[ProActiveSw];

EXECUTE sp_rename N'[SoftwareSolution].[tmp_ms_xx_ProActiveSw]', N'ProActiveSw';

EXECUTE sp_rename N'[SoftwareSolution].[tmp_ms_xx_constraint_PK_SoftwareSolution_ProActiveSw_Id1]', N'PK_SoftwareSolution_ProActiveSw_Id', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
/*
The column [SoftwareSolution].[SwSpMaintenance].[SwDigit] on table [SoftwareSolution].[SwSpMaintenance] must be added, but the column has no default value and does not allow NULL values. If the table contains data, the ALTER script will not work. To avoid this issue you must either: add a default value to the column, mark it as allowing NULL values, or enable the generation of smart-defaults as a deployment option.
*/
GO
PRINT N'Starting rebuilding table [SoftwareSolution].[SwSpMaintenance]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [SoftwareSolution].[tmp_ms_xx_SwSpMaintenance] (
    [Id]                                                BIGINT     IDENTITY (1, 1) NOT NULL,
    [Pla]                                               BIGINT     NOT NULL,
    [Sfab]                                              BIGINT     NOT NULL,
    [Sog]                                               BIGINT     NOT NULL,
    [SwDigit]                                           BIGINT     NOT NULL,
    [YearAvailability]                                  BIGINT     NOT NULL,
    [Availability]                                      BIGINT     NOT NULL,
    [2ndLevelSupportCosts]                              FLOAT (53) NULL,
    [InstalledBaseSog]                                  FLOAT (53) NULL,
    [ReinsuranceFlatfee]                                FLOAT (53) NULL,
    [CurrencyReinsurance]                               BIGINT     NULL,
    [RecommendedSwSpMaintenanceListPrice]               FLOAT (53) NULL,
    [MarkupForProductMarginSwLicenseListPrice]          FLOAT (53) NULL,
    [ShareSwSpMaintenanceListPrice]                     FLOAT (53) NULL,
    [DiscountDealerPrice]                               FLOAT (53) NULL,
    [2ndLevelSupportCosts_Approved]                     FLOAT (53) NULL,
    [InstalledBaseSog_Approved]                         FLOAT (53) NULL,
    [ReinsuranceFlatfee_Approved]                       FLOAT (53) NULL,
    [CurrencyReinsurance_Approved]                      BIGINT     NULL,
    [RecommendedSwSpMaintenanceListPrice_Approved]      FLOAT (53) NULL,
    [MarkupForProductMarginSwLicenseListPrice_Approved] FLOAT (53) NULL,
    [ShareSwSpMaintenanceListPrice_Approved]            FLOAT (53) NULL,
    [DiscountDealerPrice_Approved]                      FLOAT (53) NULL,
    [CreatedDateTime]                                   DATETIME   DEFAULT (getutcdate()) NOT NULL,
    [DeactivatedDateTime]                               DATETIME   NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_SoftwareSolution_SwSpMaintenance_Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [SoftwareSolution].[SwSpMaintenance])
    BEGIN
        SET IDENTITY_INSERT [SoftwareSolution].[tmp_ms_xx_SwSpMaintenance] ON;
        INSERT INTO [SoftwareSolution].[tmp_ms_xx_SwSpMaintenance] ([Id], [Pla], [Sfab], [Sog], [SwDigit], [YearAvailability], [Availability], [2ndLevelSupportCosts], [InstalledBaseSog], [ReinsuranceFlatfee], [CurrencyReinsurance], [RecommendedSwSpMaintenanceListPrice], [MarkupForProductMarginSwLicenseListPrice], [ShareSwSpMaintenanceListPrice], [DiscountDealerPrice], [2ndLevelSupportCosts_Approved], [InstalledBaseSog_Approved], [ReinsuranceFlatfee_Approved], [CurrencyReinsurance_Approved], [RecommendedSwSpMaintenanceListPrice_Approved], [MarkupForProductMarginSwLicenseListPrice_Approved], [ShareSwSpMaintenanceListPrice_Approved], [DiscountDealerPrice_Approved], [CreatedDateTime], [DeactivatedDateTime])
        SELECT   [SoftwareSolution].[SwSpMaintenance].[Id],
                 [SoftwareSolution].[SwSpMaintenance].[Pla],
                 [SoftwareSolution].[SwSpMaintenance].[Sfab],
                 [SoftwareSolution].[SwSpMaintenance].[Sog],
				 [SwDigit].[Id],
                 [SoftwareSolution].[SwSpMaintenance].[YearAvailability],
                 [SoftwareSolution].[SwSpMaintenance].[Availability],
                 [SoftwareSolution].[SwSpMaintenance].[2ndLevelSupportCosts],
                 [SoftwareSolution].[SwSpMaintenance].[InstalledBaseSog],
                 [SoftwareSolution].[SwSpMaintenance].[ReinsuranceFlatfee],
                 [SoftwareSolution].[SwSpMaintenance].[CurrencyReinsurance],
                 [SoftwareSolution].[SwSpMaintenance].[RecommendedSwSpMaintenanceListPrice],
                 [SoftwareSolution].[SwSpMaintenance].[MarkupForProductMarginSwLicenseListPrice],
                 [SoftwareSolution].[SwSpMaintenance]. [ShareSwSpMaintenanceListPrice],
                 [SoftwareSolution].[SwSpMaintenance].[DiscountDealerPrice],
                 [SoftwareSolution].[SwSpMaintenance].[2ndLevelSupportCosts_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[InstalledBaseSog_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[ReinsuranceFlatfee_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[CurrencyReinsurance_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[RecommendedSwSpMaintenanceListPrice_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[MarkupForProductMarginSwLicenseListPrice_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[ShareSwSpMaintenanceListPrice_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[DiscountDealerPrice_Approved],
                 [SoftwareSolution].[SwSpMaintenance].[CreatedDateTime],
                 [SoftwareSolution].[SwSpMaintenance].[DeactivatedDateTime]
        FROM     [SoftwareSolution].[SwSpMaintenance]
		INNER JOIN (SELECT  * FROM [InputAtoms].[SwDigit] WHERE [DeactivatedDateTime] IS NULL) AS [SwDigit] ON [SoftwareSolution].[SwSpMaintenance].[Sog] = [SwDigit].[SogId]
        ORDER BY [SoftwareSolution].[SwSpMaintenance].[Id] ASC;
        SET IDENTITY_INSERT [SoftwareSolution].[tmp_ms_xx_SwSpMaintenance] OFF;
    END

DROP TABLE [SoftwareSolution].[SwSpMaintenance];

EXECUTE sp_rename N'[SoftwareSolution].[tmp_ms_xx_SwSpMaintenance]', N'SwSpMaintenance';

EXECUTE sp_rename N'[SoftwareSolution].[tmp_ms_xx_constraint_PK_SoftwareSolution_SwSpMaintenance_Id1]', N'PK_SoftwareSolution_SwSpMaintenance_Id', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Creating [History_RelatedItems].[SwDigit]...';


GO
CREATE TABLE [History_RelatedItems].[SwDigit] (
    [CostBlockHistory] BIGINT NOT NULL,
    [SwDigit]          BIGINT NULL
);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_ProActiveSwCountry_InputAtomsCountry]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwCountry_InputAtomsCountry] FOREIGN KEY ([Country]) REFERENCES [InputAtoms].[Country] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_ProActiveSwPla_InputAtomsPla]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwPla_InputAtomsPla] FOREIGN KEY ([Pla]) REFERENCES [InputAtoms].[Pla] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_ProActiveSwSog_InputAtomsSog]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwSog_InputAtomsSog] FOREIGN KEY ([Sog]) REFERENCES [InputAtoms].[Sog] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_ProActiveSwCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY ([CostBlockHistory]) REFERENCES [History].[CostBlockHistory] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_ProActiveSwSwDigit_InputAtomsSwDigit]...';


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwSwDigit_InputAtomsSwDigit] FOREIGN KEY ([SwDigit]) REFERENCES [InputAtoms].[SwDigit] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_SwSpMaintenancePla_InputAtomsPla]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenancePla_InputAtomsPla] FOREIGN KEY ([Pla]) REFERENCES [InputAtoms].[Pla] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_SwSpMaintenanceSfab_InputAtomsSfab]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSfab_InputAtomsSfab] FOREIGN KEY ([Sfab]) REFERENCES [InputAtoms].[Sfab] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_SwSpMaintenanceSog_InputAtomsSog]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSog_InputAtomsSog] FOREIGN KEY ([Sog]) REFERENCES [InputAtoms].[Sog] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_SwSpMaintenanceAvailability_DependenciesAvailability]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceAvailability_DependenciesAvailability] FOREIGN KEY ([Availability]) REFERENCES [Dependencies].[Availability] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_SwSpMaintenanceCurrencyReinsurance_ReferencesCurrency]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceCurrencyReinsurance_ReferencesCurrency] FOREIGN KEY ([CurrencyReinsurance]) REFERENCES [References].[Currency] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_SwSpMaintenanceCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY ([CostBlockHistory]) REFERENCES [History].[CostBlockHistory] ([Id]);


GO
PRINT N'Creating [History].[FK_HistorySoftwareSolution_SwSpMaintenanceSwDigit_InputAtomsSwDigit]...';


GO
ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSwDigit_InputAtomsSwDigit] FOREIGN KEY ([SwDigit]) REFERENCES [InputAtoms].[SwDigit] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionProActiveSwCountry_InputAtomsCountry]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionProActiveSwCountry_InputAtomsCountry] FOREIGN KEY ([Country]) REFERENCES [InputAtoms].[Country] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionProActiveSwPla_InputAtomsPla]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionProActiveSwPla_InputAtomsPla] FOREIGN KEY ([Pla]) REFERENCES [InputAtoms].[Pla] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionProActiveSwSog_InputAtomsSog]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionProActiveSwSog_InputAtomsSog] FOREIGN KEY ([Sog]) REFERENCES [InputAtoms].[Sog] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionProActiveSwSwDigit_InputAtomsSwDigit]...';


GO
ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionProActiveSwSwDigit_InputAtomsSwDigit] FOREIGN KEY ([SwDigit]) REFERENCES [InputAtoms].[SwDigit] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenancePla_InputAtomsPla]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenancePla_InputAtomsPla] FOREIGN KEY ([Pla]) REFERENCES [InputAtoms].[Pla] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceSfab_InputAtomsSfab]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSfab_InputAtomsSfab] FOREIGN KEY ([Sfab]) REFERENCES [InputAtoms].[Sfab] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceSog_InputAtomsSog]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSog_InputAtomsSog] FOREIGN KEY ([Sog]) REFERENCES [InputAtoms].[Sog] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceAvailability_DependenciesAvailability]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceAvailability_DependenciesAvailability] FOREIGN KEY ([Availability]) REFERENCES [Dependencies].[Availability] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_ReferencesCurrency]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_ReferencesCurrency] FOREIGN KEY ([CurrencyReinsurance]) REFERENCES [References].[Currency] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_Approved_ReferencesCurrency]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_Approved_ReferencesCurrency] FOREIGN KEY ([CurrencyReinsurance_Approved]) REFERENCES [References].[Currency] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceYearAvailability_DependenciesYear_Availability]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceYearAvailability_DependenciesYear_Availability] FOREIGN KEY ([YearAvailability]) REFERENCES [Dependencies].[Year_Availability] ([Id]);


GO
PRINT N'Creating [SoftwareSolution].[FK_SoftwareSolutionSwSpMaintenanceSwDigit_InputAtomsSwDigit]...';


GO
ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH NOCHECK
    ADD CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSwDigit_InputAtomsSwDigit] FOREIGN KEY ([SwDigit]) REFERENCES [InputAtoms].[SwDigit] ([Id]);


GO
PRINT N'Creating [History_RelatedItems].[FK_History_RelatedItemsSwDigitCostBlockHistory_HistoryCostBlockHistory]...';


GO
ALTER TABLE [History_RelatedItems].[SwDigit] WITH NOCHECK
    ADD CONSTRAINT [FK_History_RelatedItemsSwDigitCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY ([CostBlockHistory]) REFERENCES [History].[CostBlockHistory] ([Id]);


GO
PRINT N'Creating [History_RelatedItems].[FK_History_RelatedItemsSwDigitSwDigit_InputAtomsSwDigit]...';


GO
ALTER TABLE [History_RelatedItems].[SwDigit] WITH NOCHECK
    ADD CONSTRAINT [FK_History_RelatedItemsSwDigitSwDigit_InputAtomsSwDigit] FOREIGN KEY ([SwDigit]) REFERENCES [InputAtoms].[SwDigit] ([Id]);


GO
PRINT N'Refreshing [InputAtoms].[WgSogView]...';


GO
EXECUTE sp_refreshsqlmodule N'[InputAtoms].[WgSogView]';


GO
PRINT N'Refreshing [SoftwareSolution].[ProActiveView]...';


GO
EXECUTE sp_refreshsqlmodule N'[SoftwareSolution].[ProActiveView]';


GO
PRINT N'Refreshing [SoftwareSolution].[SwSpMaintenanceView]...';


GO
EXECUTE sp_refreshsqlmodule N'[SoftwareSolution].[SwSpMaintenanceView]';


GO
PRINT N'Refreshing [SoftwareSolution].[SwSpMaintenanceCostView]...';


GO
EXECUTE sp_refreshsqlmodule N'[SoftwareSolution].[SwSpMaintenanceCostView]';


GO
PRINT N'Refreshing [SoftwareSolution].[ServiceCostCalculationView]...';


GO
EXECUTE sp_refreshsqlmodule N'[SoftwareSolution].[ServiceCostCalculationView]';


GO
PRINT N'Refreshing [Report].[SolutionPackPriceList]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SolutionPackPriceList]';


GO
PRINT N'Refreshing [Report].[SolutionPackPriceListDetail]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SolutionPackPriceListDetail]';


GO
PRINT N'Refreshing [Report].[SwServicePriceList]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SwServicePriceList]';


GO
PRINT N'Refreshing [Report].[SwServicePriceListDetail]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SwServicePriceListDetail]';


GO
PRINT N'Refreshing [Report].[CalcOutputNewVsOld]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[CalcOutputNewVsOld]';


GO
PRINT N'Refreshing [Report].[CalcOutputVsFREEZE]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[CalcOutputVsFREEZE]';


GO
PRINT N'Refreshing [Report].[CalcParameterHw]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[CalcParameterHw]';


GO
PRINT N'Refreshing [Report].[PoStandardWarrantyMaterial]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[PoStandardWarrantyMaterial]';


GO
PRINT N'Refreshing [Report].[CalcParameterProActive]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[CalcParameterProActive]';


GO
PRINT N'Refreshing [Report].[Contract]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[Contract]';


GO
PRINT N'Refreshing [Report].[HddRetentionByCountry]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[HddRetentionByCountry]';


GO
PRINT N'Refreshing [Report].[HddRetentionCentral]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[HddRetentionCentral]';


GO
PRINT N'Refreshing [Report].[HddRetentionParameter]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[HddRetentionParameter]';


GO
PRINT N'Refreshing [Report].[Locap]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[Locap]';


GO
PRINT N'Refreshing [Report].[LocapDetailed]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LocapDetailed]';


GO
PRINT N'Refreshing [Report].[LogisticCostCentral]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LogisticCostCentral]';


GO
PRINT N'Refreshing [Report].[LogisticCostInputCentral]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LogisticCostInputCentral]';


GO
PRINT N'Refreshing [Report].[SolutionPackProActiveCosting]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[SolutionPackProActiveCosting]';


GO
PRINT N'Refreshing [Report].[LogisticCostCountry]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LogisticCostCountry]';


GO
PRINT N'Refreshing [Report].[LogisticCostInputCountry]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[LogisticCostInputCountry]';


GO
PRINT N'Refreshing [Report].[GetSwResultBySla]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[GetSwResultBySla]';


GO
PRINT N'Refreshing [Report].[HddRetention]...';


GO
EXECUTE sp_refreshsqlmodule N'[Report].[HddRetention]';


GO
PRINT N'Altering [dbo].[GetAvailabilityFeeCoverageCombination]...';


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
GO
PRINT N'Checking existing data against newly created constraints';


GO
USE SCD_2;


GO
ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwCountry_InputAtomsCountry];

ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwPla_InputAtomsPla];

ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwSog_InputAtomsSog];

ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwCostBlockHistory_HistoryCostBlockHistory];

ALTER TABLE [History].[SoftwareSolution_ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_ProActiveSwSwDigit_InputAtomsSwDigit];

ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenancePla_InputAtomsPla];

ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSfab_InputAtomsSfab];

ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSog_InputAtomsSog];

ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceAvailability_DependenciesAvailability];

ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceCurrencyReinsurance_ReferencesCurrency];

ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceCostBlockHistory_HistoryCostBlockHistory];

ALTER TABLE [History].[SoftwareSolution_SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_HistorySoftwareSolution_SwSpMaintenanceSwDigit_InputAtomsSwDigit];

ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionProActiveSwCountry_InputAtomsCountry];

ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionProActiveSwPla_InputAtomsPla];

ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionProActiveSwSog_InputAtomsSog];

ALTER TABLE [SoftwareSolution].[ProActiveSw] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionProActiveSwSwDigit_InputAtomsSwDigit];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenancePla_InputAtomsPla];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSfab_InputAtomsSfab];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSog_InputAtomsSog];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceAvailability_DependenciesAvailability];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_ReferencesCurrency];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceCurrencyReinsurance_Approved_ReferencesCurrency];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceYearAvailability_DependenciesYear_Availability];

ALTER TABLE [SoftwareSolution].[SwSpMaintenance] WITH CHECK CHECK CONSTRAINT [FK_SoftwareSolutionSwSpMaintenanceSwDigit_InputAtomsSwDigit];

ALTER TABLE [History_RelatedItems].[SwDigit] WITH CHECK CHECK CONSTRAINT [FK_History_RelatedItemsSwDigitCostBlockHistory_HistoryCostBlockHistory];

ALTER TABLE [History_RelatedItems].[SwDigit] WITH CHECK CHECK CONSTRAINT [FK_History_RelatedItemsSwDigitSwDigit_InputAtomsSwDigit];


GO
PRINT N'Update complete.';


GO
