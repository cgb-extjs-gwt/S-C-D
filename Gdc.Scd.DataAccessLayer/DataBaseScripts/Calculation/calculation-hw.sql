ALTER TABLE Hardware.AvailabilityFee DROP COLUMN CostPerKit_Approved;
go
ALTER TABLE Hardware.AvailabilityFee DROP COLUMN CostPerKitJapanBuy_Approved;
go
ALTER TABLE Hardware.AvailabilityFee DROP COLUMN MaxQty_Approved;
go
ALTER TABLE Hardware.AvailabilityFee
   ADD   CostPerKit_Approved          as (CostPerKit)
       , CostPerKitJapanBuy_Approved  as (CostPerKitJapanBuy)
       , MaxQty_Approved              as (MaxQty)
go

IF OBJECT_ID('Hardware.MaterialCostOowCalc', 'U') IS NOT NULL
  DROP TABLE Hardware.MaterialCostOowCalc;
go

CREATE TABLE Hardware.MaterialCostOowCalc (
    [Country] [bigint] NOT NULL foreign key references InputAtoms.Country(Id),
    [Wg] [bigint] NOT NULL foreign key references InputAtoms.Wg(Id),
    [MaterialCostOow] [float] NULL,
    [MaterialCostOow_Approved] [float] NULL,
    CONSTRAINT PK_MaterialCostOowCalc PRIMARY KEY NONCLUSTERED (Country, Wg)
)
GO

IF OBJECT_ID('Hardware.SpUpdateMaterialCostOowCalc') IS NOT NULL
  DROP PROCEDURE Hardware.SpUpdateMaterialCostOowCalc;
go

CREATE PROCEDURE Hardware.SpUpdateMaterialCostOowCalc
AS
BEGIN

    SET NOCOUNT ON;

    truncate table Hardware.MaterialCostOowCalc;

    -- Disable all table constraints
    ALTER TABLE Hardware.MaterialCostOowCalc NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.MaterialCostOowCalc(Country, Wg, MaterialCostOow, MaterialCostOow_Approved)
        select NonEmeiaCountry as Country, Wg, MaterialCostOow, MaterialCostOow_Approved
        from Hardware.MaterialCostOow
        where DeactivatedDateTime is null

        union 

        SELECT cr.Id AS Country, Wg, MaterialCostOow, MaterialCostOow_Approved 
		  FROM [Hardware].[MaterialCostOowEmeia] mc
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
    ALTER TABLE Hardware.MaterialCostOowCalc CHECK CONSTRAINT ALL;

END
go

IF OBJECT_ID('Hardware.MaterialCostOowUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.MaterialCostOowUpdated;
go

CREATE TRIGGER Hardware.MaterialCostOowUpdated
ON Hardware.MaterialCostOow
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostOowCalc;
END
go

IF OBJECT_ID('Hardware.MaterialCostOowEmeiaUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.MaterialCostOowEmeiaUpdated;
go

CREATE TRIGGER Hardware.MaterialCostOowEmeiaUpdated
ON Hardware.MaterialCostOowEmeia
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostOowCalc;
END
go

exec Hardware.SpUpdateMaterialCostOowCalc;
go

ALTER TABLE Hardware.ManualCost
   DROP COLUMN DealerPrice;

ALTER TABLE Hardware.ManualCost
   ADD DealerPrice as (ListPrice - (ListPrice * DealerDiscount / 100));
GO

ALTER TABLE Hardware.HddRetention
     ADD HddRet float,
         HddRet_Approved float
go

ALTER TABLE Hardware.ServiceSupportCost
    add   TotalIb                          float
        , TotalIb_Approved                 float
        , TotalIbClusterPla                float
        , TotalIbClusterPla_Approved       float
        , TotalIbClusterPlaRegion          float
        , TotalIbClusterPlaRegion_Approved float

CREATE NONCLUSTERED INDEX ix_Hardware_LogisticsCosts
    ON [Hardware].[LogisticsCosts] (Country, Wg, ReactionTimeType)
    INCLUDE ([StandardHandling],[HighAvailabilityHandling],[StandardDelivery],[ExpressDelivery],[TaxiCourierDelivery],[ReturnDeliveryFactory],[StandardHandling_Approved],[HighAvailabilityHandling_Approved],[StandardDelivery_Approved],[ExpressDelivery_Approved],[TaxiCourierDelivery_Approved],[ReturnDeliveryFactory_Approved])
GO

CREATE NONCLUSTERED INDEX ix_Atom_MarkupOtherCosts
    ON [Hardware].[MarkupOtherCosts] ([Country],[Wg], ReactionTimeTypeAvailability)
    INCLUDE (MarkupFactor, MarkupFactor_Approved, Markup, Markup_Approved)
GO

CREATE NONCLUSTERED INDEX ix_Atom_MarkupStandardWaranty
    ON [Hardware].[MarkupStandardWaranty] ([Country],[Wg])
    INCLUDE ([MarkupFactorStandardWarranty],[MarkupStandardWarranty])
GO

CREATE NONCLUSTERED INDEX ix_Hardware_ProActive
    ON [Hardware].[ProActive] ([Country],[Wg])
GO

IF OBJECT_ID('Hardware.SpGetCosts') IS NOT NULL
  DROP PROCEDURE Hardware.SpGetCosts;
go

IF OBJECT_ID('Hardware.GetCosts') IS NOT NULL
  DROP FUNCTION Hardware.GetCosts;
go 

IF OBJECT_ID('Hardware.GetCalcMember') IS NOT NULL
  DROP FUNCTION Hardware.GetCalcMember;
go 

IF OBJECT_ID('Hardware.CalcFieldServiceCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcFieldServiceCost;
go 

IF OBJECT_ID('Hardware.CalcMaterialCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcMaterialCost;
go 

IF OBJECT_ID('Hardware.CalcSrvSupportCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcSrvSupportCost;
go 

IF OBJECT_ID('Hardware.CalcTaxAndDutiesWar') IS NOT NULL
  DROP FUNCTION Hardware.CalcTaxAndDutiesWar;
go 

IF OBJECT_ID('Hardware.CalcLocSrvStandardWarranty') IS NOT NULL
  DROP FUNCTION Hardware.CalcLocSrvStandardWarranty;
go 

IF OBJECT_ID('Hardware.AvailabilityFeeCalcView', 'V') IS NOT NULL
  DROP VIEW Hardware.AvailabilityFeeCalcView;
go

IF OBJECT_ID('Hardware.AvailabilityFeeView', 'V') IS NOT NULL
  DROP VIEW Hardware.AvailabilityFeeView;
go

IF OBJECT_ID('Hardware.ServiceSupportCostView', 'V') IS NOT NULL
  DROP VIEW Hardware.ServiceSupportCostView;
go

IF OBJECT_ID('Hardware.LogisticsCostView', 'V') IS NOT NULL
  DROP VIEW Hardware.LogisticsCostView;
go

IF OBJECT_ID('Hardware.ReinsuranceView', 'V') IS NOT NULL
  DROP VIEW Hardware.ReinsuranceView;
go

IF OBJECT_ID('Hardware.FieldServiceCostView', 'V') IS NOT NULL
  DROP VIEW Hardware.FieldServiceCostView;
go

IF OBJECT_ID('Hardware.ProActiveView', 'V') IS NOT NULL
  DROP VIEW Hardware.ProActiveView;
go

IF OBJECT_ID('Hardware.TaxAndDutiesView', 'V') IS NOT NULL
  DROP VIEW Hardware.TaxAndDutiesView;
go

IF OBJECT_ID('InputAtoms.WgView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgView;
go

IF OBJECT_ID('Hardware.CalcLogisticCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcLogisticCost;
go 

IF OBJECT_ID('Hardware.CalcOtherDirectCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcOtherDirectCost;
go 

IF OBJECT_ID('Hardware.CalcCredit') IS NOT NULL
  DROP FUNCTION Hardware.CalcCredit;
go 

IF OBJECT_ID('Hardware.CalcServiceTC') IS NOT NULL
  DROP FUNCTION Hardware.CalcServiceTC;
go 

IF OBJECT_ID('Hardware.CalcServiceTP') IS NOT NULL
  DROP FUNCTION Hardware.CalcServiceTP;
go 

IF OBJECT_ID('Hardware.AddMarkup') IS NOT NULL
  DROP FUNCTION Hardware.AddMarkup;
go 

IF OBJECT_ID('Hardware.MarkupOrFixValue') IS NOT NULL
  DROP FUNCTION Hardware.MarkupOrFixValue;
go 

IF OBJECT_ID('Hardware.CalcAvailabilityFee') IS NOT NULL
  DROP FUNCTION Hardware.CalcAvailabilityFee;
go 

IF OBJECT_ID('Hardware.CalcTISC') IS NOT NULL
  DROP FUNCTION Hardware.CalcTISC;
go 

IF OBJECT_ID('Hardware.CalcYI') IS NOT NULL
  DROP FUNCTION Hardware.CalcYI;
go 

IF OBJECT_ID('Hardware.CalcProActive') IS NOT NULL
  DROP FUNCTION Hardware.CalcProActive;
go 

IF OBJECT_ID('Hardware.ConcatByDur') IS NOT NULL
  DROP FUNCTION Hardware.ConcatByDur;
go 

IF OBJECT_ID('Hardware.CalcByDur') IS NOT NULL
  DROP FUNCTION Hardware.CalcByDur;
go 

IF OBJECT_ID('Hardware.AfrYear', 'U') IS NOT NULL
  DROP TABLE Hardware.AfrYear;
go

CREATE TABLE Hardware.AfrYear(
    [Wg] [bigint] NOT NULL PRIMARY KEY CLUSTERED foreign key references InputAtoms.Wg(Id),
    [AFR1] [float] NULL,
    [AFR2] [float] NULL,
    [AFR3] [float] NULL,
    [AFR4] [float] NULL,
    [AFR5] [float] NULL,
    [AFRP1] [float] NULL,
    [AFR1_Approved] [float] NULL,
    [AFR2_Approved] [float] NULL,
    [AFR3_Approved] [float] NULL,
    [AFR4_Approved] [float] NULL,
    [AFR5_Approved] [float] NULL,
    [AFRP1_Approved] [float] NULL
)
GO

IF OBJECT_ID('Hardware.ReinsuranceYear', 'U') IS NOT NULL
  DROP TABLE Hardware.ReinsuranceYear;
go

CREATE TABLE [Hardware].[ReinsuranceYear](
    [Wg] bigint PRIMARY KEY FOREIGN KEY REFERENCES InputAtoms.Wg(Id),
    [ReinsuranceFlatfee1] [float] NULL,
    [ReinsuranceFlatfee2] [float] NULL,
    [ReinsuranceFlatfee3] [float] NULL,
    [ReinsuranceFlatfee4] [float] NULL,
    [ReinsuranceFlatfee5] [float] NULL,
    [ReinsuranceFlatfeeP1] [float] NULL,
    [ReinsuranceFlatfee1_Approved] [float] NULL,
    [ReinsuranceFlatfee2_Approved] [float] NULL,
    [ReinsuranceFlatfee3_Approved] [float] NULL,
    [ReinsuranceFlatfee4_Approved] [float] NULL,
    [ReinsuranceFlatfee5_Approved] [float] NULL,
    [ReinsuranceFlatfeeP1_Approved] [float] NULL,
    [ReinsuranceUpliftFactor_NBD_9x5] [float] NULL,
    [ReinsuranceUpliftFactor_4h_9x5] [float] NULL,
    [ReinsuranceUpliftFactor_4h_24x7] [float] NULL,
    [ReinsuranceUpliftFactor_NBD_9x5_Approved] [float] NULL,
    [ReinsuranceUpliftFactor_4h_9x5_Approved] [float] NULL,
    [ReinsuranceUpliftFactor_4h_24x7_Approved] [float] NULL
)

GO

IF OBJECT_ID('Hardware.Reinsurance_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.Reinsurance_Updated;
go

CREATE TRIGGER [Hardware].[Reinsurance_Updated]
ON [Hardware].[Reinsurance]
After INSERT, UPDATE
AS BEGIN

    declare @NBD_9x5 bigint;
    declare @4h_9x5 bigint;
    declare @4h_24x7 bigint;

    select @NBD_9x5 = id 
    from Dependencies.ReactionTime_Avalability
    where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = 'NBD')
       and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')

    select @4h_9x5 = id 
    from Dependencies.ReactionTime_Avalability
    where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
       and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')

    select @4h_24x7 = id 
    from Dependencies.ReactionTime_Avalability
    where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
       and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '24X7')

    TRUNCATE TABLE Hardware.ReinsuranceYear;

    -- Disable all table constraints
    ALTER TABLE Hardware.ReinsuranceYear NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.ReinsuranceYear(
                      Wg
                
                    , ReinsuranceFlatfee1                     
                    , ReinsuranceFlatfee2                     
                    , ReinsuranceFlatfee3                     
                    , ReinsuranceFlatfee4                     
                    , ReinsuranceFlatfee5                     
                    , ReinsuranceFlatfeeP1                    
                
                    , ReinsuranceFlatfee1_Approved            
                    , ReinsuranceFlatfee2_Approved            
                    , ReinsuranceFlatfee3_Approved            
                    , ReinsuranceFlatfee4_Approved            
                    , ReinsuranceFlatfee5_Approved            
                    , ReinsuranceFlatfeeP1_Approved           
                
                    , ReinsuranceUpliftFactor_NBD_9x5         
                    , ReinsuranceUpliftFactor_4h_9x5          
                    , ReinsuranceUpliftFactor_4h_24x7         
                
                    , ReinsuranceUpliftFactor_NBD_9x5_Approved
                    , ReinsuranceUpliftFactor_4h_9x5_Approved 
                    , ReinsuranceUpliftFactor_4h_24x7_Approved
                )
    select   r.Wg

           , max(case when d.IsProlongation = 0 and d.Value = 1  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 2  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 3  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 4  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 5  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 1 and d.Value = 1  then ReinsuranceFlatfee end) 

           , max(case when d.IsProlongation = 0 and d.Value = 1  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 2  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 3  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 4  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 5  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 1 and d.Value = 1  then ReinsuranceFlatfee_Approved end) 

           , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor end) 
           , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor end) 
           , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor end) 

           , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor_Approved end) 
           , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor_Approved end) 
           , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor_Approved end) 

    from Hardware.Reinsurance r
    join Dependencies.Duration d on d.Id = r.Duration

    where r.ReactionTimeAvailability in (@NBD_9x5, @4h_9x5, @4h_24x7) 
      and r.DeactivatedDateTime is null
    group by r.Wg;

    -- Enable all table constraints
    ALTER TABLE Hardware.ReinsuranceYear CHECK CONSTRAINT ALL;

END
GO

update Hardware.Reinsurance set ReinsuranceFlatfee = ReinsuranceFlatfee + 0;

go

IF OBJECT_ID('Hardware.AFR_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AFR_Updated;
go

CREATE TRIGGER Hardware.AFR_Updated
ON Hardware.AFR
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
        where y.Id = afr.Year
        group by afr.Wg
END
GO

update Hardware.AFR set AFR = AFR + 0
GO

IF OBJECT_ID('Fsp.HwStandardWarranty', 'U') IS NOT NULL
  DROP TABLE Fsp.HwStandardWarranty;
go

CREATE TABLE Fsp.HwStandardWarranty(
    [Country] [bigint] NOT NULL foreign key references InputAtoms.Country(Id),
    [Wg] [bigint] NOT NULL foreign key references InputAtoms.Wg(Id),
    [AvailabilityId] [bigint] NOT NULL foreign key references Dependencies.Availability(Id),
    [Duration] [bigint] NOT NULL foreign key references Dependencies.Duration(Id),
    [ReactionTimeId] [bigint] NOT NULL foreign key references Dependencies.ReactionTime(id),
    [ReactionTypeId] [bigint] NOT NULL foreign key references Dependencies.ReactionType(id),
    [ServiceLocationId] [bigint] NOT NULL foreign key references Dependencies.ServiceLocation(id),
    [ProActiveSlaId] [bigint] NOT NULL foreign key references Dependencies.ProActiveSla(id),
    [ReactionTime_Avalability] bigint,
    [ReactionTime_ReactionType] bigint,
    [ReactionTime_ReactionType_Avalability] bigint,
    CONSTRAINT PK_HwStandardWarranty PRIMARY KEY NONCLUSTERED (Country, Wg)
)
GO

CREATE VIEW [InputAtoms].[WgStdView] AS
    select *
    from InputAtoms.Wg
    where Id in (select Wg from Fsp.HwStandardWarranty) and WgType = 1
GO

IF OBJECT_ID('Fsp.HwFspCodeTranslation_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Fsp.HwFspCodeTranslation_Updated;
go

CREATE TRIGGER [Fsp].[HwFspCodeTranslation_Updated]
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
        full join Fsp fsp on fsp.CountryId = fsp2.CountryId and fsp.WgId = fsp2.WgId
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

CREATE VIEW [Fsp].[HwStandardWarrantyView] AS
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

CREATE FUNCTION Hardware.CalcByDur
(
    @year int,
    @prolongation bit,
    @v1 float,
    @v2 float,
    @v3 float,
    @v4 float,
    @v5 float,
    @vp float
)
RETURNS float
AS
BEGIN

    return
        case
            when @prolongation = 0 and @year = 1 then @v1
            when @prolongation = 0 and @year = 2 then @v1 + @v2
            when @prolongation = 0 and @year = 3 then @v1 + @v2 + @v3
            when @prolongation = 0 and @year = 4 then @v1 + @v2 + @v3 + @v4
            when @prolongation = 0 and @year = 5 then @v1 + @v2 + @v3 + @v4 + @v5
            else @vp
        end

END
GO

CREATE FUNCTION Hardware.ConcatByDur
(
    @year int,
    @prolongation bit,
    @v1 float,
    @v2 float,
    @v3 float,
    @v4 float,
    @v5 float,
    @vp float
)
RETURNS nvarchar(500)
AS
BEGIN

    return
        case
            when @prolongation = 0 and @year = 1 then cast(@v1 as nvarchar(50))
            when @prolongation = 0 and @year = 2 then cast(@v1 as nvarchar(50)) + ';' + cast(@v2 as nvarchar(50))
            when @prolongation = 0 and @year = 3 then cast(@v1 as nvarchar(50)) + ';' + cast(@v2 as nvarchar(50)) + ';' + cast(@v3 as nvarchar(50))
            when @prolongation = 0 and @year = 4 then cast(@v1 as nvarchar(50)) + ';' + cast(@v2 as nvarchar(50)) + ';' + cast(@v3 as nvarchar(50)) + ';' + cast(@v4 as nvarchar(50))
            when @prolongation = 0 and @year = 5 then cast(@v1 as nvarchar(50)) + ';' + cast(@v2 as nvarchar(50)) + ';' + cast(@v3 as nvarchar(50)) + ';' + cast(@v4 as nvarchar(50)) + ';' + cast(@v5 as nvarchar(50))
            else cast(@vp as nvarchar(50))
        end

END
GO

CREATE FUNCTION [Hardware].[CalcProActive](@setupCost float, @yearCost float, @dur int)
RETURNS float
AS
BEGIN
	RETURN @setupCost + @yearCost * @dur;
END
GO

CREATE FUNCTION [Hardware].[CalcYI](@grValue float, @deprMo float)
RETURNS float
AS
BEGIN
    if @deprMo = 0
    begin
        RETURN null;
    end
	RETURN @grValue / @deprMo * 12;
END
GO

CREATE FUNCTION [Hardware].[CalcTISC](
    @tisc float,
    @totalIB float,
    @totalIB_VENDOR float
)
RETURNS float
AS
BEGIN
    if @totalIB = 0
    begin
        RETURN null;
    end
	RETURN @tisc / @totalIB * @totalIB_VENDOR;
END
GO

CREATE FUNCTION [Hardware].[CalcAvailabilityFee](
	@kc float,
    @mq float,
    @tisc float,
    @yi float,
    @totalIB float
)
RETURNS float
AS
BEGIN
    if @mq = 0 or @totalIB = 0
    begin
        RETURN NULL;
    end

	RETURN @kc/@mq * (@tisc + @yi) / @totalIB;
END
GO

CREATE FUNCTION [Hardware].[AddMarkup](
    @value float,
    @markupFactor float,
    @markup float
)
RETURNS float
AS
BEGIN

    if @markupFactor > 0
        begin
            set @value = @value * @markupFactor;
        end
    else if @markup > 0
        begin
            set @value = @value + @markup;
        end

    RETURN @value;

END
GO

CREATE FUNCTION Hardware.MarkupOrFixValue (
    @value float,
    @markupFactor float,
    @fixed float
)
RETURNS float 
AS
BEGIN

    if @markupFactor > 0
        begin
            return @value * @markupFactor;
        end
    else if @fixed > 0
        begin
            return @fixed;
        end

    RETURN 0;

END
GO

CREATE FUNCTION [Hardware].[CalcLogisticCost](
	@standardHandling float,
    @highAvailabilityHandling float,
    @standardDelivery float,
    @expressDelivery float,
    @taxiCourierDelivery float,
    @returnDelivery float,
    @afr float
)
RETURNS float
AS
BEGIN
	RETURN @afr * (
	            @standardHandling +
                @highAvailabilityHandling +
                @standardDelivery +
                @expressDelivery +
                @taxiCourierDelivery +
                @returnDelivery
           );
END
GO

CREATE FUNCTION [Hardware].[CalcFieldServiceCost] (
    @timeAndMaterialShare float,
    @travelCost float,
    @labourCost float,
    @performanceRate float,
    @travelTime float,
    @repairTime float,
    @onsiteHourlyRate float,
    @afr float
)
RETURNS float
AS
BEGIN
    return @afr * (
                   (1 - @timeAndMaterialShare) * (@travelCost + @labourCost + @performanceRate) + 
                   @timeAndMaterialShare * ((@travelTime + @repairTime) * @onsiteHourlyRate + @performanceRate)
                );
END
GO

CREATE FUNCTION [Hardware].[CalcMaterialCost](@cost float, @afr float)
RETURNS float
AS
BEGIN
    RETURN @cost * @afr;
END
GO

CREATE FUNCTION [Hardware].[CalcSrvSupportCost] (
    @firstLevelSupport float,
    @secondLevelSupport float,
    @ibCountry float,
    @ibPla float
)
returns float
as
BEGIN
    if @ibCountry = 0 or @ibPla = 0
    begin
        return null;
    end
    return @firstLevelSupport / @ibCountry + @secondLevelSupport / @ibPla;
END
GO

CREATE FUNCTION [Hardware].[CalcTaxAndDutiesWar](@cost float, @tax float)
RETURNS float
AS
BEGIN
    RETURN @cost * @tax;
END
GO

CREATE FUNCTION [Hardware].[CalcLocSrvStandardWarranty](
    @fieldServiceCost float,
    @srvSupportCost float,
    @logisticCost float,
    @taxAndDutiesW float,
    @afr float,
    @markupFactor float,
    @markup float
)
RETURNS float
AS
BEGIN
    return Hardware.AddMarkup(@fieldServiceCost + @srvSupportCost + @logisticCost, @markupFactor, @markup) + @taxAndDutiesW * @afr;
END
GO

CREATE FUNCTION [Hardware].[CalcOtherDirectCost](
    @fieldSrvCost float,
    @srvSupportCost float,
    @materialCost float,
    @logisticCost float,
    @reinsurance float,
    @markupFactor float,
    @markup float
)
RETURNS float
AS
BEGIN
    return Hardware.AddMarkup(@fieldSrvCost + @srvSupportCost + @materialCost + @logisticCost + @reinsurance, @markupFactor, @markup);
END
GO

CREATE FUNCTION [Hardware].[CalcServiceTC](
    @fieldSrvCost float,
    @srvSupprtCost float,
    @materialCost float,
    @logisticCost float,
    @taxAndDuties float,
    @reinsurance float,
    @fee float,
    @credits float
)
RETURNS float
AS
BEGIN
	RETURN @fieldSrvCost +
           @srvSupprtCost +
           @materialCost +
           @logisticCost +
           @taxAndDuties +
           @reinsurance +
           @fee -
           @credits;
END
GO

CREATE FUNCTION [Hardware].[CalcServiceTP](
    @serviceTC float,
    @markupFactor float,
    @markup float
)
RETURNS float
AS
BEGIN
	RETURN Hardware.AddMarkup(@serviceTC, @markupFactor, @markup);
END
GO

CREATE FUNCTION [Hardware].[CalcCredit](@materialCost float, @warrantyCost float)
RETURNS float
AS
BEGIN
	RETURN @materialCost + @warrantyCost;
END
GO

CREATE VIEW [Hardware].[AvailabilityFeeView] as 
    select   fee.Country
           , fee.Wg
           
           , case  when wg.WgType = 0 then 1 else 0 end as IsMultiVendor
           
           , fee.InstalledBaseHighAvailability as IB
           , fee.InstalledBaseHighAvailability_Approved as IB_Approved
           
           , fee.TotalLogisticsInfrastructureCost          / er.Value as TotalLogisticsInfrastructureCost
           , fee.TotalLogisticsInfrastructureCost_Approved / er.Value as TotalLogisticsInfrastructureCost_Approved
           
           , case when wg.WgType = 0 then fee.StockValueMv          else fee.StockValueFj          end / er.Value as StockValue
           , case when wg.WgType = 0 then fee.StockValueMv_Approved else fee.StockValueFj_Approved end / er.Value as StockValue_Approved
           
           , fee.AverageContractDuration
           , fee.AverageContractDuration_Approved
           
           , case when fee.JapanBuy = 1          then fee.CostPerKitJapanBuy else fee.CostPerKit end as CostPerKit
           , case when fee.JapanBuy_Approved = 1 then fee.CostPerKitJapanBuy else fee.CostPerKit end as CostPerKit_Approved
           
           , fee.MaxQty

    from Hardware.AvailabilityFee fee
    JOIN InputAtoms.Wg wg on wg.Id = fee.Wg
    JOIN InputAtoms.Country c on c.Id = fee.Country
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
GO

CREATE VIEW [Hardware].[AvailabilityFeeCalcView] as 
     with InstallByCountryCte as (
        SELECT T.Country as CountryID, 
               T.Total_IB, 
               T.Total_IB_Approved, 

               T.Total_IB - coalesce(T.Total_IB_MVS, 0) as Total_IB_FTS, 
               T.Total_IB_Approved - coalesce(T.Total_IB_MVS_Approved, 0) as Total_IB_FTS_Approved, 

               T.Total_IB_MVS,
               T.Total_IB_MVS_Approved,

               T.Total_KC_MQ_IB_FTS,
               T.Total_KC_MQ_IB_FTS_Approved,

               T.Total_KC_MQ_IB_MVS,
               T.Total_KC_MQ_IB_MVS_Approved

         FROM (
            select fee.Country, 
                   sum(fee.IB) as Total_IB, 
                   sum(fee.IB_Approved) as Total_IB_Approved,
                    
                   sum(fee.IsMultiVendor * fee.IB) as Total_IB_MVS,
                   sum(fee.IsMultiVendor * fee.IB_Approved) as Total_IB_MVS_Approved,

                   sum(case when fee.MaxQty = 0 then 0 else fee.IsMultiVendor * fee.CostPerKit / fee.MaxQty * fee.IB end) as Total_KC_MQ_IB_MVS,
                   sum(case when fee.MaxQty = 0 then 0 else fee.IsMultiVendor * fee.CostPerKit_Approved / fee.MaxQty * fee.IB_Approved end) as Total_KC_MQ_IB_MVS_Approved,

                   sum(case when fee.MaxQty = 0 then 0 else (1 - fee.IsMultiVendor) * fee.CostPerKit / fee.MaxQty * fee.IB end) as Total_KC_MQ_IB_FTS,
                   sum(case when fee.MaxQty = 0 then 0 else (1 - fee.IsMultiVendor) * fee.CostPerKit_Approved / fee.MaxQty * fee.IB_Approved end) as Total_KC_MQ_IB_FTS_Approved

            from Hardware.AvailabilityFeeVIEW fee
            group by fee.Country 
        ) T
    )
    , AvFeeCte as (
        select fee.*,

               ib.Total_IB,
               ib.Total_IB_Approved,

               (case fee.IsMultiVendor 
                    when 1 then ib.Total_IB_MVS
                    else ib.Total_IB_FTS
                end) as Total_IB_VENDOR,               

               (case fee.IsMultiVendor 
                    when 1 then ib.Total_IB_MVS_Approved
                    else ib.Total_IB_FTS_Approved
                end) as Total_IB_VENDOR_Approved,               

               (case fee.IsMultiVendor
                    when 1 then ib.Total_KC_MQ_IB_MVS
                    else ib.Total_KC_MQ_IB_FTS
                end) as Total_KC_MQ_IB_VENDOR,

               (case fee.IsMultiVendor
                    when 1 then ib.Total_KC_MQ_IB_MVS_Approved
                    else ib.Total_KC_MQ_IB_FTS_Approved
                end) as Total_KC_MQ_IB_VENDOR_Approved

        from Hardware.AvailabilityFeeVIEW fee
        join InstallByCountryCte ib on ib.CountryID = fee.Country
    )
    , AvFeeCte2 as (
        select fee.*,

               Hardware.CalcYI(fee.StockValue, fee.AverageContractDuration) as YI,
               Hardware.CalcYI(fee.StockValue_Approved, fee.AverageContractDuration_Approved) as YI_Approved,
          
               Hardware.CalcTISC(fee.TotalLogisticsInfrastructureCost, fee.Total_IB, fee.Total_IB_VENDOR) as TISC,
               Hardware.CalcTISC(fee.TotalLogisticsInfrastructureCost_Approved, fee.Total_IB_Approved, fee.Total_IB_VENDOR_Approved) as TISC_Approved

        from AvFeeCte fee
    )
    select fee.*, 
           case when IB = 0 or fee.MaxQty = 0 then 0 else Hardware.CalcAvailabilityFee(fee.CostPerKit, fee.MaxQty, fee.TISC, fee.YI, fee.Total_KC_MQ_IB_VENDOR) end as Fee,
           case when IB_Approved = 0 or fee.MaxQty = 0 then 0 else Hardware.CalcAvailabilityFee(fee.CostPerKit_Approved, fee.MaxQty, fee.TISC_Approved, fee.YI_Approved, fee.Total_KC_MQ_IB_VENDOR_Approved) end as Fee_Approved
    from AvFeeCte2 fee
GO

IF OBJECT_ID('Hardware.AvailabilityFeeCalc', 'U') IS NOT NULL
  DROP TABLE Hardware.AvailabilityFeeCalc;
go

CREATE TABLE Hardware.AvailabilityFeeCalc (
    [Country] [bigint] NOT NULL FOREIGN KEY REFERENCES InputAtoms.Country(Id),
    [Wg] [bigint] NOT NULL FOREIGN KEY REFERENCES InputAtoms.Wg(Id),
    [Fee] [float] NULL,
    [Fee_Approved] [float] NULL
)

GO

CREATE NONCLUSTERED INDEX [ix_Hardware_AvailabilityFeeCalc] ON [Hardware].[AvailabilityFeeCalc]
(
	[Country] ASC,
	[Wg] ASC
)
INCLUDE ( 	[Fee],
	[Fee_Approved]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF OBJECT_ID('Hardware.UpdateAvailabilityFee') IS NOT NULL
  DROP PROCEDURE Hardware.UpdateAvailabilityFee;
go

CREATE PROCEDURE Hardware.UpdateAvailabilityFee
AS
BEGIN

    SET NOCOUNT ON;

    TRUNCATE TABLE Hardware.AvailabilityFeeCalc;

    -- Disable all table constraints
    ALTER TABLE Hardware.AvailabilityFeeCalc NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.AvailabilityFeeCalc(Country, Wg, Fee, Fee_Approved)
    select Country, Wg, Fee, Fee_Approved
    from Hardware.AvailabilityFeeCalcView fee
    join InputAtoms.Wg wg on wg.id = fee.Wg
    where wg.DeactivatedDateTime is null

    ALTER INDEX ix_Hardware_AvailabilityFeeCalc ON Hardware.AvailabilityFeeCalc REBUILD;  

    -- Enable all table constraints
    ALTER TABLE Hardware.AvailabilityFeeCalc CHECK CONSTRAINT ALL;

END
go

IF OBJECT_ID('Hardware.AvailabilityFeeUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AvailabilityFeeUpdated;
go

CREATE TRIGGER Hardware.AvailabilityFeeUpdated
ON Hardware.AvailabilityFee
After INSERT, UPDATE
AS BEGIN

    exec Hardware.UpdateAvailabilityFee;

END
go

IF OBJECT_ID('[References].ExchangeRateUpdated', 'TR') IS NOT NULL
  DROP TRIGGER [References].ExchangeRateUpdated;
go

CREATE TRIGGER [References].ExchangeRateUpdated
ON [References].ExchangeRate
After INSERT, UPDATE
AS BEGIN

    exec Hardware.UpdateAvailabilityFee;

END
go

exec Hardware.UpdateAvailabilityFee;
go

IF OBJECT_ID('Hardware.InstallBaseUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.InstallBaseUpdated;
go

CREATE TRIGGER [Hardware].[InstallBaseUpdated]
ON [Hardware].[InstallBase]
After INSERT, UPDATE
AS BEGIN

    with ibCte as (
        select ib.*
                , c.ClusterRegionId
                , pla.Id as PlaId
                , cpla.Id as ClusterPlaId 
        from Hardware.InstallBase ib
        JOIN InputAtoms.Country c on c.id = ib.Country
        JOIN InputAtoms.Wg wg on wg.id = ib.Wg
        JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
        JOIN InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId

        where ib.DeactivatedDateTime is null and wg.DeactivatedDateTime is null
    )
    , totalIb_Cte as (
        select Country
                , sum(InstalledBaseCountry) as totalIb
                , sum(InstalledBaseCountry_Approved) as totalIb_Approved
        from ibCte
        group by Country
    )
    , totalIb_PLA_Cte as (
        select Country
                , ClusterPlaId
                , sum(InstalledBaseCountry) as totalIb
                , sum(InstalledBaseCountry_Approved) as totalIb_Approved
        from ibCte
        group by Country, ClusterPlaId
    )
    , totalIb_PLA_ClusterRegion_Cte as (
        select    ClusterRegionId
                , ClusterPlaId
                , sum(InstalledBaseCountry) as totalIb
                , sum(InstalledBaseCountry_Approved) as totalIb_Approved
        from ibCte
        group by ClusterRegionId, ClusterPlaId
    )
    UPDATE ssc
            SET   ssc.TotalIb                          = t1.totalIb
                , ssc.TotalIb_Approved                 = t1.totalIb_Approved
                , ssc.TotalIbClusterPla                = t2.totalIb
                , ssc.TotalIbClusterPla_Approved       = t2.totalIb_Approved
                , ssc.TotalIbClusterPlaRegion          = t3.totalIb
                , ssc.TotalIbClusterPlaRegion_Approved = t3.totalIb_Approved
    from Hardware.ServiceSupportCost ssc
    join totalIb_Cte t1 on t1.Country = ssc.Country
    join totalIb_PLA_Cte t2 on t2.Country = ssc.Country and t2.ClusterPlaId = ssc.ClusterPla
    join totalIb_PLA_ClusterRegion_Cte t3 on t3.ClusterRegionId = ssc.ClusterRegion and t3.ClusterPlaId = ssc.ClusterPla

END
GO

IF OBJECT_ID('Hardware.HddRetentionUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.HddRetentionUpdated;
go

CREATE TRIGGER [Hardware].[HddRetentionUpdated]
ON [Hardware].[HddRetention]
After INSERT, UPDATE
AS BEGIN

    SET NOCOUNT ON;

    with cte as (
        select    h.Wg
                , sum(h.HddMaterialCost * h.HddFr / 100) as hddRet
                , sum(h.HddMaterialCost_Approved * h.HddFr_Approved / 100) as hddRet_Approved
        from Hardware.HddRetention h
        where h.Year in (select id from Dependencies.Year where IsProlongation = 0 and Value <= 5 )
        group by h.Wg
    )
    update h
        set h.HddRet = c.HddRet, HddRet_Approved = c.HddRet_Approved
    from Hardware.HddRetention h
    join cte c on c.Wg = h.Wg

END
go

update Hardware.HddRetention set HddFr = HddFr + 0;
go

IF OBJECT_ID('Hardware.HddRetentionView', 'U') IS NOT NULL
  DROP TABLE Hardware.HddRetentionView;
go

IF OBJECT_ID('Hardware.HddRetentionView', 'V') IS NOT NULL
  DROP VIEW Hardware.HddRetentionView;
go

CREATE VIEW Hardware.HddRetentionView as 
    SELECT 
           h.Wg as WgId
         , wg.Name as Wg
         , h.HddRet
         , HddRet_Approved
         , hm.TransferPrice 
         , hm.ListPrice
         , hm.DealerDiscount
         , hm.DealerPrice
         , u.Name as ChangeUserName
         , u.Email as ChangeUserEmail

    FROM Hardware.HddRetention h
    JOIN InputAtoms.Wg wg on wg.id = h.Wg
    LEFT JOIN Hardware.HddRetentionManualCost hm on hm.WgId = h.Wg
    LEFT JOIN [dbo].[User] u on u.Id = hm.ChangeUserId
    WHERE h.DeactivatedDateTime is null 
      AND h.Year = (select id from Dependencies.Year where Value = 5 and IsProlongation = 0)
go

alter table Hardware.FieldServiceCost
    add TimeAndMaterialShare_norm          as (TimeAndMaterialShare / 100)
      , TimeAndMaterialShare_norm_Approved as (TimeAndMaterialShare_Approved / 100)
GO

CREATE VIEW [Hardware].[FieldServiceCostView] AS
    SELECT  fsc.Country,
            fsc.Wg,
            case 
                when wg.WgType = 0 then 1
                else 0
            end as IsMultiVendor,
            
            hr.OnsiteHourlyRates,
            hr.OnsiteHourlyRates_Approved,

            fsc.ServiceLocation,
            rt.ReactionTypeId,
            rt.ReactionTimeId,

            fsc.RepairTime,
            fsc.RepairTime_Approved,
            
            fsc.TravelTime,
            fsc.TravelTime_Approved,

            fsc.LabourCost          / er.Value as LabourCost,
            fsc.LabourCost_Approved / er.Value as LabourCost_Approved,

            fsc.TravelCost          / er.Value as TravelCost,
            fsc.TravelCost_Approved / er.Value as TravelCost_Approved,

            fsc.PerformanceRate          / er.Value as PerformanceRate,
            fsc.PerformanceRate_Approved / er.Value as PerformanceRate_Approved,

            (fsc.TimeAndMaterialShare / 100) as TimeAndMaterialShare,
            (fsc.TimeAndMaterialShare_Approved / 100) as TimeAndMaterialShare_Approved

    FROM Hardware.FieldServiceCost fsc
    JOIN InputAtoms.Country c on c.Id = fsc.Country
    JOIN InputAtoms.Wg on wg.Id = fsc.Wg
    JOIN Dependencies.ReactionTime_ReactionType rt on rt.Id = fsc.ReactionTimeType
    LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId and hr.Country = fsc.Country
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
GO

IF OBJECT_ID('Hardware.FieldServiceCalc', 'U') IS NOT NULL
  DROP TABLE Hardware.FieldServiceCalc;
go

CREATE TABLE Hardware.FieldServiceCalc (
      [Country]             bigint NOT NULL foreign key references InputAtoms.Country(Id)
    , [Wg]                  bigint NOT NULL foreign key references InputAtoms.Wg(Id)
    , [ServiceLocation]     bigint NOT NULL foreign key references Dependencies.ServiceLocation(Id)

    , [RepairTime]          float
    , [RepairTime_Approved] float
    , [TravelTime]          float
    , [TravelTime_Approved] float
    , [TravelCost]          float
    , [TravelCost_Approved] float
    , [LabourCost]          float
    , [LabourCost_Approved] float

    CONSTRAINT PK_FieldServiceCalc PRIMARY KEY NONCLUSTERED (Country, Wg, ServiceLocation)
)
GO

ALTER INDEX PK_FieldServiceCalc ON Hardware.FieldServiceCalc DISABLE;  
go

ALTER TABLE Hardware.FieldServiceCalc NOCHECK CONSTRAINT ALL
go

insert into Hardware.FieldServiceCalc(
            Country
          , Wg
          , ServiceLocation
          , RepairTime
          , RepairTime_Approved
          , TravelTime
          , TravelTime_Approved
          , TravelCost
          , TravelCost_Approved
          , LabourCost
          , LabourCost_Approved)
select    fsc.Country
        , fsc.Wg
        , fsc.ServiceLocation
        , MIN(fsc.RepairTime)
        , MIN(fsc.RepairTime_Approved)
        , MIN(fsc.TravelTime)
        , MIN(fsc.TravelTime_Approved)
        , MIN(fsc.TravelCost)
        , MIN(fsc.TravelCost_Approved)
        , MIN(fsc.LabourCost)
        , MIN(fsc.LabourCost_Approved)
from Hardware.FieldServiceCost fsc
group by fsc.Country, fsc.Wg, fsc.ServiceLocation;
go

ALTER INDEX PK_FieldServiceCalc ON Hardware.FieldServiceCalc REBUILD;  
GO

ALTER TABLE Hardware.FieldServiceCalc CHECK CONSTRAINT ALL
go

IF OBJECT_ID('Hardware.FieldServiceTimeCalc', 'U') IS NOT NULL
  DROP TABLE Hardware.FieldServiceTimeCalc;
go

CREATE TABLE Hardware.FieldServiceTimeCalc (
      [Country]                       bigint NOT NULL foreign key references InputAtoms.Country(Id)
    , [Wg]                            bigint NOT NULL foreign key references InputAtoms.Wg(Id)
    , [ReactionTimeType]              bigint NOT NULL foreign key references Dependencies.ReactionTime_ReactionType(Id)

    , [PerformanceRate]               float
    , [PerformanceRate_Approved]      float
    , [TimeAndMaterialShare]          float
    , [TimeAndMaterialShare_Approved] float

    , [TimeAndMaterialShare_norm]          as (TimeAndMaterialShare / 100)
    , [TimeAndMaterialShare_norm_Approved] as (TimeAndMaterialShare_Approved / 100)

    CONSTRAINT PK_FieldServiceTimeCalc PRIMARY KEY NONCLUSTERED (Country, Wg, ReactionTimeType)
)
GO

ALTER INDEX PK_FieldServiceTimeCalc ON Hardware.FieldServiceTimeCalc DISABLE;  
go

ALTER TABLE Hardware.FieldServiceTimeCalc NOCHECK CONSTRAINT ALL
go

insert into Hardware.FieldServiceTimeCalc(Country, Wg, ReactionTimeType, PerformanceRate, PerformanceRate_Approved, TimeAndMaterialShare, TimeAndMaterialShare_Approved)
select    fsc.Country
        , fsc.Wg
        , fsc.ReactionTimeType
        , MIN(fsc.PerformanceRate)
        , MIN(fsc.PerformanceRate_Approved)
        , MIN(fsc.TimeAndMaterialShare)
        , MIN(fsc.TimeAndMaterialShare_Approved)
from Hardware.FieldServiceCost fsc
join Dependencies.ReactionTimeType rtt on rtt.id = fsc.ReactionTimeType and rtt.IsDisabled = 0
group by fsc.Country, fsc.Wg, fsc.ReactionTimeType;
go

ALTER INDEX PK_FieldServiceTimeCalc ON Hardware.FieldServiceTimeCalc REBUILD;  
go

ALTER TABLE Hardware.FieldServiceTimeCalc CHECK CONSTRAINT ALL
go

IF OBJECT_ID('[Hardware].[FieldServiceCost_Updated]', 'TR') IS NOT NULL
  DROP TRIGGER [Hardware].[FieldServiceCost_Updated];
go

CREATE TRIGGER [Hardware].[FieldServiceCost_Updated]
ON [Hardware].[FieldServiceCost]
After UPDATE
AS BEGIN

    with cte as (
        select i.*
        from inserted i
        join deleted d on d.Id = i.Id
    )
    update fsc 
            set RepairTime = c.RepairTime
              , RepairTime_Approved = c.RepairTime_Approved

              , TravelTime = c.TravelTime
              , TravelTime_Approved = c.TravelTime_Approved
              
              , TravelCost = c.TravelCost
              , TravelCost_Approved = c.TravelCost_Approved
              
              , LabourCost = c.LabourCost
              , LabourCost_Approved = c.LabourCost_Approved
    from Hardware.FieldServiceCalc fsc
    join cte c on c.Country = fsc.Country and c.Wg = fsc.Wg and c.ServiceLocation = fsc.ServiceLocation;

    with cte as (
        select i.*
        from inserted i
        join deleted d on d.Id = i.Id
    )
    update fst 
            set PerformanceRate = c.PerformanceRate
              , PerformanceRate_Approved = c.PerformanceRate_Approved

              , TimeAndMaterialShare = c.TimeAndMaterialShare
              , TimeAndMaterialShare_Approved = c.TimeAndMaterialShare_Approved
    from Hardware.FieldServiceTimeCalc fst
    join cte c on c.Country = fst.Country and c.Wg = fst.Wg and c.ReactionTimeType = fst.ReactionTimeType

END
go

alter table Hardware.TaxAndDuties
    add TaxAndDuties_norm          as (TaxAndDuties / 100)
      , TaxAndDuties_norm_Approved as (TaxAndDuties_Approved / 100)
GO

CREATE VIEW [Hardware].[TaxAndDutiesView] as
    select Country,
           TaxAndDuties_norm, 
           TaxAndDuties_norm_Approved
    from Hardware.TaxAndDuties
    where DeactivatedDateTime is null
GO

CREATE VIEW [InputAtoms].[WgView] WITH SCHEMABINDING as
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

CREATE VIEW [Hardware].[LogisticsCostView] AS
    SELECT lc.Country,
           lc.Wg, 
           rt.ReactionTypeId as ReactionType, 
           rt.ReactionTimeId as ReactionTime,
           
           lc.StandardHandling          / er.Value as StandardHandling,
           lc.StandardHandling_Approved / er.Value as StandardHandling_Approved,

           lc.HighAvailabilityHandling          / er.Value as HighAvailabilityHandling,
           lc.HighAvailabilityHandling_Approved / er.Value as HighAvailabilityHandling_Approved,

           lc.StandardDelivery          / er.Value as StandardDelivery,
           lc.StandardDelivery_Approved / er.Value as StandardDelivery_Approved,

           lc.ExpressDelivery          / er.Value as ExpressDelivery,
           lc.ExpressDelivery_Approved / er.Value as ExpressDelivery_Approved,

           lc.TaxiCourierDelivery          / er.Value as TaxiCourierDelivery,
           lc.TaxiCourierDelivery_Approved / er.Value as TaxiCourierDelivery_Approved,

           lc.ReturnDeliveryFactory          / er.Value as ReturnDeliveryFactory,
           lc.ReturnDeliveryFactory_Approved / er.Value as ReturnDeliveryFactory_Approved

    FROM Hardware.LogisticsCosts lc
    JOIN Dependencies.ReactionTime_ReactionType rt on rt.Id = lc.ReactionTimeType
    JOIN InputAtoms.Country c on c.Id = lc.Country
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
GO

alter table Hardware.MarkupOtherCosts
    add MarkupFactor_norm          as (MarkupFactor / 100)
      , MarkupFactor_norm_Approved as (MarkupFactor_Approved / 100)
go

alter table Hardware.MarkupStandardWaranty
    add MarkupFactorStandardWarranty_norm          as (MarkupFactorStandardWarranty / 100)
      , MarkupFactorStandardWarranty_norm_Approved as (MarkupFactorStandardWarranty_Approved / 100)
GO

CREATE VIEW [Hardware].[ServiceSupportCostView] as
    with cte as (
        select   ssc.Country
               , ssc.ClusterRegion
               , ssc.ClusterPla

               , ssc.[1stLevelSupportCostsCountry] / er.Value          as '1stLevelSupportCosts'
               , ssc.[1stLevelSupportCostsCountry_Approved] / er.Value as '1stLevelSupportCosts_Approved'
           
               , ssc.[2ndLevelSupportCostsLocal] / er.Value            as '2ndLevelSupportCostsLocal'
               , ssc.[2ndLevelSupportCostsLocal_Approved] / er.Value   as '2ndLevelSupportCostsLocal_Approved'

               , ssc.[2ndLevelSupportCostsClusterRegion]               as '2ndLevelSupportCostsClusterRegion'
               , ssc.[2ndLevelSupportCostsClusterRegion_Approved]      as '2ndLevelSupportCostsClusterRegion_Approved'

               , case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / er.Value 
                        else ssc.[2ndLevelSupportCostsClusterRegion]
                   end as '2ndLevelSupportCosts'
                
               , case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / er.Value 
                        else ssc.[2ndLevelSupportCostsClusterRegion_Approved]
                   end as '2ndLevelSupportCosts_Approved'

               , case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion          end as Total_IB_Pla
               , case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end as Total_IB_Pla_Approved

               , ssc.TotalIb
               , ssc.TotalIb_Approved
               , ssc.TotalIbClusterPla
               , ssc.TotalIbClusterPla_Approved
               , ssc.TotalIbClusterPlaRegion
               , ssc.TotalIbClusterPlaRegion_Approved

        from Hardware.ServiceSupportCost ssc
        join InputAtoms.Country c on c.Id = ssc.Country
        left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
    )
    select ssc.Country
         , ssc.ClusterRegion
         , ssc.ClusterPla
         , ssc.[1stLevelSupportCosts]
         , ssc.[1stLevelSupportCosts_Approved]
         , ssc.[2ndLevelSupportCosts]
         , ssc.[2ndLevelSupportCosts_Approved]

         , case when ssc.TotalIb <> 0 and ssc.Total_IB_Pla <> 0 
                then ssc.[1stLevelSupportCosts] / ssc.TotalIb + ssc.[2ndLevelSupportCosts] / ssc.Total_IB_Pla
            end as ServiceSupport

         , case when ssc.TotalIb_Approved <> 0 and ssc.Total_IB_Pla_Approved <> 0 
                then ssc.[1stLevelSupportCosts_Approved] / ssc.TotalIb_Approved + ssc.[2ndLevelSupportCosts_Approved] / ssc.Total_IB_Pla_Approved
            end as ServiceSupport_Approved

    from cte ssc
GO

alter table Hardware.Reinsurance
    add ReinsuranceFlatfee_norm          as (ReinsuranceFlatfee * coalesce(ReinsuranceUpliftFactor / 100, 1))
      , ReinsuranceFlatfee_norm_Approved as (ReinsuranceFlatfee_Approved * coalesce(ReinsuranceUpliftFactor_Approved / 100, 1))
GO

CREATE VIEW [Hardware].[ReinsuranceView] as
    SELECT r.Wg, 
           r.Duration,
           r.ReactionTimeAvailability,

           r.ReinsuranceFlatfee_norm / er.Value                    as Cost,
           r.ReinsuranceFlatfee_norm_Approved / er2.Value as Cost_Approved

    FROM Hardware.Reinsurance r
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
    LEFT JOIN [References].ExchangeRate er2 on er2.CurrencyId = r.CurrencyReinsurance_Approved
GO

CREATE NONCLUSTERED INDEX ix_Hardware_Reinsurance_Sla ON [Hardware].[Reinsurance] ([Wg],[Duration],[ReactionTimeAvailability])
GO

CREATE VIEW [Hardware].[ProActiveView] with schemabinding as 
    with ProActiveCte as 
    (
        select pro.Country,
               pro.Wg,
               sla.Id as ProActiveSla,
               (pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate) as LocalRemoteAccessSetup,
               (pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved) as LocalRemoteAccessSetup_Approved,

               (pro.LocalRegularUpdateReadyEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalRegularUpdateReadyRepetition) as LocalRegularUpdate,

               (pro.LocalRegularUpdateReadyEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalRegularUpdateReadyRepetition) as LocalRegularUpdate_Approved,

               (pro.LocalPreparationShcEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalPreparationShcRepetition) as LocalPreparation,

               (pro.LocalPreparationShcEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalPreparationShcRepetition) as LocalPreparation_Approved,

               (pro.LocalRemoteShcCustomerBriefingEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalRemoteShcCustomerBriefingRepetition) as LocalRemoteCustomerBriefing,

               (pro.LocalRemoteShcCustomerBriefingEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalRemoteShcCustomerBriefingRepetition) as LocalRemoteCustomerBriefing_Approved,

               (pro.LocalOnsiteShcCustomerBriefingEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalOnsiteShcCustomerBriefingRepetition) as LocalOnsiteCustomerBriefing,

               (pro.LocalOnsiteShcCustomerBriefingEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalOnsiteShcCustomerBriefingRepetition) as LocalOnsiteCustomerBriefing_Approved,

               (pro.TravellingTime * 
                pro.OnSiteHourlyRate * 
                sla.TravellingTimeRepetition) as Travel,

               (pro.TravellingTime_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.TravellingTimeRepetition) as Travel_Approved,

               (pro.CentralExecutionShcReportCost * 
                sla.CentralExecutionShcReportRepetition) as CentralExecutionReport,

               (pro.CentralExecutionShcReportCost_Approved * 
                sla.CentralExecutionShcReportRepetition) as CentralExecutionReport_Approved

        from Hardware.ProActive pro, 
             Dependencies.ProActiveSla sla
    )
    select  pro.Country,
            pro.Wg,
            pro.ProActiveSla,

            pro.LocalPreparation,
            pro.LocalPreparation_Approved,

            pro.LocalRegularUpdate,
            pro.LocalRegularUpdate_Approved,

            pro.LocalRemoteCustomerBriefing,
            pro.LocalRemoteCustomerBriefing_Approved,

            pro.LocalOnsiteCustomerBriefing,
            pro.LocalOnsiteCustomerBriefing_Approved,

            pro.Travel,
            pro.Travel_Approved,

            pro.CentralExecutionReport,
            pro.CentralExecutionReport_Approved,

           pro.LocalRemoteAccessSetup as Setup,
           pro.LocalRemoteAccessSetup_Approved  as Setup_Approved,

           (pro.LocalPreparation + 
            pro.LocalRegularUpdate + 
            pro.LocalRemoteCustomerBriefing +
            pro.LocalOnsiteCustomerBriefing +
            pro.Travel +
            pro.CentralExecutionReport) as Service,
       
           (pro.LocalPreparation_Approved + 
            pro.LocalRegularUpdate_Approved + 
            pro.LocalRemoteCustomerBriefing_Approved +
            pro.LocalOnsiteCustomerBriefing_Approved +
            pro.Travel_Approved +
            pro.CentralExecutionReport_Approved) as Service_Approved

    from ProActiveCte pro;
GO

IF OBJECT_ID('[Hardware].[GetCalcMember]') IS NOT NULL
  DROP FUNCTION [Hardware].[GetCalcMember];
go 

CREATE FUNCTION [Hardware].[GetCalcMember] (
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
            , c.Name               as Country
            , c.CurrencyId
            , er.Value             as ExchangeRate
            , m.WgId
            , wg.Name              as Wg
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

            , stdw.DurationValue   as StdWarranty

            --Cost values

            , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                           end as AFR1 
            , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                           end as AFR2 
            , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
            , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
            , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
            , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1
                                                                              
            , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved       end as MaterialCostWarranty
            , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved            end as MaterialCostOow     
                                                                                                                      
            , case when @approved = 0 then tax.TaxAndDuties_norm              else tax.TaxAndDuties_norm_Approved          end as TaxAndDuties
                                                                                                                      
            , case when @approved = 0 then r.Cost                             else r.Cost_Approved                         end as Reinsurance

            --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then fscStd.LabourCost                  else fscStd.LabourCost_Approved              end / er.Value as StdLabourCost             
            , case when @approved = 0 then fscStd.TravelCost                  else fscStd.TravelCost_Approved              end / er.Value as StdTravelCost             
            , case when @approved = 0 then fstStd.PerformanceRate             else fstStd.PerformanceRate_Approved         end / er.Value as StdPerformanceRate        

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end / er.Value as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end / er.Value as TravelCost             
            , case when @approved = 0 then fst.PerformanceRate                else fst.PerformanceRate_Approved            end / er.Value as PerformanceRate        
            , case when @approved = 0 then fst.TimeAndMaterialShare_norm      else fst.TimeAndMaterialShare_norm_Approved  end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             
            , case when @approved = 0 then hr.OnsiteHourlyRates               else hr.OnsiteHourlyRates_Approved           end as OnsiteHourlyRates      
                       
            --##### SERVICE SUPPORT COST #########                                                                                               
            , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]  else ssc.[1stLevelSupportCostsCountry_Approved] end / er.Value as [1stLevelSupportCosts] 
            , case when @approved = 0 
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion] end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                end as [2ndLevelSupportCosts] 
            , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
            , case when @approved = 0
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                end as TotalIbPla

            --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then lcStd.ExpressDelivery              else lcStd.ExpressDelivery_Approved          end / er.Value as StdExpressDelivery         
            , case when @approved = 0 then lcStd.HighAvailabilityHandling     else lcStd.HighAvailabilityHandling_Approved end / er.Value as StdHighAvailabilityHandling
            , case when @approved = 0 then lcStd.StandardDelivery             else lcStd.StandardDelivery_Approved         end / er.Value as StdStandardDelivery        
            , case when @approved = 0 then lcStd.StandardHandling             else lcStd.StandardHandling_Approved         end / er.Value as StdStandardHandling        
            , case when @approved = 0 then lcStd.ReturnDeliveryFactory        else lcStd.ReturnDeliveryFactory_Approved    end / er.Value as StdReturnDeliveryFactory   
            , case when @approved = 0 then lcStd.TaxiCourierDelivery          else lcStd.TaxiCourierDelivery_Approved      end / er.Value as StdTaxiCourierDelivery     

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end / er.Value as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end / er.Value as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end / er.Value as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end / er.Value as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end / er.Value as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end / er.Value as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                    else 0
               end as AvailabilityFee

            , case when @approved = 0 then moc.Markup                              else moc.Markup_Approved                            end / er.Value as MarkupOtherCost                      
            , case when @approved = 0 then moc.MarkupFactor_norm                   else moc.MarkupFactor_norm_Approved                 end as MarkupFactorOtherCost                
                                                                                                                                     
            , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / er.Value as MarkupStandardWarranty      
            , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end as MarkupFactorStandardWarranty

            , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate
                else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved
               end as LocalRemoteAccessSetup

            --####### PROACTIVE COST ###################
            , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * prosla.LocalRegularUpdateReadyRepetition 
                else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRegularUpdateReadyRepetition 
               end as LocalRegularUpdate

            , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * prosla.LocalPreparationShcRepetition 
                else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalPreparationShcRepetition 
               end as LocalPreparation

            , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalRemoteShcCustomerBriefingRepetition 
                else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRemoteShcCustomerBriefingRepetition 
               end as LocalRemoteCustomerBriefing

            , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalOnsiteShcCustomerBriefingRepetition 
                else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalOnsiteShcCustomerBriefingRepetition 
               end as LocalOnsiteCustomerBriefing

            , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate * prosla.TravellingTimeRepetition 
                else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved * prosla.TravellingTimeRepetition 
               end as Travel

            , case when @approved = 0 then pro.CentralExecutionShcReportCost * prosla.CentralExecutionShcReportRepetition 
                else pro.CentralExecutionShcReportCost_Approved * prosla.CentralExecutionShcReportRepetition 
               end as CentralExecutionReport

            --########## MANUAL COSTS ################
            , man.ListPrice          / er.Value as ListPrice                   
            , man.DealerDiscount                as DealerDiscount              
            , man.DealerPrice        / er.Value as DealerPrice                 
            , man.ServiceTC          / er.Value as ServiceTCManual                   
            , man.ServiceTP          / er.Value as ServiceTPManual                   
            , man.ServiceTP_Released / er.Value as ServiceTP_Released                  
            , u.Name                            as ChangeUserName
            , u.Email                           as ChangeUserEmail

    FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId and hr.Country = m.CountryId

    LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.ServiceSupportCost ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.FieldServiceCalc fscStd ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lcStd on lcStd.Country = stdw.Country AND lcStd.Wg = stdw.Wg AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability

    LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Wg = m.WgId AND msw.Country = m.CountryId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId
)
GO

IF OBJECT_ID('[Hardware].[GetCosts]') IS NOT NULL
    DROP FUNCTION [Hardware].[GetCosts]
GO

CREATE FUNCTION [Hardware].[GetCosts](
    @approved bit,
    @cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , case when m.TaxAndDuties is null then 0 else m.TaxAndDuties end as TaxAndDutiesOrZero

                , case when m.Reinsurance is null then 0 else m.Reinsurance end as ReinsuranceOrZero

                , case when m.AvailabilityFee is null then 0 else m.AvailabilityFee end as AvailabilityFeeOrZero

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

                , m.StdLabourCost + m.StdTravelCost + coalesce(m.StdPerformanceRate, 0) as FieldServicePerYearStdw

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StdStandardHandling + m.StdHighAvailabilityHandling + m.StdStandardDelivery + m.StdExpressDelivery + m.StdTaxiCourierDelivery + m.StdReturnDeliveryFactory as LogisticPerYearStdw

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive
       
        from Hardware.GetCalcMember(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdWarranty >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdWarranty >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdWarranty >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdWarranty >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5
                , 0  as mat1P

                , case when m.StdWarranty >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdWarranty >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdWarranty >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdWarranty >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdWarranty >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1 as matO1P

                , m.FieldServicePerYearStdw * m.AFR1  as FieldServiceCostStdw1
                , m.FieldServicePerYearStdw * m.AFR2  as FieldServiceCostStdw2
                , m.FieldServicePerYearStdw * m.AFR3  as FieldServiceCostStdw3
                , m.FieldServicePerYearStdw * m.AFR4  as FieldServiceCostStdw4
                , m.FieldServicePerYearStdw * m.AFR5  as FieldServiceCostStdw5

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYearStdw * m.AFR1  as LogisticStdw1
                , m.LogisticPerYearStdw * m.AFR2  as LogisticStdw2
                , m.LogisticPerYearStdw * m.AFR3  as LogisticStdw3
                , m.LogisticPerYearStdw * m.AFR4  as LogisticStdw4
                , m.LogisticPerYearStdw * m.AFR5  as LogisticStdw5

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdWarranty >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdWarranty >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdWarranty >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdWarranty >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5
                , 0  as tax1P

                , case when m.StdWarranty >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdWarranty >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdWarranty >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdWarranty >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdWarranty >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5
                , m.TaxAndDutiesOrZero * m.matO1P as taxO1P

                , m.mat1  + m.matO1                     as matCost1
                , m.mat2  + m.matO2                     as matCost2
                , m.mat3  + m.matO3                     as matCost3
                , m.mat4  + m.matO4                     as matCost4
                , m.mat5  + m.matO5                     as matCost5
                , m.mat1P + m.matO1P                    as matCost1P

                , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
                , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
                , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
                , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
                , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
                , m.TaxAndDutiesOrZero * (m.mat1P + m.matO1P) as TaxAndDuties1P

        from CostCte2 m
    )
    , CostCte3 as (
        select    
                  m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw1, m.ServiceSupportPerYear, m.LogisticStdw1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw2, m.ServiceSupportPerYear, m.LogisticStdw2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw3, m.ServiceSupportPerYear, m.LogisticStdw3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw4, m.ServiceSupportPerYear, m.LogisticStdw4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw5, m.ServiceSupportPerYear, m.LogisticStdw5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5

        from CostCte2_2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceOrZero + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1  as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceOrZero + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2  as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceOrZero + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3  as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceOrZero + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4  as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceOrZero + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5  as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceOrZero + m.OtherDirect1P + m.AvailabilityFeeOrZero              as ServiceTP1P

        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
                , case when m.ServiceTP1  < m.OtherDirect1  then 0 else m.ServiceTP1  - m.OtherDirect1  end as ServiceTC1
                , case when m.ServiceTP2  < m.OtherDirect2  then 0 else m.ServiceTP2  - m.OtherDirect2  end as ServiceTC2
                , case when m.ServiceTP3  < m.OtherDirect3  then 0 else m.ServiceTP3  - m.OtherDirect3  end as ServiceTC3
                , case when m.ServiceTP4  < m.OtherDirect4  then 0 else m.ServiceTP4  - m.OtherDirect4  end as ServiceTC4
                , case when m.ServiceTP5  < m.OtherDirect5  then 0 else m.ServiceTP5  - m.OtherDirect5  end as ServiceTC5
                , case when m.ServiceTP1P < m.OtherDirect1P then 0 else m.ServiceTP1P - m.OtherDirect1P end as ServiceTC1P
        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.ExchangeRate
         , m.WgId
         , m.Wg
         , m.AvailabilityId
         , m.Availability
         , m.DurationId
         , m.Duration
         , m.Year
         , m.IsProlongation
         , m.ReactionTimeId
         , m.ReactionTime
         , m.ReactionTypeId
         , m.ReactionType
         , m.ServiceLocationId
         , m.ServiceLocation
         , m.ProActiveSlaId
         , m.ProActiveSla

         , m.Sla
         , m.SlaHash

         , m.StdWarranty

         --Cost

         , m.AvailabilityFee * m.Year as AvailabilityFee
         , m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
         , m.taxO1 + m.taxO2 + m.taxO3 + m.taxO4 + m.taxO5 as TaxAndDutiesOow
         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
         , m.matO1 + m.matO2 + m.matO3 + m.matO4 + m.matO5 as MaterialOow

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
       
         , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
       
         , m.Credit1 + m.Credit2 + m.Credit3 + m.Credit4 + m.Credit5 as Credits

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ServiceTC
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP

         , m.ServiceTC1
         , m.ServiceTC2
         , m.ServiceTC3
         , m.ServiceTC4
         , m.ServiceTC5
         , m.ServiceTC1P

         , m.ServiceTP1
         , m.ServiceTP2
         , m.ServiceTP3
         , m.ServiceTP4
         , m.ServiceTP5
         , m.ServiceTP1P

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ServiceTPManual
         , m.ServiceTP_Released

         , m.ChangeUserName
         , m.ChangeUserEmail

       from CostCte6 m
)
GO

IF OBJECT_ID('[Hardware].[GetCalcMemberSla]') IS NOT NULL
  DROP FUNCTION [Hardware].[GetCalcMemberSla];
go 

CREATE FUNCTION [Hardware].[GetCalcMemberSla] (
    @approved       bit,
    @sla            Portfolio.Sla readonly
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT    m.Id

            --SLA

            , m.CountryId          
            , c.Name               as Country
            , c.CurrencyId
            , er.Value             as ExchangeRate
            , m.WgId
            , wg.Name              as Wg
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

            , m.Fsp
            , m.FspDescription

            , m.Sla
            , m.SlaHash

            , stdw.DurationValue   as StdWarranty

            --Cost values

            , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                           end as AFR1 
            , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                           end as AFR2 
            , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
            , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
            , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
            , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1
                                                                              
            , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved       end as MaterialCostWarranty
            , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved            end as MaterialCostOow     
                                                                                                                      
            , case when @approved = 0 then tax.TaxAndDuties_norm              else tax.TaxAndDuties_norm_Approved          end as TaxAndDuties
                                                                                                                      
            , case when @approved = 0 then r.Cost                             else r.Cost_Approved                         end as Reinsurance

            --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then fscStd.LabourCost                  else fscStd.LabourCost_Approved              end / er.Value as StdLabourCost             
            , case when @approved = 0 then fscStd.TravelCost                  else fscStd.TravelCost_Approved              end / er.Value as StdTravelCost             
            , case when @approved = 0 then fstStd.PerformanceRate             else fstStd.PerformanceRate_Approved         end / er.Value as StdPerformanceRate        

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end / er.Value as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end / er.Value as TravelCost             
            , case when @approved = 0 then fst.PerformanceRate                else fst.PerformanceRate_Approved            end / er.Value as PerformanceRate        
            , case when @approved = 0 then fst.TimeAndMaterialShare_norm      else fst.TimeAndMaterialShare_norm_Approved  end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             
            , case when @approved = 0 then hr.OnsiteHourlyRates               else hr.OnsiteHourlyRates_Approved           end as OnsiteHourlyRates      
                       
            --##### SERVICE SUPPORT COST #########                                                                                               
            , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]  else ssc.[1stLevelSupportCostsCountry_Approved] end / er.Value as [1stLevelSupportCosts] 
            , case when @approved = 0 
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion] end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                end as [2ndLevelSupportCosts] 
            , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
            , case when @approved = 0
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                end as TotalIbPla

            --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then lcStd.ExpressDelivery              else lcStd.ExpressDelivery_Approved          end / er.Value as StdExpressDelivery         
            , case when @approved = 0 then lcStd.HighAvailabilityHandling     else lcStd.HighAvailabilityHandling_Approved end / er.Value as StdHighAvailabilityHandling
            , case when @approved = 0 then lcStd.StandardDelivery             else lcStd.StandardDelivery_Approved         end / er.Value as StdStandardDelivery        
            , case when @approved = 0 then lcStd.StandardHandling             else lcStd.StandardHandling_Approved         end / er.Value as StdStandardHandling        
            , case when @approved = 0 then lcStd.ReturnDeliveryFactory        else lcStd.ReturnDeliveryFactory_Approved    end / er.Value as StdReturnDeliveryFactory   
            , case when @approved = 0 then lcStd.TaxiCourierDelivery          else lcStd.TaxiCourierDelivery_Approved      end / er.Value as StdTaxiCourierDelivery     

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end / er.Value as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end / er.Value as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end / er.Value as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end / er.Value as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end / er.Value as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end / er.Value as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                    else 0
               end as AvailabilityFee

            , case when @approved = 0 then moc.Markup                              else moc.Markup_Approved                            end / er.Value as MarkupOtherCost                      
            , case when @approved = 0 then moc.MarkupFactor_norm                   else moc.MarkupFactor_norm_Approved                 end as MarkupFactorOtherCost                
                                                                                                                                     
            , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / er.Value as MarkupStandardWarranty      
            , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end as MarkupFactorStandardWarranty

            , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate
                else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved
               end as LocalRemoteAccessSetup

            --####### PROACTIVE COST ###################
            , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * prosla.LocalRegularUpdateReadyRepetition 
                else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRegularUpdateReadyRepetition 
               end as LocalRegularUpdate

            , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * prosla.LocalPreparationShcRepetition 
                else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalPreparationShcRepetition 
               end as LocalPreparation

            , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalRemoteShcCustomerBriefingRepetition 
                else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRemoteShcCustomerBriefingRepetition 
               end as LocalRemoteCustomerBriefing

            , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalOnsiteShcCustomerBriefingRepetition 
                else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalOnsiteShcCustomerBriefingRepetition 
               end as LocalOnsiteCustomerBriefing

            , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate * prosla.TravellingTimeRepetition 
                else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved * prosla.TravellingTimeRepetition 
               end as Travel

            , case when @approved = 0 then pro.CentralExecutionShcReportCost * prosla.CentralExecutionShcReportRepetition 
                else pro.CentralExecutionShcReportCost_Approved * prosla.CentralExecutionShcReportRepetition 
               end as CentralExecutionReport

            --########## MANUAL COSTS ################
            , man.ListPrice          / er.Value as ListPrice                   
            , man.DealerDiscount                as DealerDiscount              
            , man.DealerPrice        / er.Value as DealerPrice                 
            , man.ServiceTC          / er.Value as ServiceTCManual                   
            , man.ServiceTP          / er.Value as ServiceTPManual                   
            , man.ServiceTP_Released / er.Value as ServiceTP_Released                  
            , u.Name                            as ChangeUserName
            , u.Email                           as ChangeUserEmail

    FROM @sla m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId and hr.Country = m.CountryId

    LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.ServiceSupportCost ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.FieldServiceCalc fscStd ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lcStd on lcStd.Country = stdw.Country AND lcStd.Wg = stdw.Wg AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability

    LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Wg = m.WgId AND msw.Country = m.CountryId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId
)
go

IF OBJECT_ID('[Hardware].[GetCostsSla]') IS NOT NULL
    DROP FUNCTION [Hardware].[GetCostsSla]
GO

CREATE FUNCTION [Hardware].[GetCostsSla](
    @approved       bit,
    @sla            Portfolio.Sla readonly
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , case when m.TaxAndDuties is null then 0 else m.TaxAndDuties end as TaxAndDutiesOrZero

                , case when m.Reinsurance is null then 0 else m.Reinsurance end as ReinsuranceOrZero

                , case when m.AvailabilityFee is null then 0 else m.AvailabilityFee end as AvailabilityFeeOrZero

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

                , m.StdLabourCost + m.StdTravelCost + coalesce(m.StdPerformanceRate, 0) as FieldServicePerYearStdw

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StdStandardHandling + m.StdHighAvailabilityHandling + m.StdStandardDelivery + m.StdExpressDelivery + m.StdTaxiCourierDelivery + m.StdReturnDeliveryFactory as LogisticPerYearStdw

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive
       
        from Hardware.GetCalcMemberSla(@approved, @sla) m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdWarranty >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdWarranty >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdWarranty >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdWarranty >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5
                , 0  as mat1P

                , case when m.StdWarranty >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdWarranty >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdWarranty >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdWarranty >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdWarranty >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1 as matO1P

                , m.FieldServicePerYearStdw * m.AFR1  as FieldServiceCostStdw1
                , m.FieldServicePerYearStdw * m.AFR2  as FieldServiceCostStdw2
                , m.FieldServicePerYearStdw * m.AFR3  as FieldServiceCostStdw3
                , m.FieldServicePerYearStdw * m.AFR4  as FieldServiceCostStdw4
                , m.FieldServicePerYearStdw * m.AFR5  as FieldServiceCostStdw5

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYearStdw * m.AFR1  as LogisticStdw1
                , m.LogisticPerYearStdw * m.AFR2  as LogisticStdw2
                , m.LogisticPerYearStdw * m.AFR3  as LogisticStdw3
                , m.LogisticPerYearStdw * m.AFR4  as LogisticStdw4
                , m.LogisticPerYearStdw * m.AFR5  as LogisticStdw5

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdWarranty >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdWarranty >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdWarranty >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdWarranty >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5
                , 0  as tax1P

                , case when m.StdWarranty >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdWarranty >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdWarranty >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdWarranty >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdWarranty >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5
                , m.TaxAndDutiesOrZero * m.matO1P as taxO1P

                , m.mat1  + m.matO1                     as matCost1
                , m.mat2  + m.matO2                     as matCost2
                , m.mat3  + m.matO3                     as matCost3
                , m.mat4  + m.matO4                     as matCost4
                , m.mat5  + m.matO5                     as matCost5
                , m.mat1P + m.matO1P                    as matCost1P

                , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
                , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
                , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
                , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
                , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
                , m.TaxAndDutiesOrZero * (m.mat1P + m.matO1P) as TaxAndDuties1P

        from CostCte2 m
    )
    , CostCte3 as (
        select    
                  m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw1, m.ServiceSupportPerYear, m.LogisticStdw1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw2, m.ServiceSupportPerYear, m.LogisticStdw2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw3, m.ServiceSupportPerYear, m.LogisticStdw3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw4, m.ServiceSupportPerYear, m.LogisticStdw4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw5, m.ServiceSupportPerYear, m.LogisticStdw5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5

        from CostCte2_2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceOrZero + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1  as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceOrZero + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2  as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceOrZero + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3  as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceOrZero + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4  as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceOrZero + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5  as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceOrZero + m.OtherDirect1P + m.AvailabilityFeeOrZero              as ServiceTP1P

        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
                , case when m.ServiceTP1  < m.OtherDirect1  then 0 else m.ServiceTP1  - m.OtherDirect1  end as ServiceTC1
                , case when m.ServiceTP2  < m.OtherDirect2  then 0 else m.ServiceTP2  - m.OtherDirect2  end as ServiceTC2
                , case when m.ServiceTP3  < m.OtherDirect3  then 0 else m.ServiceTP3  - m.OtherDirect3  end as ServiceTC3
                , case when m.ServiceTP4  < m.OtherDirect4  then 0 else m.ServiceTP4  - m.OtherDirect4  end as ServiceTC4
                , case when m.ServiceTP5  < m.OtherDirect5  then 0 else m.ServiceTP5  - m.OtherDirect5  end as ServiceTC5
                , case when m.ServiceTP1P < m.OtherDirect1P then 0 else m.ServiceTP1P - m.OtherDirect1P end as ServiceTC1P
        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.ExchangeRate
         , m.WgId
         , m.Wg
         , m.AvailabilityId
         , m.Availability
         , m.DurationId
         , m.Duration
         , m.Year
         , m.IsProlongation
         , m.ReactionTimeId
         , m.ReactionTime
         , m.ReactionTypeId
         , m.ReactionType
         , m.ServiceLocationId
         , m.ServiceLocation
         , m.ProActiveSlaId
         , m.ProActiveSla

         , m.Fsp
         , m.FspDescription

         , m.Sla
         , m.SlaHash

         , m.StdWarranty

         --Cost

         , m.AvailabilityFee * m.Year as AvailabilityFee
         , m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
         , m.taxO1 + m.taxO2 + m.taxO3 + m.taxO4 + m.taxO5 as TaxAndDutiesOow
         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
         , m.matO1 + m.matO2 + m.matO3 + m.matO4 + m.matO5 as MaterialOow

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
       
         , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
       
         , m.Credit1 + m.Credit2 + m.Credit3 + m.Credit4 + m.Credit5 as Credits

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ServiceTC
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP

         , m.ServiceTC1
         , m.ServiceTC2
         , m.ServiceTC3
         , m.ServiceTC4
         , m.ServiceTC5
         , m.ServiceTC1P

         , m.ServiceTP1
         , m.ServiceTP2
         , m.ServiceTP3
         , m.ServiceTP4
         , m.ServiceTP5
         , m.ServiceTP1P

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ServiceTPManual
         , m.ServiceTP_Released

         , m.ChangeUserName
         , m.ChangeUserEmail

       from CostCte6 m
)
GO

IF OBJECT_ID('Hardware.SpGetCosts') IS NOT NULL
  DROP PROCEDURE Hardware.SpGetCosts;
go

CREATE PROCEDURE Hardware.SpGetCosts
    @approved     bit,
    @local        bit,
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @lastid       bigint,
    @limit        int,
    @total        int output
AS
BEGIN

    SET NOCOUNT ON;

    select @total = COUNT(Id) from Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    declare @sla Portfolio.Sla;
    insert into @sla select * from Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    declare @cur nvarchar(max);
    declare @exchange float;

    if @local = 1
    begin
    
        --convert values from EUR to local

        select costs.Id

             , Country
             , cur.Name as Currency
             , costs.ExchangeRate

             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty

             --Cost

             , AvailabilityFee               * costs.ExchangeRate  as AvailabilityFee 
             , TaxAndDutiesW                 * costs.ExchangeRate  as TaxAndDutiesW
             , TaxAndDutiesOow               * costs.ExchangeRate  as TaxAndDutiesOow
             , Reinsurance                   * costs.ExchangeRate  as Reinsurance
             , ProActive                     * costs.ExchangeRate  as ProActive
             , ServiceSupportCost            * costs.ExchangeRate  as ServiceSupportCost

             , MaterialW                     * costs.ExchangeRate  as MaterialW
             , MaterialOow                   * costs.ExchangeRate  as MaterialOow
             , FieldServiceCost              * costs.ExchangeRate  as FieldServiceCost
             , Logistic                      * costs.ExchangeRate  as Logistic
             , OtherDirect                   * costs.ExchangeRate  as OtherDirect
             , LocalServiceStandardWarranty  * costs.ExchangeRate  as LocalServiceStandardWarranty
             , Credits                       * costs.ExchangeRate  as Credits
             , ServiceTC                     * costs.ExchangeRate  as ServiceTC
             , ServiceTP                     * costs.ExchangeRate  as ServiceTP

             , ServiceTCManual               * costs.ExchangeRate  as ServiceTCManual
             , ServiceTPManual               * costs.ExchangeRate  as ServiceTPManual

             , ServiceTP_Released            * costs.ExchangeRate  as ServiceTP_Released

             , ListPrice                     * costs.ExchangeRate  as ListPrice
             , DealerPrice                   * costs.ExchangeRate  as DealerPrice
             , DealerDiscount                                      as DealerDiscount
                                                             
             , ChangeUserName                                      as ChangeUserName
             , ChangeUserEmail                                     as ChangeUserEmail

        from Hardware.GetCostsSla(@approved, @sla) costs
        join [References].Currency cur on cur.Id = costs.CurrencyId
        order by Id
        
    end
    else
    begin

        select costs.Id

             , Country
             , 'EUR' as Currency
             , costs.ExchangeRate

             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty

             --Cost

             , AvailabilityFee               
             , TaxAndDutiesW                 
             , TaxAndDutiesOow               
             , Reinsurance                   
             , ProActive                     
             , ServiceSupportCost            

             , MaterialW                     
             , MaterialOow                   
             , FieldServiceCost              
             , Logistic                      
             , OtherDirect                   
             , LocalServiceStandardWarranty  
             , Credits                       
             , ServiceTC                     
             , ServiceTP                     

             , ServiceTCManual               
             , ServiceTPManual               

             , ServiceTP_Released            

             , ListPrice                     
             , DealerPrice                   
             , DealerDiscount                
                                             
             , ChangeUserName                
             , ChangeUserEmail               

        from Hardware.GetCostsSla(@approved, @sla) costs
        order by Id
    end
END
go

IF OBJECT_ID('Hardware.CalcServiceCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcServiceCost;
go

CREATE FUNCTION Hardware.CalcServiceCost(

    --#### CALCULATION YEAR #############################
      @Year                                   int
    , @IsProlongation                         bit
    , @StdWarranty                            int

    --##### AVAIRAGE FAIL RATE ##############################
    , @AFR1                                   float
    , @AFR2                                   float
    , @AFR3                                   float
    , @AFR4                                   float
    , @AFR5                                   float
    , @AFRP1                                  float

    --##### MATERIAL COST ##############################
    , @MaterialCostWarranty                   float
    , @MaterialCostOow                        float

    --##### TAXES ##############################
    , @TaxAndDuties                           float
    , @Reinsurance                            float

    --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
    , @StdLabourCost                          float
    , @StdTravelCost                          float
    , @StdPerformanceRate                     float

    --##### FIELD SERVICE COST #########                                                                                               
    , @LabourCost                             float
    , @TravelCost                             float
    , @PerformanceRate                        float
    , @TimeAndMaterialShare                   float
    , @TravelTime                             float
    , @RepairTime                             float
    , @OnsiteHourlyRates                      float
                       
    --##### SERVICE SUPPORT COST #########                                                                                               
    , @1stLevelSupportCosts                   float
    , @2ndLevelSupportCosts                   float
    , @TotalIb                                float
    , @TotalIbPla                             float

    --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
    , @StdExpressDelivery                     float
    , @StdHighAvailabilityHandling            float
    , @StdStandardDelivery                    float
    , @StdStandardHandling                    float
    , @StdReturnDeliveryFactory               float
    , @StdTaxiCourierDelivery                 float

    --##### LOGISTICS COST #########                                                                                               
    , @ExpressDelivery                        float
    , @HighAvailabilityHandling               float
    , @StandardDelivery                       float
    , @StandardHandling                       float
    , @ReturnDeliveryFactory                  float
    , @TaxiCourierDelivery                    float

    , @AvailabilityFee                        float
    , @MarkupOtherCost                        float
    , @MarkupFactorOtherCost                  float
    , @MarkupStandardWarranty                 float
    , @MarkupFactorStandardWarranty           float

    --####### PROACTIVE COST ###################
    , @LocalRemoteAccessSetup                 float
    , @LocalRegularUpdate                     float
    , @LocalPreparation                       float
    , @LocalRemoteCustomerBriefing            float
    , @LocalOnsiteCustomerBriefing            float
    , @Travel                                 float
    , @CentralExecutionReport                 float
)
RETURNS @tbl TABLE  (
          AvailabilityFee              float
        , TaxAndDutiesW                float
        , TaxAndDutiesOow              float
        , Reinsurance                  float
        , ProActive                    float
        , ServiceSupportCost           float
        , MaterialW                    float
        , MaterialOow                  float
        , FieldServiceCost             float
        , Logistic                     float
        , OtherDirect                  float
        , LocalServiceStandardWarranty float
        , Credits                      float
        , ServiceTC                    float
        , ServiceTP                    float
        , ServiceTC1                   float
        , ServiceTC2                   float
        , ServiceTC3                   float
        , ServiceTC4                   float
        , ServiceTC5                   float
        , ServiceTC1P                  float
        , ServiceTP1                   float
        , ServiceTP2                   float
        , ServiceTP3                   float
        , ServiceTP4                   float
        , ServiceTP5                   float
        , ServiceTP1P                  float
    )
AS
BEGIN

    declare @TaxAndDutiesOrZero float = case when @TaxAndDuties is null then 0 else @TaxAndDuties end;

    declare @ReinsuranceOrZero float = case when @Reinsurance is null then 0 else @Reinsurance end;

    declare @AvailabilityFeeOrZero float = case when @AvailabilityFee is null then 0 else @AvailabilityFee end;

    declare @ServiceSupportPerYear float = case when @TotalIb > 0 and @TotalIbPla > 0 then @1stLevelSupportCosts / @TotalIb + @2ndLevelSupportCosts / @TotalIbPla end;

    declare @FieldServicePerYearStdw float = @StdLabourCost + @StdTravelCost + coalesce(@StdPerformanceRate, 0);

    declare @FieldServicePerYear float = (1 - @TimeAndMaterialShare) * (@TravelCost + @LabourCost + @PerformanceRate) + @TimeAndMaterialShare * ((@TravelTime + @repairTime) * @OnsiteHourlyRates + @PerformanceRate);

    declare @LogisticPerYearStdw float = @StdStandardHandling + @StdHighAvailabilityHandling + @StdStandardDelivery + @StdExpressDelivery + @StdTaxiCourierDelivery + @StdReturnDeliveryFactory;

    declare @LogisticPerYear float = @StandardHandling + @HighAvailabilityHandling + @StandardDelivery + @ExpressDelivery + @TaxiCourierDelivery + @ReturnDeliveryFactory;

    declare @ProActive float = @LocalRemoteAccessSetup + @Year * (@LocalPreparation + @LocalRegularUpdate + @LocalRemoteCustomerBriefing + @LocalOnsiteCustomerBriefing + @Travel + @CentralExecutionReport);

    declare @mat1  float = 0;
    declare @mat2  float = 0;
    declare @mat3  float = 0;
    declare @mat4  float = 0;
    declare @mat5  float = 0;
    declare @mat1P float = 0;

    declare @matO1  float = @MaterialCostOow * @AFR1;
    declare @matO2  float = @MaterialCostOow * @AFR2;
    declare @matO3  float = @MaterialCostOow * @AFR3;
    declare @matO4  float = @MaterialCostOow * @AFR4;
    declare @matO5  float = @MaterialCostOow * @AFR5;
    declare @matO1P float = @MaterialCostOow * @AFRP1;

    declare @tax1  float = 0;
    declare @tax2  float = 0;
    declare @tax3  float = 0;
    declare @tax4  float = 0;
    declare @tax5  float = 0;
    declare @tax1P float = 0;

    declare @taxO1  float = @TaxAndDutiesOrZero * @matO1;
    declare @taxO2  float = @TaxAndDutiesOrZero * @matO2;
    declare @taxO3  float = @TaxAndDutiesOrZero * @matO3;
    declare @taxO4  float = @TaxAndDutiesOrZero * @matO4;
    declare @taxO5  float = @TaxAndDutiesOrZero * @matO5;
    declare @taxO1P float = @TaxAndDutiesOrZero * @matO1P;

    declare @LocalServiceStandardWarranty1 float = 0;
    declare @LocalServiceStandardWarranty2 float = 0;
    declare @LocalServiceStandardWarranty3 float = 0;
    declare @LocalServiceStandardWarranty4 float = 0;
    declare @LocalServiceStandardWarranty5 float = 0;

    if @StdWarranty >= 1
    begin
        set @mat1    = @MaterialCostWarranty * @AFR1;
        set @matO1   = 0;
        set @tax1    = @TaxAndDutiesOrZero * @mat1;
        set @taxO1   = 0;
        set @LocalServiceStandardWarranty1  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR1, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR1, @tax1, @AFR1, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end
    
    if @StdWarranty >= 2
    begin
        set @mat2   = @MaterialCostWarranty * @AFR2;
        set @matO2  = 0;
        set @tax2   = @TaxAndDutiesOrZero * @mat2;
        set @taxO2  = 0;
        set @LocalServiceStandardWarranty2  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR2, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR2, @tax2, @AFR2, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end

    if @StdWarranty >= 3
    begin
        set @mat3   = @MaterialCostWarranty * @AFR3;
        set @matO3  = 0;
        set @tax3   = @TaxAndDutiesOrZero * @mat3;
        set @taxO3  = 0;
        set @LocalServiceStandardWarranty3  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR3, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR3, @tax3, @AFR3, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end 

    if @StdWarranty >= 4
    begin
        set @mat4   = @MaterialCostWarranty * @AFR4;
        set @matO4  = 0;
        set @tax4   = @TaxAndDutiesOrZero * @mat4;
        set @taxO4  = 0;
        set @LocalServiceStandardWarranty4  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR4, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR4, @tax4, @AFR4, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end

    if @StdWarranty >= 5
    begin
        set @mat5   = @MaterialCostWarranty * @AFR5;
        set @matO5  = 0;
        set @tax5   = @TaxAndDutiesOrZero * @mat5;
        set @taxO5  = 0;
        set @LocalServiceStandardWarranty5  = Hardware.CalcLocSrvStandardWarranty(@FieldServicePerYearStdw * @AFR5, @ServiceSupportPerYear, @LogisticPerYearStdw * @AFR5, @tax5, @AFR5, 1 + @MarkupFactorStandardWarranty, @MarkupStandardWarranty);
    end

    declare @Credit1 float = @mat1 + @LocalServiceStandardWarranty1;
    declare @Credit2 float = @mat2 + @LocalServiceStandardWarranty2;
    declare @Credit3 float = @mat3 + @LocalServiceStandardWarranty3;
    declare @Credit4 float = @mat4 + @LocalServiceStandardWarranty4;
    declare @Credit5 float = @mat5 + @LocalServiceStandardWarranty5;

    declare @matCost1  float = @mat1  + @matO1;
    declare @matCost2  float = @mat2  + @matO2;
    declare @matCost3  float = @mat3  + @matO3;
    declare @matCost4  float = @mat4  + @matO4;
    declare @matCost5  float = @mat5  + @matO5;
    declare @matCost1P float = @mat1P + @matO1P;

    declare @TaxAndDuties1  float = @TaxAndDutiesOrZero * (@mat1  + @matO1);
    declare @TaxAndDuties2  float = @TaxAndDutiesOrZero * (@mat2  + @matO2);
    declare @TaxAndDuties3  float = @TaxAndDutiesOrZero * (@mat3  + @matO3);
    declare @TaxAndDuties4  float = @TaxAndDutiesOrZero * (@mat4  + @matO4);
    declare @TaxAndDuties5  float = @TaxAndDutiesOrZero * (@mat5  + @matO5);
    declare @TaxAndDuties1P float = @TaxAndDutiesOrZero * (@mat1P + @matO1P);

    declare @FieldServiceCost1  float = @FieldServicePerYear * @AFR1;
    declare @FieldServiceCost2  float = @FieldServicePerYear * @AFR2;
    declare @FieldServiceCost3  float = @FieldServicePerYear * @AFR3;
    declare @FieldServiceCost4  float = @FieldServicePerYear * @AFR4;
    declare @FieldServiceCost5  float = @FieldServicePerYear * @AFR5;
    declare @FieldServiceCost1P float = @FieldServicePerYear * @AFRP1;

    declare @Logistic1  float = @LogisticPerYear * @AFR1;
    declare @Logistic2  float = @LogisticPerYear * @AFR2;
    declare @Logistic3  float = @LogisticPerYear * @AFR3;
    declare @Logistic4  float = @LogisticPerYear * @AFR4;
    declare @Logistic5  float = @LogisticPerYear * @AFR5;
    declare @Logistic1P float = @LogisticPerYear * @AFRP1;

    declare @OtherDirect1  float = Hardware.MarkupOrFixValue(@FieldServiceCost1  + @ServiceSupportPerYear + @matCost1  + @Logistic1  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect2  float = Hardware.MarkupOrFixValue(@FieldServiceCost2  + @ServiceSupportPerYear + @matCost2  + @Logistic2  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect3  float = Hardware.MarkupOrFixValue(@FieldServiceCost3  + @ServiceSupportPerYear + @matCost3  + @Logistic3  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect4  float = Hardware.MarkupOrFixValue(@FieldServiceCost4  + @ServiceSupportPerYear + @matCost4  + @Logistic4  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect5  float = Hardware.MarkupOrFixValue(@FieldServiceCost5  + @ServiceSupportPerYear + @matCost5  + @Logistic5  + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);
    declare @OtherDirect1P float = Hardware.MarkupOrFixValue(@FieldServiceCost1P + @ServiceSupportPerYear + @matCost1P + @Logistic1P + @ReinsuranceOrZero + @AvailabilityFeeOrZero, @MarkupFactorOtherCost, @MarkupOtherCost);

    declare @ServiceTP1  float = @FieldServiceCost1  + @ServiceSupportPerYear + @matCost1  + @Logistic1  + @TaxAndDuties1  + @ReinsuranceOrZero + @OtherDirect1  + @AvailabilityFeeOrZero - @Credit1;
    declare @ServiceTP2  float = @FieldServiceCost2  + @ServiceSupportPerYear + @matCost2  + @Logistic2  + @TaxAndDuties2  + @ReinsuranceOrZero + @OtherDirect2  + @AvailabilityFeeOrZero - @Credit2;
    declare @ServiceTP3  float = @FieldServiceCost3  + @ServiceSupportPerYear + @matCost3  + @Logistic3  + @TaxAndDuties3  + @ReinsuranceOrZero + @OtherDirect3  + @AvailabilityFeeOrZero - @Credit3;
    declare @ServiceTP4  float = @FieldServiceCost4  + @ServiceSupportPerYear + @matCost4  + @Logistic4  + @TaxAndDuties4  + @ReinsuranceOrZero + @OtherDirect4  + @AvailabilityFeeOrZero - @Credit4;
    declare @ServiceTP5  float = @FieldServiceCost5  + @ServiceSupportPerYear + @matCost5  + @Logistic5  + @TaxAndDuties5  + @ReinsuranceOrZero + @OtherDirect5  + @AvailabilityFeeOrZero - @Credit5;
    declare @ServiceTP1P float = @FieldServiceCost1P + @ServiceSupportPerYear + @matCost1P + @Logistic1P + @TaxAndDuties1P + @ReinsuranceOrZero + @OtherDirect1P + @AvailabilityFeeOrZero;

    declare @ServiceTC1  float = case when @ServiceTP1  < @OtherDirect1  then 0 else @ServiceTP1  - @OtherDirect1  end;
    declare @ServiceTC2  float = case when @ServiceTP2  < @OtherDirect2  then 0 else @ServiceTP2  - @OtherDirect2  end;
    declare @ServiceTC3  float = case when @ServiceTP3  < @OtherDirect3  then 0 else @ServiceTP3  - @OtherDirect3  end;
    declare @ServiceTC4  float = case when @ServiceTP4  < @OtherDirect4  then 0 else @ServiceTP4  - @OtherDirect4  end;
    declare @ServiceTC5  float = case when @ServiceTP5  < @OtherDirect5  then 0 else @ServiceTP5  - @OtherDirect5  end;
    declare @ServiceTC1P float = case when @ServiceTP1P < @OtherDirect1P then 0 else @ServiceTP1P - @OtherDirect1P end;

    insert into @tbl
    select   @AvailabilityFee * @Year --as AvailabilityFee
           , @tax1 + @tax2 + @tax3 + @tax4 + @tax5 --as TaxAndDutiesW
           , @taxO1 + @taxO2 + @taxO3 + @taxO4 + @taxO5 --as TaxAndDutiesOow
           , @Reinsurance
           , @ProActive
           , @Year * @ServiceSupportPerYear --as ServiceSupportCost
        
           , @mat1 + @mat2 + @mat3 + @mat4 + @mat5 --as MaterialW
           , @matO1 + @matO2 + @matO3 + @matO4 + @matO5 --as MaterialOow
        
           , Hardware.CalcByDur(@Year, @IsProlongation, @FieldServiceCost1, @FieldServiceCost2, @FieldServiceCost3, @FieldServiceCost4, @FieldServiceCost5, @FieldServiceCost1P) --as FieldServiceCost
           , Hardware.CalcByDur(@Year, @IsProlongation, @Logistic1, @Logistic2, @Logistic3, @Logistic4, @Logistic5, @Logistic1P) --as Logistic
           , Hardware.CalcByDur(@Year, @IsProlongation, @OtherDirect1, @OtherDirect2, @OtherDirect3, @OtherDirect4, @OtherDirect5, @OtherDirect1P) --as OtherDirect
          
           , @LocalServiceStandardWarranty1 + @LocalServiceStandardWarranty2 + @LocalServiceStandardWarranty3 + @LocalServiceStandardWarranty4 + @LocalServiceStandardWarranty5 --as LocalServiceStandardWarranty
          
           , @Credit1 + @Credit2 + @Credit3 + @Credit4 + @Credit5 --as Credits
        
           , Hardware.CalcByDur(@Year, @IsProlongation, @ServiceTC1, @ServiceTC2, @ServiceTC3, @ServiceTC4, @ServiceTC5, @ServiceTC1P) --as ServiceTC
           , Hardware.CalcByDur(@Year, @IsProlongation, @ServiceTP1, @ServiceTP2, @ServiceTP3, @ServiceTP4, @ServiceTP5, @ServiceTP1P) --as ServiceTP
        
           , @ServiceTC1
           , @ServiceTC2
           , @ServiceTC3
           , @ServiceTC4
           , @ServiceTC5
           , @ServiceTC1P
        
           , @ServiceTP1
           , @ServiceTP2
           , @ServiceTP3
           , @ServiceTP4
           , @ServiceTP5
           , @ServiceTP1P;

    RETURN;
END
GO

IF OBJECT_ID('[Hardware].[CalcServiceCostSla]') IS NOT NULL
    DROP FUNCTION [Hardware].[CalcServiceCostSla]
GO

CREATE FUNCTION [Hardware].[CalcServiceCostSla](@approved bit, @sla Portfolio.Sla readonly)
RETURNS TABLE 
AS
RETURN 
(
    select 
           m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.ExchangeRate
         , m.WgId
         , m.Wg
         , m.AvailabilityId
         , m.Availability
         , m.DurationId
         , m.Duration
         , m.Year
         , m.IsProlongation
         , m.ReactionTimeId
         , m.ReactionTime
         , m.ReactionTypeId
         , m.ReactionType
         , m.ServiceLocationId
         , m.ServiceLocation
         , m.ProActiveSlaId
         , m.ProActiveSla

         , m.Fsp
         , m.FspDescription

         , m.Sla
         , m.SlaHash

         , m.StdWarranty

         --Cost

         , c.AvailabilityFee
         , c.TaxAndDutiesW
         , c.TaxAndDutiesOow
         , c.Reinsurance
         , c.ProActive
         , c.ServiceSupportCost
         , c.MaterialW
         , c.MaterialOow
         , c.FieldServiceCost
         , c.Logistic
         , c.OtherDirect
         , c.LocalServiceStandardWarranty
         , c.Credits

         , c.ServiceTC
         , c.ServiceTP

         , c.ServiceTC1
         , c.ServiceTC2
         , c.ServiceTC3
         , c.ServiceTC4
         , c.ServiceTC5
         , c.ServiceTC1P

         , c.ServiceTP1
         , c.ServiceTP2
         , c.ServiceTP3
         , c.ServiceTP4
         , c.ServiceTP5
         , c.ServiceTP1P

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ServiceTPManual
         , m.ServiceTP_Released

         , m.ChangeUserName
         , m.ChangeUserEmail

    from Hardware.GetCalcMemberSla(0, @sla) m
    cross apply Hardware.CalcServiceCost(
                  m.Year
                , m.IsProlongation
                , m.StdWarranty
                
                , m.AFR1
                , m.AFR2
                , m.AFR3
                , m.AFR4
                , m.AFR5
                , m.AFRP1
                
                , m.MaterialCostWarranty
                , m.MaterialCostOow
                
                , m.TaxAndDuties
                , m.Reinsurance
                
                , m.StdLabourCost
                , m.StdTravelCost
                , m.StdPerformanceRate
                
                , m.LabourCost
                , m.TravelCost
                , m.PerformanceRate
                , m.TimeAndMaterialShare
                , m.TravelTime
                , m.RepairTime
                , m.OnsiteHourlyRates

                , m.[1stLevelSupportCosts]
                , m.[2ndLevelSupportCosts]
                , m.TotalIb
                , m.TotalIbPla

                , m.StdExpressDelivery
                , m.StdHighAvailabilityHandling
                , m.StdStandardDelivery
                , m.StdStandardHandling
                , m.StdReturnDeliveryFactory
                , m.StdTaxiCourierDelivery

                , m.ExpressDelivery
                , m.HighAvailabilityHandling
                , m.StandardDelivery
                , m.StandardHandling
                , m.ReturnDeliveryFactory
                , m.TaxiCourierDelivery

                , m.AvailabilityFee
                , m.MarkupOtherCost
                , m.MarkupFactorOtherCost
                , m.MarkupStandardWarranty
                , m.MarkupFactorStandardWarranty

                , m.LocalRemoteAccessSetup
                , m.LocalRegularUpdate
                , m.LocalPreparation
                , m.LocalRemoteCustomerBriefing
                , m.LocalOnsiteCustomerBriefing
                , m.Travel
                , m.CentralExecutionReport
            ) c
)
go

IF OBJECT_ID('Hardware.GetCostsSlaSog') IS NOT NULL
  DROP FUNCTION Hardware.GetCostsSlaSog;
go

CREATE FUNCTION [Hardware].[GetCostsSlaSog](@approved bit, @sla Portfolio.Sla readonly)
RETURNS TABLE 
AS
RETURN 
(
    with cte as (
        select    
               m.Id

             --SLA

             , m.CountryId
             , m.Country
             , m.CurrencyId
             , m.ExchangeRate

             , m.WgId
             , m.Wg
             , wg.Description as WgDescription
             , wg.SogId
             , sog.Name as Sog

             , m.AvailabilityId
             , m.Availability
             , m.DurationId
             , m.Duration
             , m.Year
             , m.IsProlongation
             , m.ReactionTimeId
             , m.ReactionTime
             , m.ReactionTypeId
             , m.ReactionType
             , m.ServiceLocationId
             , m.ServiceLocation
             , m.ProActiveSlaId
             , m.ProActiveSla
             , m.Sla
             , m.SlaHash

             , m.StdWarranty

             --Cost

             , m.AvailabilityFee
             , m.TaxAndDutiesW
             , m.TaxAndDutiesOow
             , m.Reinsurance
             , m.ProActive
             , m.ServiceSupportCost
             , m.MaterialW
             , m.MaterialOow
             , m.FieldServiceCost
             , m.Logistic
             , m.OtherDirect
             , m.LocalServiceStandardWarranty
             , m.Credits

             , ib.InstalledBaseCountry

             , (sum(m.ServiceTC * ib.InstalledBaseCountry)                               over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tc 
             , (sum(case when m.ServiceTC > 0 then ib.InstalledBaseCountry end)          over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tc
             , (sum(m.ServiceTP_Released * ib.InstalledBaseCountry)                      over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp
             , (sum(case when m.ServiceTP_Released > 0 then ib.InstalledBaseCountry end) over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp

             , m.ListPrice
             , m.DealerDiscount
             , m.DealerPrice

        from Hardware.GetCostsSla(@approved, @sla) m
        join InputAtoms.Wg wg on wg.id = m.WgId
        join InputAtoms.Sog sog on sog.id = wg.SogId
        left join Hardware.InstallBase ib on ib.Country = m.CountryId and ib.Wg = m.WgId
    )
    select    
            m.Id

            --SLA

            , m.CountryId
            , m.Country
            , m.CurrencyId
            , m.ExchangeRate

            , m.WgId
            , m.Wg
            , m.WgDescription
            , m.SogId
            , m.Sog

            , m.AvailabilityId
            , m.Availability
            , m.DurationId
            , m.Duration
            , m.Year
            , m.IsProlongation
            , m.ReactionTimeId
            , m.ReactionTime
            , m.ReactionTypeId
            , m.ReactionType
            , m.ServiceLocationId
            , m.ServiceLocation
            , m.ProActiveSlaId
            , m.ProActiveSla
            , m.Sla
            , m.SlaHash

            , m.StdWarranty

            --Cost

            , m.AvailabilityFee
            , m.TaxAndDutiesW
            , m.TaxAndDutiesOow
            , m.Reinsurance
            , m.ProActive
            , m.ServiceSupportCost
            , m.MaterialW
            , m.MaterialOow
            , m.FieldServiceCost
            , m.Logistic
            , m.OtherDirect
            , m.LocalServiceStandardWarranty
            , m.Credits

            , case when m.sum_ib_by_tc > 0 then m.sum_ib_x_tc / m.sum_ib_by_tc end as ServiceTcSog
            , case when m.sum_ib_by_tp > 0 then m.sum_ib_x_tp / m.sum_ib_by_tp end as ServiceTpSog
    
            , m.ListPrice
            , m.DealerDiscount
            , m.DealerPrice  

    from cte m
)
go

