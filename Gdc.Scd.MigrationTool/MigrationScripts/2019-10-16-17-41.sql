ALTER DATABASE SCD_2 SET RECOVERY SIMPLE
GO 

if OBJECT_ID('dbo.spDropTable') is not null
    drop procedure dbo.spDropTable;
go

create procedure dbo.spDropTable(
    @tableName NVARCHAR(128)
)
as
begin

    if OBJECT_ID(@tableName) is null
        return;

    declare @sql nvarchar(255) = N'DROP TABLE ' + @tableName;
    EXEC sp_executesql @sql;

end
go

if OBJECT_ID('dbo.spDropIndex') is not null
    drop procedure dbo.spDropIndex;
go

create procedure dbo.spDropIndex(
    @tableName NVARCHAR(128),
    @indexName NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @indexName = REPLACE(REPLACE(@indexName, '[', ''), ']', '');

    if not exists(SELECT *
                FROM sys.indexes i
                WHERE i.object_id = OBJECT_ID(@tableName)
                AND i.name = @indexName)
        return;

    declare @sql nvarchar(255) = N'DROP INDEX ' + @indexName + ' ON ' + @tableName;
    EXEC sp_executesql @sql;

end
go

if OBJECT_ID('dbo.spDropColumn') is not null
    drop procedure dbo.spDropColumn;
go

create procedure dbo.spDropColumn(
    @tableName NVARCHAR(128),
    @colName   NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @colName = REPLACE(REPLACE(@colName, '[', ''), ']', '');

    if not exists(SELECT 1 FROM sys.columns WHERE Name = @colName AND Object_ID = Object_ID(@tableName))
        return;

    declare @sql nvarchar(255) = N'alter table ' + @tableName + ' drop column ' + @colName;
    EXEC sp_executesql @sql;

end
go

if OBJECT_ID('dbo.spDropConstaint') is not null
    drop procedure dbo.spDropConstaint;
go

create procedure dbo.spDropConstaint(
    @tableName    NVARCHAR(128),
    @constraint   NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @constraint = REPLACE(REPLACE(@constraint, '[', ''), ']', '');

    IF NOT EXISTS (select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS where CONSTRAINT_NAME=@constraint)
        return;

    declare @sql nvarchar(255) = N'alter table ' + @tableName + ' DROP CONSTRAINT ' + @constraint;
    EXEC sp_executesql @sql;

end
go

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
exec spDropIndex '[Hardware].[Reinsurance]', '[ix_Hardware_Reinsurance_Currency]';
exec spDropIndex '[Hardware].[Reinsurance]', '[ix_Hardware_Reinsurance_Currency_Appr]';

GO

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

alter table SoftwareSolution.SwSpMaintenance add Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

ALTER TRIGGER [Hardware].[AFR_Updated]
ON [Hardware].[AFR]
After INSERT, UPDATE
AS BEGIN

    truncate table Hardware.AfrYear;

    insert into Hardware.AfrYear(Wg, AFR1, AFR2, AFR3, AFR4, AFR5, AFRP1, AFR1_Approved, AFR2_Approved, AFR3_Approved, AFR4_Approved, AFR5_Approved, AFRP1_Approved)
        select afr.Wg
             , sum(case when y.IsProlongation = 0 and y.Value = 1 then afr.AFR / 100 end) as AFR1
             , sum(case when y.IsProlongation = 0 and y.Value = 2 then afr.AFR / 100 end) as AFR2
             , sum(case when y.IsProlongation = 0 and y.Value = 3 then afr.AFR / 100 end) as AFR3
             , sum(case when y.IsProlongation = 0 and y.Value = 4 then afr.AFR / 100 end) as AFR4
             , sum(case when y.IsProlongation = 0 and y.Value = 5 then afr.AFR / 100 end) as AFR5
             , sum(case when y.IsProlongation = 1 and y.Value = 1 then afr.AFR / 100 end) as AFRP1
             , sum(case when y.IsProlongation = 0 and y.Value = 1 then afr.AFR_Approved / 100 end) as AFR1_Approved
             , sum(case when y.IsProlongation = 0 and y.Value = 2 then afr.AFR_Approved / 100 end) as AFR2_Approved
             , sum(case when y.IsProlongation = 0 and y.Value = 3 then afr.AFR_Approved / 100 end) as AFR3_Approved
             , sum(case when y.IsProlongation = 0 and y.Value = 4 then afr.AFR_Approved / 100 end) as AFR4_Approved
             , sum(case when y.IsProlongation = 0 and y.Value = 5 then afr.AFR_Approved / 100 end) as AFR5_Approved
             , sum(case when y.IsProlongation = 1 and y.Value = 1 then afr.AFR_Approved / 100 end) as AFRP1_Approved
        from Hardware.AFR afr, Dependencies.Year y 
        where y.Id = afr.Year and afr.Deactivated = 0
        group by afr.Wg
END
go

exec spDropTable 'Hardware.AvailabilityFeeCalc';
go

CREATE TABLE Hardware.AvailabilityFeeCalc (
    [Country] [bigint] NOT NULL
  , [Wg] [bigint] NOT NULL
  , [Fee] [float] NULL
  , [Fee_Approved] [float] NULL
)
GO

CREATE CLUSTERED INDEX [ix_Hardware_AvailabilityFeeCalc] ON [Hardware].[AvailabilityFeeCalc]([Country] ASC, [Wg] ASC)
GO

IF OBJECT_ID('Hardware.UpdateAvailabilityFee') IS NOT NULL
  DROP PROCEDURE Hardware.UpdateAvailabilityFee;
go

CREATE PROCEDURE Hardware.UpdateAvailabilityFee
AS
BEGIN

    SET NOCOUNT ON;

    select Country, Wg, Fee, Fee_Approved into #tmp
    from Hardware.AvailabilityFeeCalcView fee
    join InputAtoms.Wg wg on wg.id = fee.Wg
    where wg.Deactivated = 0;


    TRUNCATE TABLE Hardware.AvailabilityFeeCalc;

    INSERT INTO Hardware.AvailabilityFeeCalc(Country, Wg, Fee, Fee_Approved)
    select Country, Wg, Fee, Fee_Approved
    from #tmp;

    drop table #tmp;
END
go

exec Hardware.UpdateAvailabilityFee;
go;

ALTER TRIGGER [Hardware].[InstallBaseUpdated]
ON [Hardware].[InstallBase]
After INSERT, UPDATE
AS BEGIN

    with cte as (
        select  ib.*

              , sum(case when c.InstallbaseGroup is null then null else ib.InstalledBaseCountry end) over(partition by c.InstallbaseGroup, ib.Wg) as InstalledBase1stLevel_Calc

              , sum(case when c.InstallbaseGroup is null then null else ib.InstalledBaseCountry_Approved end) over(partition by c.InstallbaseGroup, ib.Wg) as InstalledBase1stLevel_Calc_Approved

        from Hardware.InstallBase ib 
        join InputAtoms.Country c on c.Id = ib.Country
        JOIN InputAtoms.Wg wg on wg.id = ib.Wg

        where ib.Deactivated = 0  and wg.Deactivated = 0
    )
    update c 
        set   InstalledBase1stLevel = coalesce(InstalledBase1stLevel_Calc, InstalledBaseCountry)
            , InstalledBase1stLevel_Approved = coalesce(InstalledBase1stLevel_Calc_Approved, InstalledBaseCountry_Approved)
    from cte c;

    with ibCte as (
        select    
                  ib.*
                , pla.ClusterPlaId

                , (sum(ib.InstalledBase1stLevel) over(partition by ib.Country)) as sum_ib_cnt
                , (sum(ib.InstalledBase1stLevel_Approved) over(partition by ib.Country)) as sum_ib_cnt_Approved

                , (sum(ib.InstalledBaseCountry) over(partition by ib.Country, pla.ClusterPlaId)) as sum_ib_cnt_clusterPLA
                , (sum(ib.InstalledBaseCountry_Approved) over(partition by ib.Country, pla.ClusterPlaId)) as sum_ib_cnt_clusterPLA_Approved

                , (sum(ib.InstalledBaseCountry) over(partition by c.ClusterRegionId, cpla.Id)) as sum_ib_cnt_clusterRegion
                , (sum(ib.InstalledBaseCountry_Approved) over(partition by c.ClusterRegionId, cpla.Id)) as sum_ib_cnt_clusterRegion_Approved

        from Hardware.InstallBase ib
        JOIN InputAtoms.Country c on c.id = ib.Country
        JOIN InputAtoms.Wg wg on wg.id = ib.Wg
        JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
        JOIN InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId

        where ib.Deactivated = 0 and wg.Deactivated = 0
    )
    UPDATE ssc
            SET   ssc.TotalIb                          = ib.sum_ib_cnt
                , ssc.TotalIb_Approved                 = ib.sum_ib_cnt_Approved
                , ssc.TotalIbClusterPla                = ib.sum_ib_cnt_clusterPLA
                , ssc.TotalIbClusterPla_Approved       = ib.sum_ib_cnt_clusterPLA_Approved
                , ssc.TotalIbClusterPlaRegion          = ib.sum_ib_cnt_clusterRegion
                , ssc.TotalIbClusterPlaRegion_Approved = ib.sum_ib_cnt_clusterRegion_Approved
    from Hardware.ServiceSupportCost ssc
    join ibCte ib on ib.Country = ssc.Country and ib.ClusterPlaId = ssc.ClusterPla

END
go

ALTER TRIGGER [SoftwareSolution].[SwSpMaintenanceUpdated]
ON [SoftwareSolution].[SwSpMaintenance]
After INSERT, UPDATE
AS BEGIN

    declare @tbl table (
            SwDigit bigint primary key
        , TotalIB int
        , TotalIB_Approved int
    );

    with cte as (
        select  m.Sog 
                , m.SwDigit
                , max(m.InstalledBaseSog) as Total_InstalledBaseSog
                , max(m.InstalledBaseSog_Approved) as Total_InstalledBaseSog_Approved
        from SoftwareSolution.SwSpMaintenance m
        where m.Deactivated = 0
        group by m.Sog, m.SwDigit
    )
    insert into @tbl(SwDigit, TotalIB, TotalIB_Approved)
    select  m.SwDigit
            , sum(m.Total_InstalledBaseSog) over (partition by m.Sog) as Total_InstalledBaseSog
            , sum(m.Total_InstalledBaseSog_Approved) over (partition by m.Sog) as Total_InstalledBaseSog_Approved
    from cte m;

    update m set TotalIB = t.TotalIB, TotalIB_Approved = t.TotalIB_Approved
    from SoftwareSolution.SwSpMaintenance m 
    join @tbl t on t.SwDigit = m.SwDigit and m.Deactivated = 0

END
go

--==========================================================================================================================
ALTER TABLE [Fsp].[HwFspCodeTranslation] alter column Name nvarchar(32) not null;
go
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
GO

ALTER TABLE [Hardware].[MaterialCostWarrantyCalc] ADD  CONSTRAINT [PK_MaterialCostOowCalc] PRIMARY KEY CLUSTERED ([Country] ASC,[Wg] ASC)
GO

IF OBJECT_ID('Fsp.HwFspCodeTranslation_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Fsp.HwFspCodeTranslation_Updated;
go

IF OBJECT_ID('Fsp.spUpdateHwStandardWarranty') IS NOT NULL
  DROP procedure Fsp.spUpdateHwStandardWarranty;
go

exec spDropTable 'Fsp.HwStandardWarranty';
go

CREATE TABLE Fsp.HwStandardWarranty(
    Country bigint NOT NULL
  , Wg bigint NOT NULL 

  , FspId bigint NOT NULL
  , Fsp nvarchar(32) NOT NULL

  , AvailabilityId bigint NOT NULL 

  , DurationId bigint NOT NULL 
  , Duration nvarchar(128)
  , IsProlongation bit
  , DurationValue int

  , ReactionTimeId bigint NOT NULL 
  , ReactionTypeId bigint NOT NULL 

  , ServiceLocationId bigint NOT NULL 
  , ServiceLocation nvarchar(128)

  , ProActiveSlaId bigint NOT NULL 

  , ReactionTime_Avalability bigint
  , ReactionTime_ReactionType bigint
  , ReactionTime_ReactionType_Avalability bigint
)
GO

CREATE CLUSTERED INDEX [ix_HwStandardWarrantyCalc] ON [Fsp].[HwStandardWarranty]([Country] ASC, [Wg] ASC)
GO

CREATE procedure Fsp.spUpdateHwStandardWarranty
AS 
BEGIN

    SET NOCOUNT ON;

    with Std as (
        select  row_number() OVER(PARTITION BY fsp.CountryId, fsp.WgId ORDER BY lut.Priority) AS [rn]
              , fsp.*
        from fsp.HwFspCodeTranslation fsp
        join Fsp.LutPriority lut on lut.LUT = fsp.LUT

        where fsp.IsStandardWarranty = 1
    )
    select    fsp.CountryId
            , fsp.WgId
            , fsp.Id
            , fsp.Name
            , fsp.AvailabilityId

            , fsp.DurationId
            , dur.Name as Duration
            , dur.IsProlongation
            , dur.Value as DurationValue

            , fsp.ReactionTimeId
            , fsp.ReactionTypeId

            , fsp.ServiceLocationId
            , loc.Name as ServiceLocation
        
            , fsp.ProactiveSlaId
            , rta.Id as ReactionTime_Avalability
            , rtt.Id as ReactionTime_ReactionType
            , rtta.Id as ReactionTime_ReactionType_Avalability
    into #tmp
    from Std fsp
    INNER JOIN Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = fsp.AvailabilityId and rta.ReactionTimeId = fsp.ReactionTimeId
    INNER JOIN Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = fsp.ReactionTimeId and rtt.ReactionTypeId = fsp.ReactionTypeId
    INNER JOIN Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = fsp.AvailabilityId and rtta.ReactionTimeId = fsp.ReactionTimeId and rtta.ReactionTypeId = fsp.ReactionTypeId
    INNER JOIN Dependencies.Duration dur on dur.Id = fsp.DurationId
    INNER JOIN Dependencies.ServiceLocation loc on loc.id = fsp.ServiceLocationId
        
    where fsp.rn = 1;

    truncate table Fsp.HwStandardWarranty;

    insert into Fsp.HwStandardWarranty(
                          Country
                        , Wg
                        , FspId
                        , Fsp
                        , AvailabilityId

                        , DurationId 
                        , Duration
                        , IsProlongation
                        , DurationValue 

                        , ReactionTimeId
                        , ReactionTypeId

                        , ServiceLocationId
                        , ServiceLocation

                        , ProactiveSlaId
                        , ReactionTime_Avalability              
                        , ReactionTime_ReactionType             
                        , ReactionTime_ReactionType_Avalability)
    select    fsp.CountryId
            , fsp.WgId
            , fsp.Id
            , fsp.Name
            , fsp.AvailabilityId

            , fsp.DurationId
            , fsp.Duration
            , fsp.IsProlongation
            , fsp.DurationValue

            , fsp.ReactionTimeId
            , fsp.ReactionTypeId

            , fsp.ServiceLocationId
            , fsp.ServiceLocation
        
            , fsp.ProactiveSlaId
            , fsp.ReactionTime_Avalability
            , fsp.ReactionTime_ReactionType
            , fsp.ReactionTime_ReactionType_Avalability
    from #tmp fsp;

    drop table #tmp;

END
GO

ALTER PROCEDURE [Temp].[CopyHwFspCodeTranslations]
AS
BEGIN

	BEGIN TRY
		BEGIN TRANSACTION


		-- Disable all table constraints
		ALTER TABLE Fsp.[HwFspCodeTranslation] NOCHECK CONSTRAINT ALL;

		TRUNCATE TABLE [Fsp].[HwFspCodeTranslation];

		INSERT INTO [Fsp].[HwFspCodeTranslation]
				   ([AvailabilityId]
					   ,[CountryId]
					   ,[CreatedDateTime]
					   ,[DurationId]
					   ,[EKKey]
					   ,[EKSAPKey]
					   ,[Name]
					   ,[ProactiveSlaId]
					   ,[ReactionTimeId]
					   ,[ReactionTypeId]
					   ,[SCD_ServiceType]
					   ,[SecondSLA]
					   ,[ServiceDescription]
					   ,[ServiceLocationId]
					   ,[ServiceType]
					   ,[Status]
					   ,[WgId]
					   ,[IsStandardWarranty]
					   ,[LUT])
		SELECT      [AvailabilityId]
				   ,[CountryId]
				   ,[CreatedDateTime]
				   ,[DurationId]
				   ,[EKKey]
				   ,[EKSAPKey]
				   ,[Name]
				   ,[ProactiveSlaId]
				   ,[ReactionTimeId]
				   ,[ReactionTypeId]
				   ,[SCD_ServiceType]
				   ,[SecondSLA]
				   ,[ServiceDescription]
				   ,[ServiceLocationId]
				   ,[ServiceType]
				   ,[Status]
				   ,[WgId]
				   ,[IsStandardWarranty]
				   ,[LUT]
		 FROM [Temp].[HwFspCodeTranslation];

		-- Enable all table constraints
		ALTER TABLE Fsp.[HwFspCodeTranslation] CHECK CONSTRAINT ALL;

        exec Fsp.spUpdateHwStandardWarranty;

		COMMIT
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
	END CATCH

end
go

exec Fsp.spUpdateHwStandardWarranty;
go

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

--=================================================================
ALTER PROCEDURE [Hardware].[SpUpdateMaterialCostCalc]
AS
BEGIN

    SET NOCOUNT ON;

    truncate table Hardware.MaterialCostWarrantyCalc;

    -- Disable all table constraints
    ALTER TABLE Hardware.MaterialCostWarrantyCalc NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.MaterialCostWarrantyCalc(Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved)
        select NonEmeiaCountry as Country, Wg, MaterialCostOow, MaterialCostOow_Approved, MaterialCostIw, MaterialCostIw_Approved
        from Hardware.MaterialCostWarranty
        where Deactivated = 0

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
		  where Deactivated = 0

    -- Enable all table constraints
    ALTER TABLE Hardware.MaterialCostWarrantyCalc CHECK CONSTRAINT ALL;

END
go

ALTER FUNCTION [Hardware].[CalcStdw](
    @approved       bit = 0,
    @cnt            dbo.ListID READONLY,
    @wg             dbo.ListID READONLY
)
RETURNS @tbl TABLE  (
          CountryId                         bigint
        , Country                           nvarchar(255)
        , CurrencyId                        bigint
        , Currency                          nvarchar(255)
        , ClusterRegionId                   bigint
        , ExchangeRate                      float

        , WgId                              bigint
        , Wg                                nvarchar(255)
        , SogId                             bigint
        , Sog                               nvarchar(255)
        , ClusterPlaId                      bigint
        , RoleCodeId                        bigint

        , StdFspId                          bigint
        , StdFsp                            nvarchar(255)

        , StdWarranty                       int
        , StdWarrantyLocation               nvarchar(255)

        , AFR1                              float 
        , AFR2                              float
        , AFR3                              float
        , AFR4                              float
        , AFR5                              float
        , AFRP1                             float

        , OnsiteHourlyRates                 float
        , CanOverrideTransferCostAndPrice   bit

        --####### PROACTIVE COST ###################
        , LocalRemoteAccessSetup       float
        , LocalRegularUpdate           float
        , LocalPreparation             float
        , LocalRemoteCustomerBriefing  float
        , LocalOnsiteCustomerBriefing  float
        , Travel                       float
        , CentralExecutionReport       float

        , Fee                          float

        , MatW1                        float
        , MatW2                        float
        , MatW3                        float
        , MatW4                        float
        , MatW5                        float
        , MaterialW                    float

        , MatOow1                      float
        , MatOow2                      float
        , MatOow3                      float
        , MatOow4                      float
        , MatOow5                      float
        , MatOow1p                     float

        , MatCost1                     float
        , MatCost2                     float
        , MatCost3                     float
        , MatCost4                     float
        , MatCost5                     float
        , MatCost1P                    float

        , TaxW1                        float
        , TaxW2                        float
        , TaxW3                        float
        , TaxW4                        float
        , TaxW5                        float
        , TaxAndDutiesW                float

        , TaxOow1                      float
        , TaxOow2                      float
        , TaxOow3                      float
        , TaxOow4                      float
        , TaxOow5                      float
        , TaxOow1P                     float

        , TaxAndDuties1                float
        , TaxAndDuties2                float
        , TaxAndDuties3                float
        , TaxAndDuties4                float
        , TaxAndDuties5                float
        , TaxAndDuties1P               float

        , ServiceSupportPerYear        float
        , LocalServiceStandardWarranty float
        , LocalServiceStandardWarrantyManual float
        
        , Credit1                      float
        , Credit2                      float
        , Credit3                      float
        , Credit4                      float
        , Credit5                      float
        , Credits                      float
        
        , PRIMARY KEY CLUSTERED(CountryId, WgId)
    )
AS
BEGIN

    with WgCte as (
        select wg.Id as WgId
             , wg.Name as Wg
             , wg.SogId
             , sog.Name as Sog
             , pla.ClusterPlaId
             , wg.RoleCodeId

             , case when @approved = 0 then afr.AFR1                           else afr.AFR1_Approved                       end as AFR1 
             , case when @approved = 0 then afr.AFR2                           else afr.AFR2_Approved                       end as AFR2 
             , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
             , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
             , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
             , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1

        from InputAtoms.Wg wg
        left join InputAtoms.Sog sog on sog.Id = wg.SogId
        left join InputAtoms.Pla pla on pla.id = wg.PlaId
        left join Hardware.AfrYear afr on afr.Wg = wg.Id
        where wg.WgType = 1 and wg.Deactivated = 0 and (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    )
    , CntCte as (
        select c.Id as CountryId
             , c.Name as Country
             , c.CurrencyId
             , cur.Name as Currency
             , c.ClusterRegionId
             , c.CanOverrideTransferCostAndPrice
             , er.Value as ExchangeRate 
             , isnull(case when @approved = 0 then tax.TaxAndDuties_norm  else tax.TaxAndDuties_norm_Approved end, 0) as TaxAndDutiesOrZero

        from InputAtoms.Country c
        LEFT JOIN [References].Currency cur on cur.Id = c.CurrencyId
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDuties tax on tax.Country = c.Id and tax.Deactivated = 0
        where exists(select * from @cnt where id = c.Id)
    )
    , WgCnt as (
        select c.*, wg.*
        from CntCte c, WgCte wg
    )
    , Std as (
        select  m.*

              , case when @approved = 0 then hr.OnsiteHourlyRates                     else hr.OnsiteHourlyRates_Approved                 end / m.ExchangeRate as OnsiteHourlyRates      

              , stdw.FspId                                    as StdFspId
              , stdw.Fsp                                      as StdFsp
              , stdw.AvailabilityId                           as StdAvailabilityId 
              , stdw.Duration                                 as StdDuration
              , stdw.DurationId                               as StdDurationId
              , stdw.DurationValue                            as StdDurationValue
              , stdw.IsProlongation                           as StdIsProlongation
              , stdw.ProActiveSlaId                           as StdProActiveSlaId
              , stdw.ReactionTime_Avalability                 as StdReactionTime_Avalability
              , stdw.ReactionTime_ReactionType                as StdReactionTime_ReactionType
              , stdw.ReactionTime_ReactionType_Avalability    as StdReactionTime_ReactionType_Avalability
              , stdw.ServiceLocation                          as StdServiceLocation
              , stdw.ServiceLocationId                        as StdServiceLocationId

              , case when @approved = 0 then mcw.MaterialCostIw                      else mcw.MaterialCostIw_Approved                    end as MaterialCostWarranty
              , case when @approved = 0 then mcw.MaterialCostOow                     else mcw.MaterialCostOow_Approved                   end as MaterialCostOow     

              , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / m.ExchangeRate as MarkupStandardWarranty      
              , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end + 1              as MarkupFactorStandardWarranty

              --##### SERVICE SUPPORT COST #########                                                                                               
             , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]        else ssc.[1stLevelSupportCostsCountry_Approved]     end / m.ExchangeRate as [1stLevelSupportCosts] 
             , case when @approved = 0 
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion] end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                 end as [2ndLevelSupportCosts] 
             , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
             , case when @approved = 0
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                 end as TotalIbPla

              , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee
              , isnull(case when afEx.id is not null 
                            then (case when @approved = 0 then af.Fee else af.Fee_Approved end) 
                        end, 
                    0) as FeeOrZero

              --####### PROACTIVE COST ###################

              , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate   else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved end as LocalRemoteAccessSetup
              , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate             else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved           end as LocalRegularUpdate
              , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate                 else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved               end as LocalPreparation
              , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalRemoteCustomerBriefing
              , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalOnsiteCustomerBriefing
              , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate                            else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved                          end as Travel
              , case when @approved = 0 then pro.CentralExecutionShcReportCost                                    else pro.CentralExecutionShcReportCost_Approved                                           end as CentralExecutionReport

              --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0 
                     then fscStd.LabourCost + fscStd.TravelCost + isnull(fstStd.PerformanceRate, 0)
                     else fscStd.LabourCost_Approved + fscStd.TravelCost_Approved + isnull(fstStd.PerformanceRate_Approved, 0)
                 end / m.ExchangeRate as FieldServicePerYearStdw

               --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0
                     then lcStd.StandardHandling + lcStd.HighAvailabilityHandling + lcStd.StandardDelivery + lcStd.ExpressDelivery + lcStd.TaxiCourierDelivery + lcStd.ReturnDeliveryFactory 
                     else lcStd.StandardHandling_Approved + lcStd.HighAvailabilityHandling_Approved + lcStd.StandardDelivery_Approved + lcStd.ExpressDelivery_Approved + lcStd.TaxiCourierDelivery_Approved + lcStd.ReturnDeliveryFactory_Approved
                 end / m.ExchangeRate as LogisticPerYearStdw

              , man.StandardWarranty / m.ExchangeRate as ManualStandardWarranty

        from WgCnt m

        LEFT JOIN Hardware.RoleCodeHourlyRates hr ON hr.Country = m.CountryId and hr.RoleCode = m.RoleCodeId and hr.Deactivated = 0

        LEFT JOIN Fsp.HwStandardWarranty stdw ON stdw.Country = m.CountryId and stdw.Wg = m.WgId 

        LEFT JOIN Hardware.ServiceSupportCost ssc ON ssc.Country = m.CountryId and ssc.ClusterPla = m.ClusterPlaId and ssc.Deactivated = 0

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw ON mcw.Country = m.CountryId and mcw.Wg = m.WgId

        LEFT JOIN Hardware.MarkupStandardWaranty msw ON msw.Country = m.CountryId AND msw.Wg = m.WgId and msw.Deactivated = 0

        LEFT JOIN Hardware.AvailabilityFeeCalc af ON af.Country = m.CountryId AND af.Wg = m.WgId 
        LEFT JOIN Admin.AvailabilityFee afEx ON afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = stdw.ReactionTimeId AND afEx.ReactionTypeId = stdw.ReactionTypeId AND afEx.ServiceLocationId = stdw.ServiceLocationId

        LEFT JOIN Hardware.ProActive pro ON pro.Country= m.CountryId and pro.Wg= m.WgId and pro.Deactivated = 0

        LEFT JOIN Hardware.FieldServiceCalc fscStd     ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId 
        LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType 

        LEFT JOIN Hardware.LogisticsCosts lcStd        ON lcStd.Country  = stdw.Country AND lcStd.Wg = stdw.Wg  AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType and lcStd.Deactivated = 0

        LEFT JOIN Hardware.StandardWarrantyManualCost man on man.CountryId = m.CountryId and man.WgId = m.WgId
    )
    , CostCte as (
        select    m.*

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

        from Std m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdDurationValue >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdDurationValue >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdDurationValue >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdDurationValue >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdDurationValue >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5

                , case when m.StdDurationValue >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdDurationValue >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdDurationValue >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdDurationValue >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdDurationValue >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1                                                  as matO1P

        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdDurationValue >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdDurationValue >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdDurationValue >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdDurationValue >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdDurationValue >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5

                , case when m.StdDurationValue >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdDurationValue >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdDurationValue >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdDurationValue >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdDurationValue >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5

        from CostCte2 m
    )
    , CostCte3 as (
        select   m.*

               , case when m.StdDurationValue >= 1 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty1
               , case when m.StdDurationValue >= 2 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty2
               , case when m.StdDurationValue >= 3 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty3
               , case when m.StdDurationValue >= 4 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty4
               , case when m.StdDurationValue >= 5 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty5

        from CostCte2_2 m
    )
    insert into @tbl(
                 CountryId                    
               , Country                      
               , CurrencyId                   
               , Currency                     
               , ClusterRegionId              
               , ExchangeRate                 
               
               , WgId                         
               , Wg                           
               , SogId                        
               , Sog                          
               , ClusterPlaId                 
               , RoleCodeId                   

               , StdFspId
               , StdFsp  

               , StdWarranty         
               , StdWarrantyLocation 
               
               , AFR1                         
               , AFR2                         
               , AFR3                         
               , AFR4                         
               , AFR5                         
               , AFRP1                        

               , OnsiteHourlyRates

               , CanOverrideTransferCostAndPrice

               , LocalRemoteAccessSetup     
               , LocalRegularUpdate         
               , LocalPreparation           
               , LocalRemoteCustomerBriefing
               , LocalOnsiteCustomerBriefing
               , Travel                     
               , CentralExecutionReport     
               
               , Fee                          

               , MatW1                
               , MatW2                
               , MatW3                
               , MatW4                
               , MatW5                
               , MaterialW            
               
               , MatOow1              
               , MatOow2              
               , MatOow3              
               , MatOow4              
               , MatOow5              
               , MatOow1p             
               
               , MatCost1             
               , MatCost2             
               , MatCost3             
               , MatCost4             
               , MatCost5             
               , MatCost1P            
               
               , TaxW1                
               , TaxW2                
               , TaxW3                
               , TaxW4                
               , TaxW5                
               , TaxAndDutiesW        
               
               , TaxOow1              
               , TaxOow2              
               , TaxOow3              
               , TaxOow4              
               , TaxOow5              
               , TaxOow1P             
               
               , TaxAndDuties1        
               , TaxAndDuties2        
               , TaxAndDuties3        
               , TaxAndDuties4        
               , TaxAndDuties5        
               , TaxAndDuties1P       

               , ServiceSupportPerYear
               , LocalServiceStandardWarranty 
               , LocalServiceStandardWarrantyManual
               
               , Credit1                      
               , Credit2                      
               , Credit3                      
               , Credit4                      
               , Credit5                      
               , Credits                      
        )
    select    m.CountryId                    
            , m.Country                      
            , m.CurrencyId                   
            , m.Currency                     
            , m.ClusterRegionId              
            , m.ExchangeRate                 

            , m.WgId        
            , m.Wg          
            , m.SogId       
            , m.Sog         
            , m.ClusterPlaId
            , m.RoleCodeId  

            , m.StdFspId
            , m.StdFsp
            , m.StdDurationValue
            , m.StdServiceLocation

            , m.AFR1 
            , m.AFR2 
            , m.AFR3 
            , m.AFR4 
            , m.AFR5 
            , m.AFRP1

            , m.OnsiteHourlyRates
            , m.CanOverrideTransferCostAndPrice

            , m.LocalRemoteAccessSetup     
            , m.LocalRegularUpdate         
            , m.LocalPreparation           
            , m.LocalRemoteCustomerBriefing
            , m.LocalOnsiteCustomerBriefing
            , m.Travel                     
            , m.CentralExecutionReport     

            , m.Fee

            , m.mat1                
            , m.mat2                
            , m.mat3                
            , m.mat4                
            , m.mat5                
            , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
            
            , m.matO1              
            , m.matO2              
            , m.matO3              
            , m.matO4              
            , m.matO5              
            , m.matO1P
            
            , m.mat1  + m.matO1  as matCost1
            , m.mat2  + m.matO2  as matCost2
            , m.mat3  + m.matO3  as matCost3
            , m.mat4  + m.matO4  as matCost4
            , m.mat5  + m.matO5  as matCost5
            , m.matO1P           as matCost1P
            
            , m.tax1                
            , m.tax2                
            , m.tax3                
            , m.tax4                
            , m.tax5                
            , m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
            
            , m.TaxAndDutiesOrZero * m.matO1              
            , m.TaxAndDutiesOrZero * m.matO2              
            , m.TaxAndDutiesOrZero * m.matO3              
            , m.TaxAndDutiesOrZero * m.matO4              
            , m.TaxAndDutiesOrZero * m.matO5              
            , m.TaxAndDutiesOrZero * m.matO1P             
            
            , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
            , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
            , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
            , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
            , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
            , m.TaxAndDutiesOrZero * m.matO1P as TaxAndDuties1P

            , m.ServiceSupportPerYear

            , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
            , m.ManualStandardWarranty as LocalServiceStandardWarrantyManual

            , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
            , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
            , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
            , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
            , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5

            , m.mat1 + m.LocalServiceStandardWarranty1   +
                m.mat2 + m.LocalServiceStandardWarranty2 +
                m.mat3 + m.LocalServiceStandardWarranty3 +
                m.mat4 + m.LocalServiceStandardWarranty4 +
                m.mat5 + m.LocalServiceStandardWarranty5 as Credit

    from CostCte3 m;

    RETURN;
END
go

ALTER function [Hardware].[GetInstallBaseOverSog](
    @approved bit,
    @cnt dbo.ListID readonly
)
returns @tbl table (
      Country                      bigint
    , Sog                          bigint
    , Wg                           bigint
    , InstalledBaseCountry         float
    , InstalledBaseCountryNorm     float
    , TotalInstalledBaseCountrySog float
    , PRIMARY KEY CLUSTERED(Country, Wg)
)
begin

    with IbCte as (
        select  ib.Country
              , ib.Wg
              , wg.SogId

              , coalesce(case when @approved = 0 then ib.InstalledBaseCountry else ib.InstalledBaseCountry_Approved end, 0) as InstalledBaseCountry

        from Hardware.InstallBase ib
        join InputAtoms.Wg wg on wg.id = ib.Wg and wg.SogId is not null 

        where ib.Country in (select id from @cnt) and ib.Deactivated = 0
    )
    , IbSogCte as (
        select  ib.*
              , (sum(ib.InstalledBaseCountry) over(partition by ib.Country, ib.SogId)) as sum_ib_by_sog
        from IbCte ib
    )
    insert into @tbl
        select ib.Country
             , ib.SogId
             , ib.Wg
             , ib.InstalledBaseCountry
             , case when ib.sum_ib_by_sog > 0 then ib.InstalledBaseCountry else 1 end InstalledBaseCountryNorm_By_Sog
             , ib.sum_ib_by_sog
        from IbSogCte ib

    return;

end
go

ALTER FUNCTION [Hardware].[GetCalcMember] (
    @approved       bit,
    @cnt            dbo.ListID readonly,
    @wg             dbo.ListID readonly,
    @av             dbo.ListID readonly,
    @dur            dbo.ListID readonly,
    @reactiontime   dbo.ListID readonly,
    @reactiontype   dbo.ListID readonly,
    @loc            dbo.ListID readonly,
    @pro            dbo.ListID readonly,
    @lastid         bigint,
    @limit          int
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT    m.Id

            --SLA

            , m.CountryId          
            , std.Country
            , std.CurrencyId
            , std.Currency
            , std.ExchangeRate
            , m.WgId
            , std.Wg
            , std.SogId
            , std.Sog
            , m.DurationId
            , dur.Name             as Duration
            , dur.Value            as Year
            , dur.IsProlongation   as IsProlongation
            , m.AvailabilityId
            , av.Name              as Availability
            , m.ReactionTimeId
            , rtime.Name           as ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

            , std.StdWarranty
            , std.StdWarrantyLocation

            --Cost values

            , std.AFR1  
            , std.AFR2  
            , std.AFR3  
            , std.AFR4  
            , std.AFR5  
            , std.AFRP1 

            , std.MatCost1
            , std.MatCost2
            , std.MatCost3
            , std.MatCost4
            , std.MatCost5
            , std.MatCost1P

            , std.MatOow1 
            , std.MatOow2 
            , std.MatOow3 
            , std.MatOow4 
            , std.MatOow5 
            , std.MatOow1p

            , std.MaterialW

            , std.TaxAndDuties1
            , std.TaxAndDuties2
            , std.TaxAndDuties3
            , std.TaxAndDuties4
            , std.TaxAndDuties5
            , std.TaxAndDuties1P

            , std.TaxOow1 
            , std.TaxOow2 
            , std.TaxOow3 
            , std.TaxOow4 
            , std.TaxOow5 
            , std.TaxOow1P
            
            , std.TaxAndDutiesW

            , r.Cost as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end / std.ExchangeRate as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end / std.ExchangeRate as TravelCost             
            , case when @approved = 0 then fst.PerformanceRate                else fst.PerformanceRate_Approved            end / std.ExchangeRate as PerformanceRate        
            , case when @approved = 0 then fst.TimeAndMaterialShare_norm      else fst.TimeAndMaterialShare_norm_Approved  end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             

            , std.OnsiteHourlyRates      
                       
            --##### SERVICE SUPPORT COST #########                                                                                               
            , std.ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end / std.ExchangeRate as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end / std.ExchangeRate as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end / std.ExchangeRate as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end / std.ExchangeRate as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end / std.ExchangeRate as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end / std.ExchangeRate as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee

            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.Markup else moc.ProlongationMarkup end)                             
                    else (case when dur.IsProlongation = 0 then moc.Markup_Approved else moc.ProlongationMarkup_Approved end)                      
                end / std.ExchangeRate as MarkupOtherCost                      
            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm else moc.ProlongationMarkupFactor_norm end)                             
                    else (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm_Approved else moc.ProlongationMarkupFactor_norm_Approved end)                      
                end as MarkupFactorOtherCost                

            --####### PROACTIVE COST ###################
            , std.LocalRemoteAccessSetup
            , std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                 as LocalRegularUpdate
            , std.LocalPreparation * proSla.LocalPreparationShcRepetition                       as LocalPreparation
            , std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition as LocalRemoteCustomerBriefing
            , std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition as LocalOnsiteCustomerBriefing
            , std.Travel * proSla.TravellingTimeRepetition                                      as Travel
            , std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition           as CentralExecutionReport

            , std.LocalServiceStandardWarranty
            , std.LocalServiceStandardWarrantyManual
            , std.Credit1
            , std.Credit2
            , std.Credit3
            , std.Credit4
            , std.Credit5
            , std.Credits

            --########## MANUAL COSTS ################
            , man.ListPrice          / std.ExchangeRate as ListPrice                   
            , man.DealerDiscount                        as DealerDiscount              
            , man.DealerPrice        / std.ExchangeRate as DealerPrice                 
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / std.ExchangeRate) end as ServiceTCManual                   
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTP     / std.ExchangeRate) end as ServiceTPManual                   
            , man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  
            , man.ReleaseDate                           as ReleaseDate
            , man.ChangeDate                            
            , u.Name                                    as ChangeUserName
            , u.Email                                   as ChangeUserEmail

    FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    INNER JOIN Hardware.CalcStdw(@approved, @cnt, @wg) std on std.CountryId = m.CountryId and std.WgId = m.WgId

    LEFT JOIN Hardware.GetReinsurance(@approved) r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId
)
go

ALTER FUNCTION [SoftwareSolution].[GetSwSpMaintenancePaging] (
    @approved bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS @tbl TABLE 
        (   
            [rownum] [int] NOT NULL,
            [Id] [bigint] NOT NULL,
            [Pla] [bigint] NOT NULL,
            [Sfab] [bigint] NOT NULL,
            [Sog] [bigint] NOT NULL,
            [SwDigit] [bigint] NOT NULL,
            [Availability] [bigint] NOT NULL,
            [Year] [bigint] NOT NULL,
            [2ndLevelSupportCosts] [float] NULL,
            [InstalledBaseSog] [float] NULL,
            [TotalInstalledBaseSog] [float] NULL,
            [ReinsuranceFlatfee] [float] NULL,
            [CurrencyReinsurance] [bigint] NULL,
            [RecommendedSwSpMaintenanceListPrice] [float] NULL,
            [MarkupForProductMarginSwLicenseListPrice] [float] NULL,
            [ShareSwSpMaintenanceListPrice] [float] NULL,
            [DiscountDealerPrice] [float] NULL
        )
AS
BEGIN
        declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
        declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
        declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

        if @limit > 0
        begin
            with cte as (
                select ROW_NUMBER() over(
                            order by ssm.SwDigit
                                   , ya.AvailabilityId
                                   , ya.YearId
                        ) as rownum
                      , ssm.*
                      , ya.AvailabilityId
                      , ya.YearId
                FROM SoftwareSolution.SwSpMaintenance ssm
                JOIN Dependencies.Duration_Availability ya on ya.Id = ssm.DurationAvailability
                WHERE (@isEmptyDigit = 1 or ssm.SwDigit in (select id from @digit))
                    AND (@isEmptyAV = 1 or ya.AvailabilityId in (select id from @av))
                    AND (@isEmptyYear = 1 or ya.YearId in (select id from @year))
                    and ssm.Deactivated = 0
            )
            insert @tbl
            select top(@limit)
                    rownum
                  , ssm.Id
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Sog
                  , ssm.SwDigit
                  , ssm.AvailabilityId
                  , ssm.YearId
              
                  , case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
                  , case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
                  , case when @approved = 0 then ssm.TotalIB else ssm.TotalIB_Approved end

                  , case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
                  , case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end
                  , case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
                  , case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

            from cte ssm where rownum > @lastid
        end
    else
        begin
            insert @tbl
            select -1 as rownum
                  , ssm.Id
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Sog
                  , ssm.SwDigit
                  , ya.AvailabilityId
                  , ya.YearId

                  , case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
                  , case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
                  , case when @approved = 0 then ssm.TotalIB else ssm.TotalIB_Approved end

                  , case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
                  , case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end
                  , case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
                  , case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

            FROM SoftwareSolution.SwSpMaintenance ssm
            JOIN Dependencies.Duration_Availability ya on ya.Id = ssm.DurationAvailability

            WHERE (@isEmptyDigit = 1 or ssm.SwDigit in (select id from @digit))
                AND (@isEmptyAV = 1 or ya.AvailabilityId in (select id from @av))
                AND (@isEmptyYear = 1 or ya.YearId in (select id from @year))
                and ssm.Deactivated = 0

        end

    RETURN;
END
go

ALTER FUNCTION [SoftwareSolution].[GetProActivePaging] (
     @approved bit,
     @cnt dbo.ListID readonly,
     @digit dbo.ListID readonly,
     @av dbo.ListID readonly,
     @year dbo.ListID readonly,
     @lastid bigint,
     @limit int
)
RETURNS @tbl TABLE 
        (   
            rownum                                  int NOT NULL,
            Id                                      bigint,
            Country                                 bigint,
            Pla                                     bigint,
            Sog                                     bigint,
                                                    
            SwDigit                                 bigint,
                                                    
            FspId                                   bigint,
            Fsp                                     nvarchar(30),
            FspServiceDescription                   nvarchar(max),
            AvailabilityId                          bigint,
            DurationId                              bigint,
            ReactionTimeId                          bigint,
            ReactionTypeId                          bigint,
            ServiceLocationId                       bigint,
            ProactiveSlaId                          bigint,

            LocalRemoteAccessSetupPreparationEffort float,
            LocalRegularUpdateReadyEffort           float,
            LocalPreparationShcEffort               float,
            CentralExecutionShcReportCost           float,
            LocalRemoteShcCustomerBriefingEffort    float,
            LocalOnSiteShcCustomerBriefingEffort    float,
            TravellingTime                          float,
            OnSiteHourlyRate                        float
        )
AS
BEGIN
		declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
		declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
		declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
		declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

        if @limit > 0
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
				where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
					AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
					AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
            )
            , cte as (
                select ROW_NUMBER() over(
                            order by
                               pro.SwDigit
                             , fsp.AvailabilityId
                             , fsp.DurationId
                             , fsp.ReactionTimeId
                             , fsp.ReactionTypeId
                             , fsp.ServiceLocationId
                             , fsp.ProactiveSlaId
                         ) as rownum
                     , pro.Id
                     , pro.Country
                     , pro.Pla
                     , pro.Sog

                     , pro.SwDigit

                     , fsp.id as FspId
                     , fsp.Name as Fsp
                     , fsp.ServiceDescription as FspServiceDescription
                     , fsp.AvailabilityId
                     , fsp.DurationId
                     , fsp.ReactionTimeId
                     , fsp.ReactionTypeId
                     , fsp.ServiceLocationId
                     , fsp.ProactiveSlaId

                     , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                     , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                     , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                     , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                     , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                     , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                    FROM SoftwareSolution.ProActiveSw pro
                    LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

				    WHERE pro.Deactivated = 0
                    and (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
				    AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
					AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))

            )
            INSERT @tbl
            SELECT *
            from cte pro where pro.rownum > @lastid
        end
    else
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
				where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
				AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
				AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
            )
            INSERT @tbl
            SELECT -1 as rownum
                 , pro.Id
                 , pro.Country
                 , pro.Pla
                 , pro.Sog

                 , pro.SwDigit

                 , fsp.id as FspId
                 , fsp.Name as Fsp
                 , fsp.ServiceDescription as FspServiceDescription
                 , fsp.AvailabilityId
                 , fsp.DurationId
                 , fsp.ReactionTimeId
                 , fsp.ReactionTypeId
                 , fsp.ServiceLocationId
                 , fsp.ProactiveSlaId

                 , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                 , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                 , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                 , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                 , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                 , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                FROM SoftwareSolution.ProActiveSw pro
                LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

				WHERE pro.Deactivated = 0
                and (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
				AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
				AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))

        end

    RETURN;
END
go

ALTER function [Report].[GetReinsuranceYear](@approved bit)
returns @tbl table (
    [Wg] [bigint] NOT NULL PRIMARY KEY,
    [ReinsuranceFlatfee1] [float] NULL,
    [ReinsuranceFlatfee2] [float] NULL,
    [ReinsuranceFlatfee3] [float] NULL,
    [ReinsuranceFlatfee4] [float] NULL,
    [ReinsuranceFlatfee5] [float] NULL,
    [ReinsuranceFlatfeeP1] [float] NULL,
    [ReinsuranceUpliftFactor_NBD_9x5] [float] NULL,
    [ReinsuranceUpliftFactor_4h_9x5] [float] NULL,
    [ReinsuranceUpliftFactor_4h_24x7] [float] NULL
)
begin

    declare @NBD_9x5 bigint = (select id 
            from Dependencies.ReactionTime_Avalability
            where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = 'NBD')
                and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')
        );

    declare @4h_9x5 bigint = (select id 
            from Dependencies.ReactionTime_Avalability
            where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
                and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')
        );

    declare @4h_24x7 bigint = (select id 
            from Dependencies.ReactionTime_Avalability
            where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
                and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '24X7')
        );

    declare @exchange_rate table (
        CurrencyId [bigint],
        Value [float] NULL
    );

    insert into @exchange_rate(CurrencyId, Value)
    select cur.Id, er.Value
    from [References].Currency cur
    join [References].ExchangeRate er on er.CurrencyId = cur.Id;

    if @approved = 0
        with cte as (
            select r.Wg
                 , d.Value as Duration
                 , d.IsProlongation 
                 , r.ReactionTimeAvailability
                 , r.ReinsuranceFlatfee / er.Value as ReinsuranceFlatfee
                 , r.ReinsuranceUpliftFactor       
            from Hardware.Reinsurance r
            join Dependencies.Duration d on d.Id = r.Duration
            left join @exchange_rate er on er.CurrencyId = r.CurrencyReinsurance

            where   r.ReactionTimeAvailability in (@NBD_9x5, @4h_9x5, @4h_24x7) 
                and r.Deactivated = 0
        )
        INSERT INTO @tbl(Wg
                   
                       , ReinsuranceFlatfee1                     
                       , ReinsuranceFlatfee2                     
                       , ReinsuranceFlatfee3                     
                       , ReinsuranceFlatfee4                     
                       , ReinsuranceFlatfee5                     
                       , ReinsuranceFlatfeeP1                    
                   
                       , ReinsuranceUpliftFactor_NBD_9x5         
                       , ReinsuranceUpliftFactor_4h_9x5          
                       , ReinsuranceUpliftFactor_4h_24x7)
        select    r.Wg

                , max(case when r.IsProlongation = 0 and r.Duration = 1  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 2  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 3  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 4  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 5  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 1 and r.Duration = 1  then ReinsuranceFlatfee end) 

                , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor end) 

        from cte r
        group by r.Wg;
    else
        with cte as (
            select r.Wg
                 , d.Value as Duration
                 , d.IsProlongation 
                 , r.ReactionTimeAvailability
                 , r.ReinsuranceFlatfee_Approved / er.Value as ReinsuranceFlatfee
                 , r.ReinsuranceUpliftFactor_Approved       as ReinsuranceUpliftFactor
            from Hardware.Reinsurance r
            join Dependencies.Duration d on d.Id = r.Duration
            left join @exchange_rate er on er.CurrencyId = r.CurrencyReinsurance_Approved

            where   r.ReactionTimeAvailability in (@NBD_9x5, @4h_9x5, @4h_24x7) 
                and r.Deactivated = 0
        )
        INSERT INTO @tbl(Wg
                   
                       , ReinsuranceFlatfee1                     
                       , ReinsuranceFlatfee2                     
                       , ReinsuranceFlatfee3                     
                       , ReinsuranceFlatfee4                     
                       , ReinsuranceFlatfee5                     
                       , ReinsuranceFlatfeeP1                    
                   
                       , ReinsuranceUpliftFactor_NBD_9x5         
                       , ReinsuranceUpliftFactor_4h_9x5          
                       , ReinsuranceUpliftFactor_4h_24x7)
        select    r.Wg

                , max(case when r.IsProlongation = 0 and r.Duration = 1  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 2  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 3  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 4  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 0 and r.Duration = 5  then ReinsuranceFlatfee end) 
                , max(case when r.IsProlongation = 1 and r.Duration = 1  then ReinsuranceFlatfee end) 

                , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor end) 
                , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor end) 

        from cte r
        group by r.Wg;

    return;
end
go

if OBJECT_ID('[Report].[GetParameterStd]') is not null
    drop function [Report].[GetParameterStd];
go

create function [Report].[GetParameterStd]
(
    @approved bit,
    @cnt bigint,
    @wg bigint
)
RETURNS @result table(
      CountryId                         bigint
    , Country                           nvarchar(255)
    , Currency                          nvarchar(255)
    , ExchangeRate                      float
    , TaxAndDuties                      float
                                        
    , WgId                              bigint
    , Wg                                nvarchar(255)   
    , WgDescription                     nvarchar(255)
    , SCD_ServiceType                   nvarchar(255)
    , SogDescription                    nvarchar(255)
    , RoleCodeId                        bigint
    , AFR1                              float  
    , AFR2                              float
    , AFR3                              float
    , AFR4                              float
    , AFR5                              float
    , AFRP1                             float
    , ReinsuranceFlatfee1               float
    , ReinsuranceFlatfee2               float
    , ReinsuranceFlatfee3               float
    , ReinsuranceFlatfee4               float
    , ReinsuranceFlatfee5               float
    , ReinsuranceFlatfeeP1              float
    , ReinsuranceUpliftFactor_4h_24x7   float
    , ReinsuranceUpliftFactor_4h_9x5    float
    , ReinsuranceUpliftFactor_NBD_9x5   float
    , [1stLevelSupportCosts]            float
    , [2ndLevelSupportCosts]            float
    , MaterialCostWarranty              float
    , MaterialCostOow                   float
    , OnsiteHourlyRate                  float
    , Fee                               float
    , MarkupFactorStandardWarranty      float
    , MarkupStandardWarranty            float
    , primary key clustered (CountryId, WgId) 
)
as
begin

    with WgCte as (
        select wg.Id
             , wg.Name
             , wg.Description
             , wg.SCD_ServiceType
             , pla.ClusterPlaId
             , sog.Description as SogDescription
             , wg.RoleCodeId
        
             , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                               end as AFR1 
             , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                               end as AFR2 
             , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                           end as AFR3 
             , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                           end as AFR4 
             , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                           end as AFR5 
             , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                          end as AFRP1

             , r.ReinsuranceFlatfee1              
             , r.ReinsuranceFlatfee2              
             , r.ReinsuranceFlatfee3              
             , r.ReinsuranceFlatfee4              
             , r.ReinsuranceFlatfee5              
             , r.ReinsuranceFlatfeeP1             
             , r.ReinsuranceUpliftFactor_4h_24x7  
             , r.ReinsuranceUpliftFactor_4h_9x5   
             , r.ReinsuranceUpliftFactor_NBD_9x5  

             , case when @approved = 0 then ssc.[1stLevelSupportCosts] else ssc.[1stLevelSupportCosts_Approved] end as [1stLevelSupportCosts]
             , case when @approved = 0 then ssc.[2ndLevelSupportCosts] else ssc.[2ndLevelSupportCosts_Approved] end as [2ndLevelSupportCosts]

             , case when @approved = 0 then mcw.MaterialCostIw else mcw.MaterialCostIw_Approved end as MaterialCostWarranty
             , case when @approved = 0 then mcw.MaterialCostOow else mcw.MaterialCostOow_Approved end as MaterialCostOow

             , case when @approved = 0 then hr.OnsiteHourlyRates else hr.OnsiteHourlyRates_Approved end as OnsiteHourlyRate

             , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee

             , case when @approved = 0 then msw.MarkupFactorStandardWarranty else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty

             , case when @approved = 0 then msw.MarkupStandardWarranty else msw.MarkupStandardWarranty_Approved end       as MarkupStandardWarranty

        from InputAtoms.Wg wg 

        INNER JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
        
        LEFT JOIN InputAtoms.Sog sog on sog.id = wg.SogId
        
        LEFT JOIN Hardware.AfrYear afr on afr.Wg = wg.Id
        
        LEFT JOIN Report.GetReinsuranceYear(@approved) r on r.Wg = wg.Id

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw on mcw.Country = @cnt and mcw.Wg = wg.Id

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = @cnt and ssc.ClusterPla = pla.ClusterPlaId

        LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.Country = @cnt and hr.RoleCode = wg.RoleCodeId 

        LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Country = @cnt AND msw.Wg = wg.Id and msw.Deactivated = 0

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = @cnt AND af.Wg = wg.Id

        where wg.Deactivated = 0 and (@wg is null or wg.Id = @wg)
    )
    , CountryCte as (
        select c.Id
             , c.Name
             , cur.Name as Currency
             , er.Value as ExchangeRate
             , case when @approved = 0 then tax.TaxAndDuties else tax.TaxAndDuties_Approved end as TaxAndDuties
        from InputAtoms.Country c 
        INNER JOIN [References].Currency cur on cur.Id = c.CurrencyId
        INNER JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDuties tax on tax.Country = c.Id and tax.Deactivated = 0
        where c.Id = @cnt
    )
    insert into @result
    select 
                c.Id
              , c.Name
              , Currency                         
              , ExchangeRate                     
              , TaxAndDuties                     
                                                 
              , wg.Id                             
              , wg.Name                               
              , wg.Description                    
              , SCD_ServiceType                  
              , SogDescription                   
              , RoleCodeId                       
              , AFR1                             
              , AFR2                             
              , AFR3                             
              , AFR4                             
              , AFR5                             
              , AFRP1                            
              , ReinsuranceFlatfee1              
              , ReinsuranceFlatfee2              
              , ReinsuranceFlatfee3              
              , ReinsuranceFlatfee4              
              , ReinsuranceFlatfee5              
              , ReinsuranceFlatfeeP1             
              , ReinsuranceUpliftFactor_4h_24x7  
              , ReinsuranceUpliftFactor_4h_9x5   
              , ReinsuranceUpliftFactor_NBD_9x5  
              , [1stLevelSupportCosts]           
              , [2ndLevelSupportCosts]           
              , MaterialCostWarranty             
              , MaterialCostOow                  
              , OnsiteHourlyRate                 
              , Fee                              
              , MarkupFactorStandardWarranty     
              , MarkupStandardWarranty           

    from WgCte wg, CountryCte c;

    return
end

go

if OBJECT_ID('[Report].[GetParameterHw]') is not null
    drop function [Report].[GetParameterHw];
go

create function [Report].[GetParameterHw]
(
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @duration     bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    with CostCte as (
            select 
                m.Id
                , m.CountryId
                , std.Country
                , std.WgDescription
                , std.Wg
                , std.SogDescription
                , std.SCD_ServiceType
                , pro.ExternalName as Sla
                , loc.Name as ServiceLocation
                , rtime.Name as ReactionTime
                , rtype.Name as ReactionType
                , av.Name as Availability
                , std.Currency
                , std.ExchangeRate

                --FSP
                , fsp.Name Fsp
                , fsp.ServiceDescription as FspDescription

                --cost blocks

                , case when @approved = 0 then fsc.LabourCost else fsc.LabourCost_Approved end as LabourCost
                , case when @approved = 0 then fsc.TravelCost else fsc.TravelCost_Approved end as TravelCost
                , case when @approved = 0 then fst.PerformanceRate else fst.PerformanceRate_Approved end as PerformanceRate
                , case when @approved = 0 then fsc.TravelTime else fsc.TravelTime_Approved end as TravelTime
                , case when @approved = 0 then fsc.RepairTime else fsc.RepairTime_Approved end as RepairTime
                , std.OnsiteHourlyRate


                , case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end         as StandardHandling
                , case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling 
                , case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end         as StandardDelivery
                , case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end          as ExpressDelivery
                , case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end      as TaxiCourierDelivery
                , case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end    as ReturnDeliveryFactory 
                
                , case when @approved = 0 
                        then lc.StandardHandling + lc.HighAvailabilityHandling 
                        else lc.StandardHandling_Approved + lc.HighAvailabilityHandling_Approved 
                    end as LogisticHandlingPerYear

                , case when @approved = 0 
                        then lc.StandardDelivery + lc.ExpressDelivery + lc.TaxiCourierDelivery + lc.ReturnDeliveryFactory 
                        else lc.StandardDelivery_Approved + lc.ExpressDelivery_Approved + lc.TaxiCourierDelivery_Approved + lc.ReturnDeliveryFactory_Approved 
                    end as LogisticTransportPerYear

                , case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee
      
                , std.TaxAndDuties as TaxAndDutiesW

                , case when @approved = 0 then moc.Markup else moc.Markup_Approved end as MarkupOtherCost                      
                , case when @approved = 0 then moc.MarkupFactor_norm else moc.MarkupFactor_norm_Approved end as MarkupFactorOtherCost                

                , std.MarkupFactorStandardWarranty
                , std.MarkupStandardWarranty
      
                , std.AFR1
                , std.AFR2
                , std.AFR3
                , std.AFR4
                , std.AFR5
                , std.AFRP1

                , case when dur.Value = 1 then std.AFR1 
                       when dur.Value = 2 then std.AFR1 + std.AFR2 
                       when dur.Value = 3 then std.AFR1 + std.AFR2 + std.AFR3 
                       when dur.Value = 4 then std.AFR1 + std.AFR2 + std.AFR3 + std.AFR4 
                       when dur.Value = 5 then std.AFR1 + std.AFR2 + std.AFR3 + std.AFR4 + std.AFR5
                    end AfrSum

                , case when @approved = 0 
                        then Hardware.CalcFieldServiceCost(
                                                  fst.TimeAndMaterialShare_norm 
                                                , fsc.TravelCost                
                                                , fsc.LabourCost                
                                                , fst.PerformanceRate           
                                                , fsc.TravelTime                
                                                , fsc.RepairTime                
                                                , std.OnsiteHourlyRate
                                                , 1
                                            ) 
                        else Hardware.CalcFieldServiceCost(
                                              fst.TimeAndMaterialShare_norm_Approved  
                                            , fsc.TravelCost_Approved  
                                            , fsc.LabourCost_Approved  
                                            , fst.PerformanceRate_Approved  
                                            , fsc.TravelTime_Approved  
                                            , fsc.RepairTime_Approved  
                                            , std.OnsiteHourlyRate 
                                            , 1
                                        )
                    end as FieldServicePerYear

                , std.[1stLevelSupportCosts]
                , std.[2ndLevelSupportCosts]
           
                , std.ReinsuranceFlatfee1
                , std.ReinsuranceFlatfee2
                , std.ReinsuranceFlatfee3
                , std.ReinsuranceFlatfee4
                , std.ReinsuranceFlatfee5
                , std.ReinsuranceFlatfeeP1
                , std.ReinsuranceUpliftFactor_4h_24x7
                , std.ReinsuranceUpliftFactor_4h_9x5
                , std.ReinsuranceUpliftFactor_NBD_9x5

                , std.MaterialCostWarranty
                , std.MaterialCostOow

                , dur.Value as Duration
                , dur.IsProlongation

        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId 

        INNER JOIN Report.GetParameterStd(@approved, @cnt, @wg) std on std.CountryId = m.CountryId and std.WgId = m.WgId

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
        LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId and fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

        LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTimeType = m.ReactionTime_ReactionType
                                            AND lc.Deactivated = 0

        LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId 
                                               and moc.Wg = m.WgId 
                                               AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability 
                                               and moc.Deactivated = 0

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId 
                                            AND afEx.ReactionTimeId = m.ReactionTimeId 
                                            AND afEx.ReactionTypeId = m.ReactionTypeId 
                                            AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    
                m.Id
              , m.Country
              , m.WgDescription
              , m.Wg
              , m.SogDescription
              , m.SCD_ServiceType
              , m.Sla
              , m.ServiceLocation
              , m.ReactionTime
              , m.ReactionType
              , m.Availability

             , m.Currency

             --FSP
              , m.Fsp
              , m.FspDescription

              --cost blocks

              , m.LabourCost as LabourCost
              , m.TravelCost as TravelCost
              , m.PerformanceRate as PerformanceRate
              , m.TravelTime
              , m.RepairTime
              , m.OnsiteHourlyRate as OnsiteHourlyRate

              , m.AvailabilityFee * m.ExchangeRate as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
      
              , m.AFR1   * 100 as AFR1
              , m.AFR2   * 100 as AFR2
              , m.AFR3   * 100 as AFR3
              , m.AFR4   * 100 as AFR4
              , m.AFR5   * 100 as AFR5
              , m.AFRP1  * 100 as AFRP1

              , m.[1stLevelSupportCosts] * m.ExchangeRate as [1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts] * m.ExchangeRate as [2ndLevelSupportCosts]
           
              , m.ReinsuranceFlatfee1 * m.ExchangeRate as ReinsuranceFlatfee1
              , m.ReinsuranceFlatfee2 * m.ExchangeRate as ReinsuranceFlatfee2
              , m.ReinsuranceFlatfee3 * m.ExchangeRate as ReinsuranceFlatfee3
              , m.ReinsuranceFlatfee4 * m.ExchangeRate as ReinsuranceFlatfee4
              , m.ReinsuranceFlatfee5 * m.ExchangeRate as ReinsuranceFlatfee5
              , m.ReinsuranceFlatfeeP1 * m.ExchangeRate as ReinsuranceFlatfeeP1
              , m.ReinsuranceUpliftFactor_4h_24x7 as ReinsuranceUpliftFactor_4h_24x7
              , m.ReinsuranceUpliftFactor_4h_9x5 as ReinsuranceUpliftFactor_4h_9x5
              , m.ReinsuranceUpliftFactor_NBD_9x5 as ReinsuranceUpliftFactor_NBD_9x5

              , m.MaterialCostWarranty * m.ExchangeRate as MaterialCostWarranty
              , m.MaterialCostOow * m.ExchangeRate as MaterialCostOow

              , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Duration as varchar(1)) end as Duration

              , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
              , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
              , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
              , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
              , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
              , m.FieldServicePerYear * m.AFRP1 as FieldServiceCostP1
            
              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , m.LogisticHandlingPerYear * m.AfrSum as LogisticsHandling

              , m.LogisticTransportPerYear * m.AfrSum as LogisticTransportcost

    from CostCte m
)
go

ALTER PROCEDURE [Report].[spProActive]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       bigint,
    @limit        int
)
AS
BEGIN

    declare @cntGroup nvarchar(255) = (select Name from InputAtoms.CountryGroup where Id = (select CountryGroupId from InputAtoms.Country where id = @cnt))

    declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where SogId in (select wg.SogId from InputAtoms.Wg wg where @wg is null or wg.Id = @wg)
        and IsSoftware = 0
        and SogId is not null
        and Deactivated = 0;

    if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

    declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSog(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable) m
        where @wg is null or m.WgId = @wg
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as FspDescription

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    m.Id
            , m.Country
            , @cntGroup as CountryGroup
            , m.Fsp
            , m.Wg

            , m.ServiceLocation
            , m.ReactionTime
            , m.ReactionType
            , m.Availability
            , m.ProActiveSla

            , m.ServicePeriod as Duration

             , m.ServiceTpSog * m.ExchangeRate as ReActive
             , m.ProActiveSog * m.ExchangeRate as ProActive
             , (m.ServiceTpSog + coalesce(m.ProActiveSog, 0)) * m.ExchangeRate as ServiceTP

            , m.Currency

            , sog.Name as Sog
            , sog.Description as SogDescription

            , m.FspDescription
    from cte2 m
    JOIN InputAtoms.Sog sog on Sog.Id = m.SogId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
go

ALTER VIEW [InputAtoms].[WgStdView] AS
    select *
    from InputAtoms.Wg
    where Id in (select Wg from Fsp.HwStandardWarranty) and WgType = 1

GO

IF OBJECT_ID('Hardware.GetReinsurance') IS NOT NULL
  DROP FUNCTION Hardware.GetReinsurance;
GO


ALTER DATABASE SCD_2 SET RECOVERY FULL
GO 
