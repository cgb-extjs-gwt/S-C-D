exec spDropConstaint '[Fsp].[HwFspCodeTranslation]', '[PK_HwFspCodeTranslation]';

exec spDropConstaint '[Hardware].[MarkupOtherCosts]', '[PK_Hardware_MarkupOtherCosts_Id]';
exec spDropConstaint '[Hardware].[MarkupOtherCosts]', '[PK_Hardware_MarkupOtherCosts]';
exec spDropConstaint '[Hardware].[MarkupStandardWaranty]', '[PK_Hardware_MarkupStandardWaranty]';
exec spDropConstaint '[Hardware].[MarkupStandardWaranty]', '[PK_Hardware_MarkupStandardWaranty_Id]';
exec spDropConstaint '[Hardware].[ProActive]', '[PK_Hardware_ProActive_Id]';
exec spDropConstaint '[Hardware].[ProActive]', '[PK_Hardware_ProActive]';

exec spDropIndex '[Hardware].[LogisticsCosts]', '[ix_Hardware_LogisticsCosts]';
exec spDropIndex '[Hardware].[LogisticsCosts]', '[IX_Hardware_LogisticsCosts_Country]';
exec spDropIndex '[Hardware].[MarkupOtherCosts]', '[ix_Atom_MarkupOtherCosts]';
exec spDropIndex '[Hardware].[MarkupOtherCosts]', '[IX_Hardware_MarkupOtherCosts_Country]';
exec spDropIndex '[Hardware].[MarkupStandardWaranty]', '[IX_Hardware_MarkupStandardWaranty_Country]';
exec spDropIndex '[Hardware].[MarkupStandardWaranty]', '[ix_MarkupStandardWaranty_Country_Wg]';
exec spDropIndex '[Hardware].[AvailabilityFeeCalc]', '[ix_Hardware_AvailabilityFeeCalc]'

exec spDropIndex '[Portfolio].[LocalPortfolio]', '[IX_LocalPortfolio_Country_Sla]';
exec spDropIndex '[Portfolio].[LocalPortfolio]', '[IX_LocalPortfolio_Sla]';

exec spDropConstaint '[Hardware].[LogisticsCosts]', '[PK_Hardware_LogisticsCosts_Id]';
exec spDropConstaint '[Hardware].[LogisticsCosts]', '[PK_Hardware_LogisticsCosts]';
exec spDropConstaint '[Hardware].[LogisticsCosts]', 'PK_Hardware_LogisticsCosts___';
exec spDropConstaint '[Hardware].[FieldServiceCalc]', '[PK_FieldServiceCalc]';
exec spDropConstaint '[Hardware].[FieldServiceTimeCalc]', '[PK_FieldServiceTimeCalc]';
exec spDropConstaint '[Hardware].[AvailabilityFeeCalc]', '[PK_Hardware_AvailabilityFeeCalc]';

exec spDropConstaint '[Hardware].[Reinsurance]', '[PK_Hardware_Reinsurance_Id]';
exec spDropConstaint '[Hardware].[Reinsurance]', '[PK_Hardware_Reinsurance]';

exec spDropIndex '[Hardware].[FieldServiceCalc]', '[ix_FieldServiceCalc_Country]';
exec spDropIndex '[Hardware].[FieldServiceTimeCalc]', '[ix_FieldServiceTimeCalc_Country]';

exec spDropIndex '[Portfolio].[LocalPortfolio]', '[IX_LocalPortfolio_Country_Sla]';
exec spDropIndex '[Portfolio].[LocalPortfolio]', '[IX_LocalPortfolio_Sla]';

exec spDropIndex 'Fsp.HwFspCodeTranslation', 'IX_HwFspCodeTranslation_Sla';
exec spDropIndex 'Fsp.HwFspCodeTranslation', 'IX_HwFspCodeTranslation_SlaHash';
exec spDropIndex 'Fsp.HwFspCodeTranslation', 'IX_HwFspCodeTranslation_SlaHash_Sla';

exec spDropIndex '[Hardware].[Reinsurance]', '[ix_Hardware_Reinsurance_Sla]';

exec spDropColumn 'Hardware.MarkupOtherCosts', 'Deactivated';

exec spDropColumn 'Portfolio.LocalPortfolio', 'Sla';
exec spDropColumn 'Portfolio.LocalPortfolio', 'SlaHash';
exec spDropColumn 'Fsp.HwFspCodeTranslation', 'Sla';
exec spDropColumn 'Fsp.HwFspCodeTranslation', 'SlaHash';
exec spDropColumn 'Hardware.AFR', 'Deactivated';
exec spDropColumn 'Hardware.AvailabilityFee', 'Deactivated';
exec spDropColumn 'Hardware.FieldServiceCost', 'Deactivated';
exec spDropColumn 'Hardware.HddRetention', 'Deactivated';
exec spDropColumn 'Hardware.InstallBase', 'Deactivated';
exec spDropColumn 'Hardware.LogisticsCosts', 'Deactivated';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'Deactivated';
exec spDropColumn 'Hardware.MarkupStandardWaranty', 'Deactivated';
exec spDropColumn 'Hardware.MaterialCostWarranty', 'Deactivated';
exec spDropColumn 'Hardware.MaterialCostWarrantyEmeia', 'Deactivated';
exec spDropColumn 'Hardware.ProActive', 'Deactivated';
exec spDropColumn 'Hardware.Reinsurance', 'Deactivated';
exec spDropColumn 'Hardware.RoleCodeHourlyRates', 'Deactivated';
exec spDropColumn 'Hardware.ServiceSupportCost', 'Deactivated';
exec spDropColumn 'Hardware.TaxAndDuties', 'Deactivated';
exec spDropColumn 'InputAtoms.RoleCode', 'Deactivated';
exec spDropColumn 'InputAtoms.Sfab', 'Deactivated';
exec spDropColumn 'InputAtoms.Sog', 'Deactivated';
exec spDropColumn 'InputAtoms.SwDigit', 'Deactivated';
exec spDropColumn 'InputAtoms.SwLicense', 'Deactivated';
exec spDropColumn 'InputAtoms.Wg', 'Deactivated';
exec spDropColumn 'SoftwareSolution.ProActiveSw', 'Deactivated';

exec spDropColumn 'SoftwareSolution.SwSpMaintenance', 'Deactivated';
exec spDropColumn 'SoftwareSolution.SwSpMaintenance', 'TotalIB';
exec spDropColumn 'SoftwareSolution.SwSpMaintenance', 'TotalIB_Approved';

go

/*======================================================================================*/

alter table Hardware.MarkupOtherCosts
    add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

ALTER TABLE Portfolio.LocalPortfolio
    add Sla AS cast(CountryId         as nvarchar(20)) + ',' +
                cast(WgId              as nvarchar(20)) + ',' +
                cast(AvailabilityId    as nvarchar(20)) + ',' +
                cast(DurationId        as nvarchar(20)) + ',' +
                cast(ReactionTimeId    as nvarchar(20)) + ',' +
                cast(ReactionTypeId    as nvarchar(20)) + ',' +
                cast(ServiceLocationId as nvarchar(20)) + ',' +
                cast(ProactiveSlaId    as nvarchar(20)) PERSISTED not null

       , SlaHash  AS checksum (
                    cast(CountryId         as nvarchar(20)) + ',' +
                    cast(WgId              as nvarchar(20)) + ',' +
                    cast(AvailabilityId    as nvarchar(20)) + ',' +
                    cast(DurationId        as nvarchar(20)) + ',' +
                    cast(ReactionTimeId    as nvarchar(20)) + ',' +
                    cast(ReactionTypeId    as nvarchar(20)) + ',' +
                    cast(ServiceLocationId as nvarchar(20)) + ',' +
                    cast(ProactiveSlaId    as nvarchar(20)) 
                ) PERSISTED not null;
GO

CREATE NONCLUSTERED INDEX [IX_LocalPortfolio_Country_Sla] ON [Portfolio].[LocalPortfolio]
(
	[CountryId] ASC
)
INCLUDE ( 	[Id],
	[AvailabilityId],
	[DurationId],
	[ProActiveSlaId],
	[ReactionTimeId],
	[ReactionTypeId],
	[ServiceLocationId],
	[WgId],
	[ReactionTime_Avalability],
	[ReactionTime_ReactionType],
	[ReactionTime_ReactionType_Avalability],
	[Sla],
	[SlaHash]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE Fsp.HwFspCodeTranslation 
    ADD Sla  AS cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                cast(WgId                   as nvarchar(20)) + ',' +
                cast(AvailabilityId         as nvarchar(20)) + ',' +
                cast(DurationId             as nvarchar(20)) + ',' +
                cast(ReactionTimeId         as nvarchar(20)) + ',' +
                cast(ReactionTypeId         as nvarchar(20)) + ',' +
                cast(ServiceLocationId      as nvarchar(20)) + ',' +
                cast(ProactiveSlaId         as nvarchar(20)) PERSISTED not null
      
      , SlaHash  AS checksum (
                        cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                        cast(WgId                   as nvarchar(20)) + ',' +
                        cast(AvailabilityId         as nvarchar(20)) + ',' +
                        cast(DurationId             as nvarchar(20)) + ',' +
                        cast(ReactionTimeId         as nvarchar(20)) + ',' +
                        cast(ReactionTypeId         as nvarchar(20)) + ',' +
                        cast(ServiceLocationId      as nvarchar(20)) + ',' +
                        cast(ProactiveSlaId         as nvarchar(20))
                    ) PERSISTED not null;
GO

ALTER TABLE [Fsp].[HwFspCodeTranslation] alter column Name nvarchar(32) not null;
go

alter table Hardware.RoleCodeHourlyRates add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.ServiceSupportCost add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.LogisticsCosts add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.AFR add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.AvailabilityFee add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.FieldServiceCost add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.HddRetention add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.InstallBase add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.MarkupStandardWaranty add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.MaterialCostWarranty add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.MaterialCostWarrantyEmeia add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.ProActive add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.Reinsurance add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table Hardware.TaxAndDuties add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table InputAtoms.RoleCode add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table InputAtoms.Sfab add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table InputAtoms.Sog add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table InputAtoms.SwDigit add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table InputAtoms.SwLicense add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table InputAtoms.Wg add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table SoftwareSolution.ProActiveSw add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table SoftwareSolution.SwSpMaintenance 
    add 
	    TotalIB int NULL,
	    TotalIB_Approved int NULL,
        Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go
--==========================================================================================================================
CREATE CLUSTERED INDEX IX_HwFspCodeTranslation_SlaHash_Sla ON [Fsp].[HwFspCodeTranslation] (SlaHash, Sla);
GO

ALTER TABLE [Hardware].[MarkupOtherCosts] ADD CONSTRAINT [PK_Hardware_MarkupOtherCosts] PRIMARY KEY CLUSTERED 
(
    [Country] ASC,
    [Wg] ASC,
    [ReactionTimeTypeAvailability] ASC,
    [Deactivated] asc
)
GO

ALTER TABLE [Hardware].[LogisticsCosts] ADD CONSTRAINT [PK_Hardware_LogisticsCosts] PRIMARY KEY CLUSTERED 
(
    [Country] ASC,
    [Wg] ASC,
    [ReactionTimeType] ASC,
    [Deactivated] asc
)
GO

ALTER TABLE [Hardware].[FieldServiceCalc] ADD  CONSTRAINT [PK_FieldServiceCalc] PRIMARY KEY CLUSTERED 
(
    [Country] ASC,
    [Wg] ASC,
    [ServiceLocation] ASC
)
GO

ALTER TABLE [Hardware].[FieldServiceTimeCalc] ADD CONSTRAINT [PK_FieldServiceTimeCalc] PRIMARY KEY CLUSTERED 
(
    [Country] ASC,
    [Wg] ASC,
    [ReactionTimeType] ASC
)
GO

exec spDropConstaint '[Hardware].[MaterialCostWarrantyCalc]', '[PK_MaterialCostOowCalc]';
exec spDropConstaint '[Fsp].[HwStandardWarranty]', '[PK_HwStandardWarranty]';
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyCalc] ADD  CONSTRAINT [PK_MaterialCostOowCalc] PRIMARY KEY CLUSTERED ([Country] ASC,[Wg] ASC)
GO

ALTER TABLE [Fsp].[HwStandardWarranty] ADD  CONSTRAINT [PK_HwStandardWarranty] PRIMARY KEY CLUSTERED ([Country] ASC, [Wg] ASC)
GO

alter table [Hardware].[AvailabilityFeeCalc] ADD CONSTRAINT [PK_Hardware_AvailabilityFeeCalc] PRIMARY KEY CLUSTERED ([Country] ASC,[Wg] ASC)
GO

ALTER TABLE [Hardware].[ProActive] ADD CONSTRAINT [PK_Hardware_ProActive] PRIMARY KEY CLUSTERED (Country, Wg, Deactivated)
GO

ALTER TABLE [Hardware].[MarkupStandardWaranty] ADD  CONSTRAINT [PK_Hardware_MarkupStandardWaranty] PRIMARY KEY CLUSTERED ([Country] ASC,[Wg] ASC,[Deactivated] ASC)
GO

ALTER TABLE [Hardware].[Reinsurance] ADD  CONSTRAINT [PK_Hardware_Reinsurance] PRIMARY KEY CLUSTERED 
(
    [Wg] ASC,
    [ReactionTimeAvailability] ASC,
    [Duration] ASC,
    [Deactivated] ASC
)
GO



