exec dbo.spDropConstaint '[History_RelatedItems].[Company]', '[FK_History_RelatedItemsCompany_Company]';
exec dbo.spDropConstaint '[History_RelatedItems].[Company]', '[FK_History_RelatedItemsCompanyCostBlockHistory_HistoryCostBlockHistory]';
exec dbo.spDropConstaint 'InputAtoms.Pla', '[FK_Pla_Company_Company]';
exec dbo.spDropConstaint '[Hardware].[AvailabilityFee]', '[FK_AvailabilityFee_Company_Company]';
exec dbo.spDropConstaint '[History].[Hardware_AvailabilityFee]', '[FK_History_Hardware_AvailabilityFeeCompany_Company]';
exec dbo.spDropColumn '[History].[Hardware_AvailabilityFee]', 'Company';
exec dbo.spDropColumn 'InputAtoms.Pla', 'CompanyId';
exec dbo.spDropColumn '[Hardware].[AvailabilityFee]', 'Company';

exec dbo.spDropTable '[History_RelatedItems].[Company]';
exec dbo.spDropTable '[InputAtoms].[Company]';
go

create table [InputAtoms].[Company](
      Id bigint not null primary key identity
    , Name nvarchar(255) not null
);
go
insert into InputAtoms.Company(name) values (('Fujitsu FBTA')), (('Fujitsu Group Companies FGC'));
go

alter table InputAtoms.Pla
    add CompanyId bigint;
go

ALTER TABLE InputAtoms.Pla WITH CHECK ADD  CONSTRAINT [FK_Pla_Company_Company] FOREIGN KEY(CompanyId)
REFERENCES [InputAtoms].[Company](Id)
go

update InputAtoms.Pla set CompanyId = (select id from InputAtoms.Company where UPPER(name) = 'Fujitsu FBTA')
    where UPPER(Name) in (
              'DESKTOP AND WORKSTATION'
            , 'NOTEBOOK AND TABLET'
            , 'RETAIL PRODUCTS'
            , 'PERIPHERALS'
        );

update InputAtoms.Pla set CompanyId = (select id from InputAtoms.Company where UPPER(name) = 'Fujitsu Group Companies FGC')
    where UPPER(Name) in (
              'STORAGE PRODUCTS'
            , 'X86 / IA SERVER'
            , 'EPS MAINFRAME PRODUCTS'
            , 'UNIX SERVER'
            , 'UNASSIGNED'
        );
go

ALTER TABLE InputAtoms.Pla ALTER COLUMN CompanyId bigint NOT NULL
go

alter table [Hardware].[AvailabilityFee]
    add Company bigint;
go

ALTER TABLE [Hardware].[AvailabilityFee]  WITH CHECK ADD  CONSTRAINT [FK_AvailabilityFee_Company_Company] FOREIGN KEY(Company)
REFERENCES [InputAtoms].[Company](Id)
GO

alter table [History].[Hardware_AvailabilityFee]
    add Company bigint;
go

ALTER TABLE [History].[Hardware_AvailabilityFee] WITH CHECK ADD  CONSTRAINT [FK_History_Hardware_AvailabilityFeeCompany_Company] FOREIGN KEY(Company)
REFERENCES [InputAtoms].[Company](Id)
GO

CREATE TABLE [History_RelatedItems].[Company](
	[CostBlockHistory] [bigint] NOT NULL,
	[Company] [bigint] NULL
)
GO

ALTER TABLE [History_RelatedItems].[Company]  WITH CHECK ADD  CONSTRAINT [FK_History_RelatedItemsCompany_Company] FOREIGN KEY([Company])
REFERENCES  [InputAtoms].[Company]([Id])
GO

ALTER TABLE [History_RelatedItems].[Company]  WITH CHECK ADD  CONSTRAINT [FK_History_RelatedItemsCompanyCostBlockHistory_HistoryCostBlockHistory] FOREIGN KEY([CostBlockHistory])
REFERENCES [History].[CostBlockHistory] ([Id])
GO

ALTER TABLE [History_RelatedItems].[Company] CHECK CONSTRAINT [FK_History_RelatedItemsCompanyCostBlockHistory_HistoryCostBlockHistory]
GO

update fee set Company = pla.CompanyId
from Hardware.AvailabilityFee fee
join InputAtoms.Wg wg on wg.id = fee.Wg
join InputAtoms.Pla pla on pla.Id = wg.PlaId
go

ALTER TABLE [Hardware].[AvailabilityFee] ALTER COLUMN Company bigint NOT NULL
go

EXEC spDropColumn '[InputAtoms].[Wg]', 'CompanyId';
go

ALTER TABLE [InputAtoms].[Wg] ADD CompanyId bigint;
go

UPDATE wg
SET CompanyId = pla.CompanyId
FROM [InputAtoms].[Wg] wg
INNER JOIN [InputAtoms].[Pla] pla ON
wg.PlaId = pla.Id
go

ALTER VIEW [InputAtoms].[WgStdView] AS
    select *
    from InputAtoms.Wg
    where Id in (select Wg from Fsp.HwStandardWarranty) and WgType = 1
GO

ALTER VIEW [InputAtoms].[WgSogView] as 
    select wg.*
         , sog.Name as Sog
         , sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
GO
