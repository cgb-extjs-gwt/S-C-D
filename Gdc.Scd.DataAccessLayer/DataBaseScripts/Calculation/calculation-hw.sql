DROP INDEX [IX_HwFspCodeTranslation_AvailabilityId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_CountryId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_DurationId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ProactiveSlaId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ReactionTimeId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ReactionTypeId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ServiceLocationId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_WgId] ON [Fsp].[HwFspCodeTranslation]
GO

ALTER TABLE Fsp.HwFspCodeTranslation 
    ADD SlaHash  AS checksum (
                        cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                        cast(WgId                   as nvarchar(20)) + ',' +
                        cast(AvailabilityId         as nvarchar(20)) + ',' +
                        cast(DurationId             as nvarchar(20)) + ',' +
                        cast(ReactionTimeId         as nvarchar(20)) + ',' +
                        cast(ReactionTypeId         as nvarchar(20)) + ',' +
                        cast(ServiceLocationId      as nvarchar(20)) + ',' +
                        cast(ProactiveSlaId         as nvarchar(20))
                    );
GO

CREATE INDEX IX_HwFspCodeTranslation_SlaHash ON Fsp.HwFspCodeTranslation(SlaHash);
GO

ALTER TABLE Portfolio.LocalPortfolio
    ADD SlaHash  AS checksum (
                    cast(CountryId         as nvarchar(20)) + ',' +
                    cast(WgId              as nvarchar(20)) + ',' +
                    cast(AvailabilityId    as nvarchar(20)) + ',' +
                    cast(DurationId        as nvarchar(20)) + ',' +
                    cast(ReactionTimeId    as nvarchar(20)) + ',' +
                    cast(ReactionTypeId    as nvarchar(20)) + ',' +
                    cast(ServiceLocationId as nvarchar(20)) + ',' +
                    cast(ProactiveSlaId    as nvarchar(20)) 
                );
GO

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

CREATE NONCLUSTERED INDEX ix_Hardware_FieldServiceCost
    ON [Hardware].[FieldServiceCost] ([Country],[Wg])
    INCLUDE ([ServiceLocation],[ReactionTimeType],[RepairTime],[TravelTime],[LabourCost],[TravelCost],[PerformanceRate],[TimeAndMaterialShare])
GO

CREATE NONCLUSTERED INDEX ix_Hardware_AvailabilityFee
    ON [Hardware].[AvailabilityFee] (Country, Wg)
    INCLUDE ([InstalledBaseHighAvailability],[CostPerKit],[CostPerKitJapanBuy],[MaxQty],[JapanBuy],[InstalledBaseHighAvailability_Approved],[CostPerKit_Approved],[CostPerKitJapanBuy_Approved],[MaxQty_Approved])
GO

CREATE NONCLUSTERED INDEX ix_Hardware_LogisticsCosts
    ON [Hardware].[LogisticsCosts] (Country, Wg, ReactionTimeType)
    INCLUDE ([StandardHandling],[HighAvailabilityHandling],[StandardDelivery],[ExpressDelivery],[TaxiCourierDelivery],[ReturnDeliveryFactory],[StandardHandling_Approved],[HighAvailabilityHandling_Approved],[StandardDelivery_Approved],[ExpressDelivery_Approved],[TaxiCourierDelivery_Approved],[ReturnDeliveryFactory_Approved])
GO

CREATE NONCLUSTERED INDEX ix_Atom_MarkupOtherCosts
    ON [Hardware].[MarkupOtherCosts] ([Country],[Wg], ReactionTimeTypeAvailability)
    INCLUDE (MarkupFactor, MarkupFactor_Approved, Markup, Markup_Approved)
GO

CREATE NONCLUSTERED INDEX ix_Atom_MarkupStandardWaranty
    ON [Hardware].[MarkupStandardWaranty] ([Country],[Wg], [ReactionTimeTypeAvailability])
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

IF OBJECT_ID('Hardware.GetCostsFull') IS NOT NULL
  DROP FUNCTION Hardware.GetCostsFull;
go 

IF OBJECT_ID('Hardware.GetCalcMember') IS NOT NULL
  DROP FUNCTION Hardware.GetCalcMember;
go 

IF OBJECT_ID('Portfolio.GetBySla') IS NOT NULL
  DROP FUNCTION Portfolio.GetBySla;
go 

IF OBJECT_ID('Portfolio.GetBySlaPaging') IS NOT NULL
  DROP FUNCTION Portfolio.GetBySlaPaging;
go 

IF OBJECT_ID('Hardware.CalcFieldServiceCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcFieldServiceCost;
go 

IF OBJECT_ID('Hardware.CalcHddRetention') IS NOT NULL
  DROP FUNCTION Hardware.CalcHddRetention;
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

IF OBJECT_ID('Hardware.ManualCostView', 'V') IS NOT NULL
  DROP VIEW Hardware.ManualCostView;
go

IF OBJECT_ID('Hardware.MarkupOtherCostsView', 'V') IS NOT NULL
  DROP VIEW Hardware.MarkupOtherCostsView;
go

IF OBJECT_ID('Hardware.MarkupStandardWarantyView', 'V') IS NOT NULL
  DROP VIEW Hardware.MarkupStandardWarantyView;
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

IF OBJECT_ID('Hardware.AddMarkupFactorOrFixValue') IS NOT NULL
  DROP FUNCTION Hardware.AddMarkupFactorOrFixValue;
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

           , max(case when y.IsProlongation = 0 and y.Value = 1  then ReinsuranceFlatfee end) 
           , max(case when y.IsProlongation = 0 and y.Value = 2  then ReinsuranceFlatfee end) 
           , max(case when y.IsProlongation = 0 and y.Value = 3  then ReinsuranceFlatfee end) 
           , max(case when y.IsProlongation = 0 and y.Value = 4  then ReinsuranceFlatfee end) 
           , max(case when y.IsProlongation = 0 and y.Value = 5  then ReinsuranceFlatfee end) 
           , max(case when y.IsProlongation = 1 and y.Value = 1  then ReinsuranceFlatfee end) 

           , max(case when y.IsProlongation = 0 and y.Value = 1  then ReinsuranceFlatfee_Approved end) 
           , max(case when y.IsProlongation = 0 and y.Value = 2  then ReinsuranceFlatfee_Approved end) 
           , max(case when y.IsProlongation = 0 and y.Value = 3  then ReinsuranceFlatfee_Approved end) 
           , max(case when y.IsProlongation = 0 and y.Value = 4  then ReinsuranceFlatfee_Approved end) 
           , max(case when y.IsProlongation = 0 and y.Value = 5  then ReinsuranceFlatfee_Approved end) 
           , max(case when y.IsProlongation = 1 and y.Value = 1  then ReinsuranceFlatfee_Approved end) 

           , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor end) 
           , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor end) 
           , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor end) 

           , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor_Approved end) 
           , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor_Approved end) 
           , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor_Approved end) 

    from Hardware.Reinsurance r
    join Dependencies.Year y on y.Id = r.Year

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
    [Wg] [bigint] NOT NULL foreign key references InputAtoms.Wg(Id),
    [Country] [bigint] NULL foreign key references InputAtoms.Country(Id),
    [Duration] [bigint] NOT NULL foreign key references Dependencies.Duration(Id),
    [ReactionTimeId] [bigint] NOT NULL foreign key references Dependencies.ReactionTime(id),
    [ReactionTypeId] [bigint] NOT NULL foreign key references Dependencies.ReactionType(id)
)
GO

CREATE NONCLUSTERED INDEX [IX_HwStandardWarranty_Wg] ON Fsp.HwStandardWarranty
(
	[Wg] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_HwStandardWarranty_Country] ON Fsp.HwStandardWarranty
(
	[Country] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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

    insert into Fsp.HwStandardWarranty(Wg, Country, Duration, ReactionTimeId, ReactionTypeId)
    select WgId
         , CountryId
         , max(DurationId) as DurationId
         , max(ReactionTimeId) as ReactionTimeId
         , max(ReactionTypeId) as ReactionTypeId
    from Fsp.HwFspCodeTranslation fsp
    where fsp.IsStandardWarranty = 1 
    group by WgId, CountryId, DurationId;

    -- Enable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty CHECK CONSTRAINT ALL;

END
GO

IF OBJECT_ID('Fsp.HwStandardWarrantyView', 'V') IS NOT NULL
  DROP VIEW Fsp.HwStandardWarrantyView;
go

CREATE VIEW [Fsp].[HwStandardWarrantyView] AS
    SELECT std.Wg
         , std.Country
         , std.Duration
         , dur.Name
         , dur.IsProlongation
         , dur.Value as DurationValue
         , std.ReactionTimeId
         , std.ReactionTypeId
    FROM fsp.HwStandardWarranty std
    INNER JOIN Dependencies.Duration dur on dur.Id = std.Duration
GO

update Fsp.HwFspCodeTranslation set Name = Name;
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

CREATE FUNCTION [Hardware].[AddMarkupFactorOrFixValue](
    @value float,
    @markupFactor float,
    @fixed float
)
RETURNS float
AS
BEGIN

    if @markupFactor > 0
        begin
            set @value = @value * @markupFactor;
        end
    else if @fixed > 0
        begin
            set @value = @fixed;
        end

    RETURN @value;

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

CREATE FUNCTION [Hardware].[CalcHddRetention](@cost float, @fr float)
RETURNS float
AS
BEGIN
    RETURN @cost * @fr;
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
    with WgCte as (
        select wg.*
             , case 
                    when wg.WgType = 0 then 1
                    else 0
               end as IsMultiVendor
        from InputAtoms.Wg wg
    )
    select fee.Country,
           fee.Wg,
           wg.IsMultiVendor, 
           
           fee.InstalledBaseHighAvailability as IB,
           fee.InstalledBaseHighAvailability_Approved as IB_Approved,
           
           fee.TotalLogisticsInfrastructureCost          / er.Value as TotalLogisticsInfrastructureCost,
           fee.TotalLogisticsInfrastructureCost_Approved / er.Value as TotalLogisticsInfrastructureCost_Approved,

           (case 
                when wg.IsMultiVendor = 1 then fee.StockValueMv 
                else fee.StockValueFj 
            end) / er.Value as StockValue,

           (case  
                when wg.IsMultiVendor = 1 then fee.StockValueMv_Approved 
                else fee.StockValueFj_Approved 
            end) / er.Value as StockValue_Approved,
       
           fee.AverageContractDuration,
           fee.AverageContractDuration_Approved,
       
           case when fee.JapanBuy = 1          then fee.CostPerKitJapanBuy else fee.CostPerKit end as CostPerKit,
        
           case when fee.JapanBuy_Approved = 1 then fee.CostPerKitJapanBuy else fee.CostPerKit end as CostPerKit_Approved,
        
           fee.MaxQty

    from Hardware.AvailabilityFee fee
    JOIN WgCte wg on wg.Id = fee.Wg
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

CREATE TRIGGER Hardware.HddRetentionUpdated
ON Hardware.HddRetention
After INSERT, UPDATE
AS BEGIN

    with cte as (
        select    h.Id
                , h.Wg
                , h.HddMaterialCost * h.HddFr / 100 as hddRetPerYear
                , h.HddMaterialCost_Approved * h.HddFr_Approved / 100 as hddRetPerYear_Approved
                , y.IsProlongation
                , y.Value
        from Hardware.HddRetention h
        join Dependencies.Year y on y.Id = h.Year
    )
    , cte2 as (
        select *
        from cte c
            cross apply(select sum(c2.hddRetPerYear) as HddRet, 
                               sum(c2.hddRetPerYear_Approved) as HddRet_Approved
                            from cte as c2
                            where c2.Wg = c.Wg and c2.IsProlongation = c.IsProlongation and c2.Value <= c.Value) ca
    )
    update h
        set h.HddRet = c.HddRet, HddRet_Approved = c.HddRet_Approved
    from Hardware.HddRetention h
    join cte2 c on c.Id = h.Id

END
go

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

CREATE VIEW Hardware.TaxAndDutiesVIEW as
    select Country,
           (TaxAndDuties / 100) as TaxAndDuties, 
           (TaxAndDuties_Approved / 100) as TaxAndDuties_Approved 
    from Hardware.TaxAndDuties
    where DeactivatedDateTime is null
GO

CREATE VIEW InputAtoms.WgView WITH SCHEMABINDING as
    SELECT wg.Id, 
           wg.Name, 
           case 
                when wg.WgType = 0 then 1
                else 0
            end as IsMultiVendor, 
           pla.Id as Pla, 
           cpla.Id as ClusterPla
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

CREATE VIEW [Hardware].[MarkupOtherCostsView] as 
    select   m.Country
           , m.Wg
           , tta.ReactionTimeId
           , tta.ReactionTypeId
           , tta.AvailabilityId
           , m.Markup
           , m.Markup_Approved
           , (m.MarkupFactor / 100) as MarkupFactor
           , (m.MarkupFactor_Approved / 100) as MarkupFactor_Approved
    from Hardware.MarkupOtherCosts m
    join Dependencies.ReactionTime_ReactionType_Avalability tta on tta.id = m.ReactionTimeTypeAvailability
GO

CREATE VIEW [Hardware].[MarkupStandardWarantyView] as 
    select m.Country,
           m.Wg,
           tta.ReactionTimeId,
           tta.ReactionTypeId,
           tta.AvailabilityId,
           (m.MarkupFactorStandardWarranty / 100) as MarkupFactorStandardWarranty,
           (m.MarkupFactorStandardWarranty_Approved / 100) as MarkupFactorStandardWarranty_Approved,
           m.MarkupStandardWarranty,
           m.MarkupStandardWarranty_Approved
    from Hardware.MarkupStandardWaranty m
    join Dependencies.ReactionTime_ReactionType_Avalability tta on tta.id = m.ReactionTimeTypeAvailability
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

CREATE VIEW [Hardware].[ReinsuranceView] as
    SELECT r.Wg, 
           r.Year,
           rta.AvailabilityId, 
           rta.ReactionTimeId,

           r.ReinsuranceFlatfee * r.ReinsuranceUpliftFactor / 100 / er.Value as Cost,

           r.ReinsuranceFlatfee_Approved * r.ReinsuranceUpliftFactor_Approved / 100 / er2.Value as Cost_Approved

    FROM Hardware.Reinsurance r
    JOIN Dependencies.ReactionTime_Avalability rta on rta.Id = r.ReactionTimeAvailability
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
    LEFT JOIN [References].ExchangeRate er2 on er2.CurrencyId = r.CurrencyReinsurance_Approved
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

CREATE VIEW [Hardware].[ManualCostView] as
    select    man.PortfolioId

            , man.ChangeUserId
            , u.Name  as ChangeUserName
            , u.Email as ChangeUserEmail

            , man.ServiceTC   / er.Value as ServiceTC   
            , man.ServiceTP   / er.Value as ServiceTP   

            , man.ServiceTP_Released / er.Value as ServiceTP_Released

            , man.ListPrice   / er.Value as ListPrice   
            , man.DealerPrice / er.Value as DealerPrice 
            , man.DealerDiscount

    from Hardware.ManualCost man
    join Portfolio.LocalPortfolio p on p.Id = man.PortfolioId
    join InputAtoms.Country c on c.Id = p.CountryId
    join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
    left join dbo.[User] u on u.Id = man.ChangeUserId
GO

CREATE FUNCTION [Portfolio].[GetBySla](
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select m.*
        from Portfolio.LocalPortfolio m
        where   m.CountryId = @cnt
            and (@wg is null or m.WgId = @wg)
            and (@av is null or m.AvailabilityId = @av)
            and (@dur is null or m.DurationId = @dur)
            and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
            and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
            and (@loc is null or          m.ServiceLocationId = @loc)
            and (@pro is null or          m.ProActiveSlaId = @pro)
)
GO

CREATE FUNCTION [Portfolio].[GetBySlaPaging](
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint,
    @lastid bigint,
    @limit int
)
RETURNS @tbl TABLE 
            (   
                [rownum] [int] NOT NULL,
                [Id] [bigint] NOT NULL,
                [CountryId] [bigint] NOT NULL,
                [WgId] [bigint] NOT NULL,
                [AvailabilityId] [bigint] NOT NULL,
                [DurationId] [bigint] NOT NULL,
                [ReactionTimeId] [bigint] NOT NULL,
                [ReactionTypeId] [bigint] NOT NULL,
                [ServiceLocationId] [bigint] NOT NULL,
                [ProActiveSlaId] [bigint] NOT NULL,
                [SlaHash] [int] NOT NULL
            )
AS
BEGIN

    if @limit > 0
        begin
            with SlaCte as (
                select ROW_NUMBER() over(
                            order by m.CountryId
                                   , m.WgId
                                   , m.AvailabilityId
                                   , m.DurationId
                                   , m.ReactionTimeId
                                   , m.ReactionTypeId
                                   , m.ServiceLocationId
                                   , m.ProActiveSlaId
                        ) as rownum
                     , m.*
                    from Portfolio.LocalPortfolio m
                    where   (m.CountryId = @cnt)
                        and (@wg is null or m.WgId = @wg)
                        and (@av is null or m.AvailabilityId = @av)
                        and (@dur is null or m.DurationId = @dur)
                        and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
                        and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
                        and (@loc is null or          m.ServiceLocationId = @loc)
                        and (@pro is null or          m.ProActiveSlaId = @pro)
            )
            insert @tbl
            select top(@limit)
                    rownum, Id, CountryId, WgId, AvailabilityId, DurationId, ReactionTimeId, ReactionTypeId, ServiceLocationId, ProActiveSlaId, SlaHash
            from SlaCte where rownum > @lastid
        end
    else
        begin
            insert @tbl
            select -1 as rownum, Id, CountryId, WgId, AvailabilityId, DurationId, ReactionTimeId, ReactionTypeId, ServiceLocationId, ProActiveSlaId, SlaHash
            from Portfolio.LocalPortfolio m
            where   (m.CountryId = @cnt)
                and (@wg is null or m.WgId = @wg)
                and (@av is null or m.AvailabilityId = @av)
                and (@dur is null or m.DurationId = @dur)
                and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
                and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
                and (@loc is null or          m.ServiceLocationId = @loc)
                and (@pro is null or          m.ProActiveSlaId = @pro)

             order by m.CountryId
                    , m.WgId
                    , m.AvailabilityId
                    , m.DurationId
                    , m.ReactionTimeId
                    , m.ReactionTypeId
                    , m.ServiceLocationId
                    , m.ProActiveSlaId;

        end

    RETURN;
END;
go

CREATE FUNCTION [Hardware].[GetCalcMember] (
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with Cte as (
        SELECT 
                   m.*

                 , case when stdw.DurationValue is not null then stdw.DurationValue 
                        when stdw2.DurationValue is not null then stdw2.DurationValue 
                    end as StdWarranty
                 , case when stdw.DurationValue is not null then stdw.ReactionTimeId 
                        when stdw2.DurationValue is not null then stdw2.ReactionTimeId  
                    end as StdwReactionTimeId 
                 , case when stdw.DurationValue is not null then stdw.ReactionTypeId 
                        when stdw2.DurationValue is not null then stdw2.ReactionTypeId 
                    end as StdwReactionTypeId

        FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId --find local standard warranty portfolio
        LEFT JOIN Fsp.HwStandardWarrantyView stdw2 on stdw2.Wg = m.WgId and stdw2.Country is null    --find principle standard warranty portfolio, if local does not exist

    )
    SELECT m.Id

        --SLA

         , m.CountryId          
         , c.Name               as Country
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

         , m.StdWarranty

         , case when @approved = 0 then afr.AFR1  else AFR1_Approved       end as AFR1 
         , case when @approved = 0 then afr.AFR2  else AFR2_Approved       end as AFR2 
         , case when @approved = 0 then afr.AFR3  else afr.AFR3_Approved   end as AFR3 
         , case when @approved = 0 then afr.AFR4  else afr.AFR4_Approved   end as AFR4 
         , case when @approved = 0 then afr.AFR5  else afr.AFR5_Approved   end as AFR5 
         , case when @approved = 0 then afr.AFRP1 else afr.AFRP1_Approved  end as AFRP1
       
         , case when @approved = 0 then hdd.HddRet                         else hdd.HddRet_Approved                  end as HddRet              
         
         , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved    end as MaterialCostWarranty
         , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved         end as MaterialCostOow     

         , case when @approved = 0 then tax.TaxAndDuties                   else tax.TaxAndDuties_Approved            end as TaxAndDuties

         , case when @approved = 0 then r.Cost                             else r.Cost_Approved                      end as Reinsurance
         , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved              end as LabourCost             
         , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved              end as TravelCost             
         , case when @approved = 0 then fsc.TimeAndMaterialShare           else fsc.TimeAndMaterialShare_Approved    end as TimeAndMaterialShare   
         , case when @approved = 0 then fsc.PerformanceRate                else fsc.PerformanceRate_Approved         end as PerformanceRate        
         , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved              end as TravelTime             
         , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved              end as RepairTime             
         , case when @approved = 0 then fsc.OnsiteHourlyRates              else fsc.OnsiteHourlyRates_Approved       end as OnsiteHourlyRates      
                  
         , case when @approved = 0 then ssc.[1stLevelSupportCosts]         else ssc.[1stLevelSupportCosts_Approved]  end as [1stLevelSupportCosts] 
         , case when @approved = 0 then ssc.[2ndLevelSupportCosts]         else ssc.[2ndLevelSupportCosts_Approved]  end as [2ndLevelSupportCosts] 

         , case when @approved = 0 then ssc.ServiceSupport                 else ssc.ServiceSupport_Approved          end as ServiceSupport
         
         , case when @approved = 0 then lcs.ExpressDelivery                 else lcs.ExpressDelivery_Approved          end as StdExpressDelivery         
         , case when @approved = 0 then lcs.HighAvailabilityHandling        else lcs.HighAvailabilityHandling_Approved end as StdHighAvailabilityHandling
         , case when @approved = 0 then lcs.StandardDelivery                else lcs.StandardDelivery_Approved         end as StdStandardDelivery        
         , case when @approved = 0 then lcs.StandardHandling                else lcs.StandardHandling_Approved         end as StdStandardHandling        
         , case when @approved = 0 then lcs.ReturnDeliveryFactory           else lcs.ReturnDeliveryFactory_Approved    end as StdReturnDeliveryFactory   
         , case when @approved = 0 then lcs.TaxiCourierDelivery             else lcs.TaxiCourierDelivery_Approved      end as StdTaxiCourierDelivery     

         , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved          end as ExpressDelivery         
         , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling
         , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved         end as StandardDelivery        
         , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved         end as StandardHandling        
         , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved    end as ReturnDeliveryFactory   
         , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved      end as TaxiCourierDelivery     

         , case when afEx.id is null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                 else 0
           end as AvailabilityFee

         , case when @approved = 0 then moc.Markup                         else moc.Markup_Approved                       end as MarkupOtherCost                      
         , case when @approved = 0 then moc.MarkupFactor                   else moc.MarkupFactor_Approved                 end as MarkupFactorOtherCost                

         , case when @approved = 0 then msw.MarkupFactorStandardWarranty   else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty
         , case when @approved = 0 then msw.MarkupStandardWarranty         else msw.MarkupStandardWarranty_Approved       end as MarkupStandardWarranty      

         , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate
                else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved
            end as LocalRemoteAccessSetup

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

         , man.ListPrice       as ListPrice                   
         , man.DealerDiscount  as DealerDiscount              
         , man.DealerPrice     as DealerPrice                 
         , man.ServiceTC       as ServiceTCManual                   
         , man.ServiceTP       as ServiceTPManual                   
         , man.ServiceTP_Released as ServiceTP_Released                  
         , man.ChangeUserName  as ChangeUserName
         , man.ChangeUserEmail as ChangeUserEmail

         , m.SlaHash

    FROM Cte m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.HddRetention hdd on hdd.Wg = m.WgId AND hdd.Year = m.DurationId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Year = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Hardware.LogisticsCostView lcs on lcs.Country = m.CountryId AND lcs.Wg = m.WgId AND lcs.ReactionTime = m.StdwReactionTimeId AND lcs.ReactionType = m.StdwReactionTypeId

    LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeId = m.ReactionTimeId AND moc.ReactionTypeId = m.ReactionTypeId AND moc.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCostView man on man.PortfolioId = m.Id
)
GO

CREATE FUNCTION [Hardware].[GetCostsFull](
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint,
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

                , m.Year * m.ServiceSupport as ServiceSupportCost

                , m.TravelCost + m.LabourCost as FieldServicePerYearStdw

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
                , m.matO1P * m.AFRP1 as taxO1P

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

                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw1, m.ServiceSupport, m.LogisticStdw1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw2, m.ServiceSupport, m.LogisticStdw2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw3, m.ServiceSupport, m.LogisticStdw3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw4, m.ServiceSupport, m.LogisticStdw4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw5, m.ServiceSupport, m.LogisticStdw5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5
                , 0     as LocalServiceStandardWarranty1P

        from CostCte2_2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
             , 0 as Credit1P
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*
             , m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero - m.Credit1  as ServiceTC1
             , m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero - m.Credit2  as ServiceTC2
             , m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero - m.Credit3  as ServiceTC3
             , m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero - m.Credit4  as ServiceTC4
             , m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero - m.Credit5  as ServiceTC5
             , m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero - m.Credit1P as ServiceTC1P
        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
             , Hardware.AddMarkup(m.ServiceTC1,  1 + m.MarkupFactorOtherCost, m.MarkupOtherCost) as ServiceTP1
             , Hardware.AddMarkup(m.ServiceTC2,  1 + m.MarkupFactorOtherCost, m.MarkupOtherCost) as ServiceTP2
             , Hardware.AddMarkup(m.ServiceTC3,  1 + m.MarkupFactorOtherCost, m.MarkupOtherCost) as ServiceTP3
             , Hardware.AddMarkup(m.ServiceTC4,  1 + m.MarkupFactorOtherCost, m.MarkupOtherCost) as ServiceTP4
             , Hardware.AddMarkup(m.ServiceTC5,  1 + m.MarkupFactorOtherCost, m.MarkupOtherCost) as ServiceTP5
             , Hardware.AddMarkup(m.ServiceTC1P, 1 + m.MarkupFactorOtherCost, m.MarkupOtherCost) as ServiceTP1P
        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
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

         , m.StdWarranty

         --Cost

         , m.AvailabilityFee * m.Year as AvailabilityFee
         , m.HddRet
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.tax1, m.tax2, m.tax3, m.tax4, m.tax5, m.tax1P) as TaxAndDutiesW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.taxO1, m.taxO2, m.taxO3, m.taxO4, m.taxO5, m.taxO1P) as TaxAndDutiesOow
         , m.Reinsurance
         , m.ProActive
         , m.ServiceSupportCost

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.mat1, m.mat2, m.mat3, m.mat4, m.mat5, m.mat1P) as MaterialW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.matO1, m.matO2, m.matO3, m.matO4, m.matO5, m.matO1P) as MaterialOow
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
         
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.LocalServiceStandardWarranty1, m.LocalServiceStandardWarranty2, m.LocalServiceStandardWarranty3, m.LocalServiceStandardWarranty4, m.LocalServiceStandardWarranty5, m.LocalServiceStandardWarranty1P) as LocalServiceStandardWarranty
         
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Credit1, m.Credit2, m.Credit3, m.Credit4, m.Credit5, m.Credit1P) as Credits


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
         , m.ChangeUserName
         , m.ChangeUserEmail

         , m.ServiceTP_Released

         , m.SlaHash

       from CostCte6 m
)
go

CREATE FUNCTION [Hardware].[GetCosts](
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    select Id

         , Country
         , Wg
         , Availability
         , Duration
         , ReactionTime
         , ReactionType
         , ServiceLocation
         , ProActiveSla

         , StdWarranty

         , AvailabilityFee
         , HddRet
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

         , ListPrice
         , DealerDiscount
         , DealerPrice
         , ServiceTCManual
         , ServiceTPManual
         , ChangeUserName
         , ChangeUserEmail

         , ServiceTP_Released

    from Hardware.GetCostsFull(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit)
)
go

CREATE PROCEDURE [Hardware].[SpGetCosts]
    @approved bit,
    @local bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

    select @total = COUNT(id)
    from Portfolio.LocalPortfolio m
    where   (m.CountryId = @cnt)
        and (@wg is null            or m.WgId = @wg)
        and (@av is null            or m.AvailabilityId = @av)
        and (@dur is null           or m.DurationId = @dur)
        and (@reactiontime is null  or m.ReactionTimeId = @reactiontime)
        and (@reactiontype is null  or m.ReactionTypeId = @reactiontype)
        and (@loc is null           or m.ServiceLocationId = @loc)
        and (@pro is null           or m.ProActiveSlaId = @pro);


    declare @cur nvarchar(max);
    declare @exchange float;

    select @cur = cur.Name
         , @exchange =  er.Value 
    from [References].Currency cur
    join [References].ExchangeRate er on er.CurrencyId = cur.Id
    where cur.Id = (select CurrencyId from InputAtoms.Country where id = @cnt);

    if @local = 1
    begin
    
        --convert values from EUR to local

        select Id

             , Country
             , @cur as Currency
             , @exchange as ExchangeRate

             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty

             --Cost

             , AvailabilityFee               * @exchange  as AvailabilityFee 
             , HddRet                        * @exchange  as HddRet
             , TaxAndDutiesW                 * @exchange  as TaxAndDutiesW
             , TaxAndDutiesOow               * @exchange  as TaxAndDutiesOow
             , Reinsurance                   * @exchange  as Reinsurance
             , ProActive                     * @exchange  as ProActive
             , ServiceSupportCost            * @exchange  as ServiceSupportCost

             , MaterialW                     * @exchange  as MaterialW
             , MaterialOow                   * @exchange  as MaterialOow
             , FieldServiceCost              * @exchange  as FieldServiceCost
             , Logistic                      * @exchange  as Logistic
             , OtherDirect                   * @exchange  as OtherDirect
             , LocalServiceStandardWarranty  * @exchange  as LocalServiceStandardWarranty
             , Credits                       * @exchange  as Credits
             , ServiceTC                     * @exchange  as ServiceTC
             , ServiceTP                     * @exchange  as ServiceTP

             , ServiceTCManual               * @exchange  as ServiceTCManual
             , ServiceTPManual               * @exchange  as ServiceTPManual

             , ServiceTP_Released            * @exchange as ServiceTP_Released

             , ListPrice                     * @exchange  as ListPrice
             , DealerPrice                   * @exchange  as DealerPrice
             , DealerDiscount                             as DealerDiscount

             , ChangeUserName                             as ChangeUserName
             , ChangeUserEmail                            as ChangeUserEmail

        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit)
        order by Id
        
    end
    else
    begin

        select @cur as Currency, @exchange as ExchangeRate, m.*
        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
        order by Id

    end

END

GO
