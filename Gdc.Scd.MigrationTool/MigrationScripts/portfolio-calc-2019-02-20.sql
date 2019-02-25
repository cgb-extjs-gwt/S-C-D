ALTER VIEW [InputAtoms].[WgView] WITH SCHEMABINDING as
    SELECT wg.Id, 
           wg.Name, 
           case 
                when wg.WgType = 0 then 1
                else 0
            end as IsMultiVendor, 
           pla.Id as Pla, 
           cpla.Id as ClusterPla,
           wg.RoleCodeId
    from InputAtoms.Wg wg
    inner join InputAtoms.Pla pla on pla.id = wg.PlaId
    inner join InputAtoms.ClusterPla cpla on cpla.id = pla.ClusterPlaId
    where wg.WgType = 1 and wg.DeactivatedDateTime is null
GO

ALTER TABLE Portfolio.LocalPortfolio drop column ReactionTime_Avalability;
go
ALTER TABLE Portfolio.LocalPortfolio drop column ReactionTime_ReactionType ;
go
ALTER TABLE Portfolio.LocalPortfolio drop column ReactionTime_ReactionType_Avalability ;
go
ALTER TABLE Portfolio.LocalPortfolio drop column SlaHash;
go
ALTER TABLE Portfolio.LocalPortfolio drop column Sla;
go

ALTER TABLE Portfolio.LocalPortfolio
    add  ReactionTime_Avalability              bigint
       , ReactionTime_ReactionType             bigint
       , ReactionTime_ReactionType_Avalability bigint
       , Sla AS cast(CountryId         as nvarchar(20)) + 
                    cast(WgId              as nvarchar(20)) + 
                    cast(AvailabilityId    as nvarchar(20)) + 
                    cast(DurationId        as nvarchar(20)) + 
                    cast(ReactionTimeId    as nvarchar(20)) + 
                    cast(ReactionTypeId    as nvarchar(20)) + 
                    cast(ServiceLocationId as nvarchar(20)) + 
                    cast(ProactiveSlaId    as nvarchar(20))

       , SlaHash  AS checksum (
                    cast(CountryId         as nvarchar(20)) + 
                    cast(WgId              as nvarchar(20)) + 
                    cast(AvailabilityId    as nvarchar(20)) + 
                    cast(DurationId        as nvarchar(20)) + 
                    cast(ReactionTimeId    as nvarchar(20)) + 
                    cast(ReactionTypeId    as nvarchar(20)) + 
                    cast(ServiceLocationId as nvarchar(20)) + 
                    cast(ProactiveSlaId    as nvarchar(20)) 
                );
GO

update p set  ReactionTime_Avalability = rta.Id
            , ReactionTime_ReactionType = rtt.Id
            , ReactionTime_ReactionType_Avalability = rtta.Id
from Portfolio.LocalPortfolio p
join Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = p.AvailabilityId and rta.ReactionTimeId = p.ReactionTimeId
join Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = p.ReactionTimeId and rtt.ReactionTypeId = p.ReactionTypeId
join Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = p.AvailabilityId and rtta.ReactionTimeId = p.ReactionTimeId and rtta.ReactionTypeId = p.ReactionTypeId

go

alter table Hardware.TaxAndDuties drop column TaxAndDuties_norm;
alter table Hardware.TaxAndDuties drop column TaxAndDuties_norm_Approved;
GO

alter table Hardware.TaxAndDuties
    add TaxAndDuties_norm          as (TaxAndDuties / 100)
      , TaxAndDuties_norm_Approved as (TaxAndDuties_Approved / 100)
GO

ALTER VIEW [Hardware].[TaxAndDutiesView] as
    select Country,
           TaxAndDuties_norm, 
           TaxAndDuties_norm_Approved,
           TaxAndDuties_norm as TaxAndDuties, 
           TaxAndDuties_norm_Approved as TaxAndDuties_Approved
    from Hardware.TaxAndDuties
    where DeactivatedDateTime is null
GO

alter table Hardware.MarkupStandardWaranty drop column MarkupFactorStandardWarranty_norm;
alter table Hardware.MarkupStandardWaranty drop column MarkupFactorStandardWarranty_norm_Approved;
GO

alter table Hardware.MarkupStandardWaranty
    add MarkupFactorStandardWarranty_norm          as (MarkupFactorStandardWarranty / 100)
      , MarkupFactorStandardWarranty_norm_Approved as (MarkupFactorStandardWarranty_Approved / 100)
GO

IF OBJECT_ID('[Hardware].[MarkupStandardWarantyView]', 'V') IS NOT NULL
    drop VIEW [Hardware].[MarkupStandardWarantyView]
GO

alter table Hardware.MarkupOtherCosts drop column MarkupFactor_norm;
alter table Hardware.MarkupOtherCosts drop column MarkupFactor_norm_Approved;

go

alter table Hardware.MarkupOtherCosts
    add MarkupFactor_norm          as (MarkupFactor / 100)
      , MarkupFactor_norm_Approved as (MarkupFactor_Approved / 100)
go

IF OBJECT_ID('[Hardware].[MarkupOtherCostsView]', 'V') IS NOT NULL
    drop VIEW [Hardware].[MarkupOtherCostsView]
GO

alter table Hardware.Reinsurance drop column ReinsuranceFlatfee_norm;
alter table Hardware.Reinsurance drop column ReinsuranceFlatfee_norm_Approved;
GO

alter table Hardware.Reinsurance
    add ReinsuranceFlatfee_norm          as (ReinsuranceFlatfee * coalesce(ReinsuranceUpliftFactor / 100, 1))
      , ReinsuranceFlatfee_norm_Approved as (ReinsuranceFlatfee_Approved * coalesce(ReinsuranceUpliftFactor_Approved / 100, 1))
GO

ALTER VIEW [Hardware].[ReinsuranceView] as
    SELECT r.Wg, 
           r.Duration,
           r.ReactionTimeAvailability,

           r.ReinsuranceFlatfee_norm / er.Value                    as Cost,
           r.ReinsuranceFlatfee_norm_Approved / er2.Value as Cost_Approved

    FROM Hardware.Reinsurance r
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
    LEFT JOIN [References].ExchangeRate er2 on er2.CurrencyId = r.CurrencyReinsurance_Approved
GO

DROP INDEX ix_Hardware_Reinsurance_Sla ON [Hardware].[Reinsurance]
GO

CREATE NONCLUSTERED INDEX ix_Hardware_Reinsurance_Sla ON [Hardware].[Reinsurance] ([Wg],[Duration],[ReactionTimeAvailability])
GO

alter table Hardware.FieldServiceCost drop column TimeAndMaterialShare_norm;
alter table Hardware.FieldServiceCost drop column TimeAndMaterialShare_norm_Approved;
GO

alter table Hardware.FieldServiceCost
    add TimeAndMaterialShare_norm          as (TimeAndMaterialShare / 100)
      , TimeAndMaterialShare_norm_Approved as (TimeAndMaterialShare_Approved / 100)
GO

DROP INDEX [ix_Hardware_FieldServiceCost] ON [Hardware].[FieldServiceCost]
GO

CREATE NONCLUSTERED INDEX [ix_Hardware_FieldServiceCost] ON [Hardware].[FieldServiceCost] ([Country],[Wg],[ServiceLocation],[ReactionTimeType])
GO

DROP INDEX IX_HwFspCodeTranslation_SlaHash ON Fsp.HwFspCodeTranslation;
GO

DROP INDEX IX_HwFspCodeTranslation_Sla ON Fsp.HwFspCodeTranslation;
GO

ALTER TABLE Fsp.HwFspCodeTranslation drop column SlaHash;
go

ALTER TABLE Fsp.HwFspCodeTranslation drop column Sla;
go

ALTER TABLE Fsp.HwFspCodeTranslation 
    ADD Sla  AS cast(coalesce(CountryId, 0) as nvarchar(20)) + 
                cast(WgId                   as nvarchar(20)) + 
                cast(AvailabilityId         as nvarchar(20)) + 
                cast(DurationId             as nvarchar(20)) + 
                cast(ReactionTimeId         as nvarchar(20)) + 
                cast(ReactionTypeId         as nvarchar(20)) + 
                cast(ServiceLocationId      as nvarchar(20)) + 
                cast(ProactiveSlaId         as nvarchar(20))
      
      , SlaHash  AS checksum (
                        cast(coalesce(CountryId, 0) as nvarchar(20)) + 
                        cast(WgId                   as nvarchar(20)) + 
                        cast(AvailabilityId         as nvarchar(20)) + 
                        cast(DurationId             as nvarchar(20)) + 
                        cast(ReactionTimeId         as nvarchar(20)) + 
                        cast(ReactionTypeId         as nvarchar(20)) + 
                        cast(ServiceLocationId      as nvarchar(20)) + 
                        cast(ProactiveSlaId         as nvarchar(20))
                    );
GO

CREATE INDEX IX_HwFspCodeTranslation_Sla ON Fsp.HwFspCodeTranslation(Sla);
GO

CREATE INDEX IX_HwFspCodeTranslation_SlaHash ON Fsp.HwFspCodeTranslation(SlaHash);
GO

ALTER TABLE Fsp.HwStandardWarranty drop column ReactionTime_Avalability              ;
ALTER TABLE Fsp.HwStandardWarranty drop column ReactionTime_ReactionType             ;
ALTER TABLE Fsp.HwStandardWarranty drop column ReactionTime_ReactionType_Avalability ;
go

ALTER TABLE Fsp.HwStandardWarranty
    add  ReactionTime_Avalability              bigint
       , ReactionTime_ReactionType             bigint
       , ReactionTime_ReactionType_Avalability bigint

GO

ALTER TRIGGER [Fsp].[HwFspCodeTranslation_Updated]
ON [Fsp].[HwFspCodeTranslation]
After INSERT, UPDATE
AS BEGIN

    truncate table Fsp.HwStandardWarranty;

    -- Disable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty NOCHECK CONSTRAINT ALL;

    WITH StdCte AS (

    --remove duplicates in FSP
      SELECT fsp.CountryId
           , fsp.WgId
           , fsp.AvailabilityId   
           , fsp.DurationId       
           , fsp.ReactionTimeId   
           , fsp.ReactionTypeId   
           , fsp.ServiceLocationId
           , fsp.ProactiveSlaId   
           , row_number() OVER(PARTITION BY fsp.CountryId, fsp.WgId
                 ORDER BY 
                     fsp.CountryId
                   , fsp.WgId
                   , fsp.AvailabilityId    
                   , fsp.DurationId        
                   , fsp.ReactionTimeId    
                   , fsp.ReactionTypeId    
                   , fsp.ServiceLocationId 
                   , fsp.ProactiveSlaId    ) AS rownum

        from Fsp.HwFspCodeTranslation fsp
        where fsp.IsStandardWarranty = 1 
    )
    , StdCte2 as (
        select * from StdCte WHERE rownum = 1
    )
    , Fsp as (

        --find FSP for countries

        select    fsp.CountryId
                , fsp.WgId
                , fsp.AvailabilityId
                , fsp.DurationId
                , fsp.ReactionTimeId
                , fsp.ReactionTypeId
                , fsp.ServiceLocationId
                , fsp.ProactiveSlaId
        from StdCte2 fsp
        where CountryId is not null
    )
    , Fsp2 as (
        
        --create default country FSP

        select    c.Id as CountryId
                , fsp.WgId
                , fsp.AvailabilityId
                , fsp.DurationId
                , fsp.ReactionTimeId
                , fsp.ReactionTypeId
                , fsp.ServiceLocationId
                , fsp.ProactiveSlaId
        from StdCte2 fsp, InputAtoms.Country c
        where fsp.CountryId is null and c.IsMaster = 1
    )
    , StdFsp as (

        --get country FSP(if exists), or default country FSP

        select 
                  coalesce(fsp.CountryId          , fsp2.CountryId        ) as CountryId        
                , coalesce(fsp.WgId               , fsp2.WgId             ) as WgId             
                , coalesce(fsp.AvailabilityId     , fsp2.AvailabilityId   ) as AvailabilityId   
                , coalesce(fsp.DurationId         , fsp2.DurationId       ) as DurationId       
                , coalesce(fsp.ReactionTimeId     , fsp2.ReactionTimeId   ) as ReactionTimeId   
                , coalesce(fsp.ReactionTypeId     , fsp2.ReactionTypeId   ) as ReactionTypeId   
                , coalesce(fsp.ServiceLocationId  , fsp2.ServiceLocationId) as ServiceLocationId
                , coalesce(fsp.ProactiveSlaId     , fsp2.ProactiveSlaId   ) as ProactiveSlaId   
        from Fsp2 fsp2
        left join Fsp fsp on fsp.CountryId = fsp2.CountryId and fsp.WgId = fsp2.WgId
    )
    insert into Fsp.HwStandardWarranty(
                           Country
                         , Wg
                         , AvailabilityId
                         , Duration
                         , ReactionTimeId
                         , ReactionTypeId
                         , ServiceLocationId
                         , ProactiveSlaId
                         , ReactionTime_Avalability              
                         , ReactionTime_ReactionType             
                         , ReactionTime_ReactionType_Avalability)
        select    fsp.CountryId
                , fsp.WgId
                , fsp.AvailabilityId
                , fsp.DurationId
                , fsp.ReactionTimeId
                , fsp.ReactionTypeId
                , fsp.ServiceLocationId
                , fsp.ProactiveSlaId
                , rta.Id
                , rtt.Id
                , rtta.Id
        from StdFsp fsp
        join Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = fsp.AvailabilityId and rta.ReactionTimeId = fsp.ReactionTimeId
        join Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = fsp.ReactionTimeId and rtt.ReactionTypeId = fsp.ReactionTypeId
        join Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = fsp.AvailabilityId and rtta.ReactionTimeId = fsp.ReactionTimeId and rtta.ReactionTypeId = fsp.ReactionTypeId

    -- Enable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty CHECK CONSTRAINT ALL;

END
GO

update fsp.HwFspCodeTranslation set Name = Name where id  < 10
go

ALTER VIEW [Fsp].[HwStandardWarrantyView] AS
    SELECT std.Wg
         , std.Country
         , std.Duration
         , dur.Name
         , dur.IsProlongation
         , dur.Value as DurationValue
         , std.AvailabilityId
         , std.ReactionTimeId
         , std.ReactionTypeId
         , std.ServiceLocationId
         , std.ProActiveSlaId
         , std.ReactionTime_Avalability
         , std.ReactionTime_ReactionType
         , std.ReactionTime_ReactionType_Avalability
    FROM fsp.HwStandardWarranty std
    INNER JOIN Dependencies.Duration dur on dur.Id = std.Duration
GO





