exec spDropConstaint '[Fsp].[HwFspCodeTranslation]', '[PK_HwFspCodeTranslation]';

exec spDropConstaint '[Hardware].[MarkupOtherCosts]', '[PK_Hardware_MarkupOtherCosts_Id]';
exec spDropConstaint '[Hardware].[MarkupOtherCosts]', '[PK_Hardware_MarkupOtherCosts]';
exec spDropConstaint '[Hardware].[MarkupStandardWaranty]', '[PK_Hardware_MarkupStandardWaranty]';
exec spDropConstaint '[Hardware].[MarkupStandardWaranty]', '[PK_Hardware_MarkupStandardWaranty_Id]';

exec spDropConstaint '[Hardware].[ProActive]', '[PK_Hardware_ProActive_Id]';
exec spDropConstaint '[Hardware].[ProActive]', '[PK_Hardware_ProActive]';
exec spDropIndex '[Hardware].[ProActive]', '[ix_Hardware_ProActive]';
exec spDropIndex '[Hardware].[ProActive]', '[IX_Hardware_ProActive_Country]';


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

IF OBJECT_ID('Hardware.AFR_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AFR_Updated;
go

exec spDropTable 'Hardware.AfrYear';
go

CREATE TABLE Hardware.AfrYear(
    [Wg] [bigint] NOT NULL
  , [AFR1] [float] NULL
  , [AFR2] [float] NULL
  , [AFR3] [float] NULL
  , [AFR4] [float] NULL
  , [AFR5] [float] NULL
  , [AFRP1] [float] NULL
  , [AFR1_Approved] [float] NULL
  , [AFR2_Approved] [float] NULL
  , [AFR3_Approved] [float] NULL
  , [AFR4_Approved] [float] NULL
  , [AFR5_Approved] [float] NULL
  , [AFRP1_Approved] [float] NULL
)
GO

CREATE CLUSTERED INDEX [ix_Hardware_AfrYear] ON Hardware.AfrYear([Wg] ASC)
GO

CREATE TRIGGER Hardware.AFR_Updated
ON Hardware.AFR
After INSERT, UPDATE
AS BEGIN

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
        into #tmp
    from Hardware.AFR afr, Dependencies.Year y 
    where y.Id = afr.Year and Deactivated = 0
    group by afr.Wg;

    truncate table Hardware.AfrYear;

    insert into Hardware.AfrYear(Wg, AFR1, AFR2, AFR3, AFR4, AFR5, AFRP1, AFR1_Approved, AFR2_Approved, AFR3_Approved, AFR4_Approved, AFR5_Approved, AFRP1_Approved)
        select Wg
             , AFR1
             , AFR2
             , AFR3
             , AFR4
             , AFR5
             , AFRP1
             , AFR1_Approved
             , AFR2_Approved
             , AFR3_Approved
             , AFR4_Approved
             , AFR5_Approved
             , AFRP1_Approved
        from #tmp ;

    drop table #tmp;

END
GO

update Hardware.AFR set AFR = AFR + 0

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

