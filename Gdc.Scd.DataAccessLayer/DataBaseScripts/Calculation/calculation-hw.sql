IF OBJECT_ID('Hardware.StandardWarrantyManualCost', 'U') IS NOT NULL
  DROP TABLE Hardware.StandardWarrantyManualCost;
go

CREATE TABLE Hardware.StandardWarrantyManualCost (
    [Id] [bigint] primary key IDENTITY(1,1) NOT NULL,
    [CountryId] [bigint] NOT NULL foreign key references InputAtoms.Country(Id),
    [WgId] [bigint] NOT NULL foreign key references InputAtoms.Wg(Id),
    [ChangeUserId] [bigint] NOT NULL foreign key references dbo.[User](Id),
    [ChangeDate] [datetime] NOT NULL,
    [StandardWarranty] [float] NULL
)
GO

CREATE UNIQUE INDEX [ix_StandardWarrantyManualCost_Country_Wg] ON [Hardware].[StandardWarrantyManualCost]([CountryId] ASC, [WgId] ASC)
GO

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

IF OBJECT_ID('Hardware.MaterialCostWarrantyCalc', 'U') IS NOT NULL
  DROP TABLE Hardware.MaterialCostWarrantyCalc;
go

CREATE TABLE Hardware.MaterialCostWarrantyCalc (
    [Country] [bigint] NOT NULL foreign key references InputAtoms.Country(Id),
    [Wg] [bigint] NOT NULL foreign key references InputAtoms.Wg(Id),
    [MaterialCostOow] [float] NULL,
    [MaterialCostOow_Approved] [float] NULL,
	[MaterialCostIw] [float] NULL,
	[MaterialCostIw_Approved] [float] NULL,
    CONSTRAINT PK_MaterialCostOowCalc PRIMARY KEY NONCLUSTERED (Country, Wg)
)
GO

IF OBJECT_ID('Hardware.MaterialCostUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.MaterialCostUpdated;
go

CREATE TRIGGER Hardware.MaterialCostUpdated
ON Hardware.MaterialCostWarranty
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostCalc;
END
go

IF OBJECT_ID('Hardware.MaterialCostWarrantyEmeiaUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.MaterialCostWarrantyEmeiaUpdated;
go

CREATE TRIGGER Hardware.MaterialCostWarrantyEmeiaUpdated
ON Hardware.MaterialCostWarrantyEmeia
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostCalc;
END
go

exec Hardware.SpUpdateMaterialCostCalc;
go

ALTER TABLE Hardware.ManualCost
   DROP COLUMN DealerPrice;

ALTER TABLE Hardware.ManualCost
   ADD DealerPrice as (ListPrice - (ListPrice * DealerDiscount / 100));
GO

ALTER TABLE Hardware.ManualCost
   DROP COLUMN ServiceTP_Released;

ALTER TABLE Hardware.ManualCost
   ADD ServiceTP_Released AS ( ServiceTP1_Released + 
							COALESCE(ServiceTP2_Released, 0) + 
							COALESCE(ServiceTP3_Released, 0) + 
							COALESCE(ServiceTP4_Released, 0) + 
							COALESCE(ServiceTP5_Released, 0));
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
go

CREATE NONCLUSTERED INDEX [ix_RoleCodeHourlyRates_Country_RoleCode] ON [Hardware].[RoleCodeHourlyRates]
(
	[Country] ASC,
	[RoleCode] ASC
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX ix_Hardware_ProActive
    ON [Hardware].[ProActive] ([Country],[Wg])
GO

IF OBJECT_ID('Hardware.GetCosts') IS NOT NULL
  DROP FUNCTION Hardware.GetCosts;
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

IF OBJECT_ID('Hardware.PositiveValue') IS NOT NULL
    DROP FUNCTION [Hardware].[PositiveValue]
GO

CREATE FUNCTION [Hardware].[PositiveValue] (@value float)
RETURNS float 
AS
BEGIN

    if @value < 0
        begin
            return 0;
        end

    RETURN @value;

END
GO

alter table Hardware.Reinsurance
    add ReinsuranceFlatfee_norm          as (ReinsuranceFlatfee * coalesce(ReinsuranceUpliftFactor / 100, 1))
      , ReinsuranceFlatfee_norm_Approved as (ReinsuranceFlatfee_Approved * coalesce(ReinsuranceUpliftFactor_Approved / 100, 1))
GO

CREATE VIEW [InputAtoms].[WgStdView] AS
    select *
    from InputAtoms.Wg
    where Id in (select Wg from Fsp.HwStandardWarranty) and WgType = 1
GO

if OBJECT_ID('Fsp.LutPriority', 'U') is not null
    drop table Fsp.LutPriority;
go

create table Fsp.LutPriority(
      LUT nvarchar(20) not null
    , Priority int not null
)
go

insert into Fsp.LutPriority(LUT, Priority) 
    values 
      ('ASP', 1)
    , ('BEL', 1)
    , ('CAM', 2)
    , ('CRE', 1)
    , ('D',   1)
    , ('DAN', 1)
    , ('FAM', 1)
    , ('FIN', 1)
    , ('FKR', 1)
    , ('FUJ', 3)
    , ('GBR', 1)
    , ('GRI', 1)
    , ('GSP', 1)
    , ('IND', 1)
    , ('INT', 1)
    , ('ISR', 1)
    , ('ITL', 1)
    , ('LUX', 1)
    , ('MDE', 1)
    , ('ND',  2)
    , ('NDL', 1)
    , ('NOA', 1)
    , ('NOR', 1)
    , ('OES', 1)
    , ('POL', 1)
    , ('POR', 1)
    , ('RSA', 1)
    , ('RUS', 1)
    , ('SEE', 1)
    , ('SPA', 1)
    , ('SWD', 1)
    , ('SWZ', 1)
    , ('TRK', 1)
    , ('UNG', 1);
go

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

    if @markupFactor is not null
        begin
            set @value = @value * @markupFactor;
        end
    else if @markup is not null
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

    if @fixed is not null and @fixed <> 0
        begin
            return @fixed;
        end

    if @markupFactor is not null and @markupFactor <> 0
        begin
            return @value * @markupFactor;
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
    @srvSupportCost   float,
    @logisticCost     float,
    @taxAndDutiesW    float,
    @afr              float,
    @fee              float,
    @markupFactor     float,
    @markup           float,
    @sarCoeff         float
)
RETURNS float
AS
BEGIN
    return Hardware.AddMarkup(@fieldServiceCost + @srvSupportCost * @sarCoeff + @logisticCost, @markupFactor, @markup) + @taxAndDutiesW * @afr + @fee;
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
    select   feeCountryWg.Country
           , feeCountryWg.Wg
           
           , case  when wg.WgType = 0 then 1 else 0 end as IsMultiVendor
           
           , feeCountryWg.InstalledBaseHighAvailability as IB
           , feeCountryWg.InstalledBaseHighAvailability_Approved as IB_Approved
           
           , feeCountryWg.TotalLogisticsInfrastructureCost          / er.Value as TotalLogisticsInfrastructureCost
           , feeCountryWg.TotalLogisticsInfrastructureCost_Approved / er.Value as TotalLogisticsInfrastructureCost_Approved
           
           , case when wg.WgType = 0 then feeCountryWg.StockValueMv          else feeCountryWg.StockValueFj          end / er.Value as StockValue
           , case when wg.WgType = 0 then feeCountryWg.StockValueMv_Approved else feeCountryWg.StockValueFj_Approved end / er.Value as StockValue_Approved
           
           , feeCountryWg.AverageContractDuration
           , feeCountryWg.AverageContractDuration_Approved
           
           , case when feeCountryWg.JapanBuy = 1          then feeWg.CostPerKitJapanBuy else feeWg.CostPerKit end as CostPerKit
           , case when feeCountryWg.JapanBuy_Approved = 1 then feeWg.CostPerKitJapanBuy else feeWg.CostPerKit end as CostPerKit_Approved
           
           , feeWg.MaxQty

    from Hardware.AvailabilityFeeCountryWg AS feeCountryWg
	JOIN Hardware.AvailabilityFeeWg AS feeWg ON feeCountryWg.Wg = feeWg.Wg
    JOIN InputAtoms.Wg wg on wg.Id = feeCountryWg.Wg
    JOIN InputAtoms.Country c on c.Id = feeCountryWg.Country
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where 
		feeCountryWg.DeactivatedDateTime is null and 
		feeWg.DeactivatedDateTime is null and 
		wg.DeactivatedDateTime is null
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
           case when IB > 0 and fee.MaxQty > 0 then Hardware.CalcAvailabilityFee(fee.CostPerKit, fee.MaxQty, fee.TISC, fee.YI, fee.Total_KC_MQ_IB_VENDOR) else 0 end as Fee,
           case when IB_Approved > 0 and fee.MaxQty > 0 then Hardware.CalcAvailabilityFee(fee.CostPerKit_Approved, fee.MaxQty, fee.TISC_Approved, fee.YI_Approved, fee.Total_KC_MQ_IB_VENDOR_Approved) else 0 end as Fee_Approved
    from AvFeeCte2 fee
GO

IF OBJECT_ID('[References].ExchangeRateUpdated', 'TR') IS NOT NULL
  DROP TRIGGER [References].ExchangeRateUpdated;
go

CREATE TRIGGER [References].ExchangeRateUpdated
ON [References].ExchangeRate
After INSERT, UPDATE
AS BEGIN
    exec Hardware.UpdateAvailabilityFee;
    exec Hardware.spUpdateReinsuranceCalc;
END
go

exec Hardware.UpdateAvailabilityFee;
exec Hardware.spUpdateReinsuranceCalc;
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

CREATE VIEW [Hardware].[HddRetentionView] as 
    SELECT 
           h.Wg as WgId
         , wg.Name as Wg
         , sog.Name as Sog
         , h.HddRet
         , HddRet_Approved
         , hm.TransferPrice 
         , hm.ListPrice
         , hm.DealerDiscount
         , hm.DealerPrice
         , u.Name as ChangeUserName
         , u.Email as ChangeUserEmail
         , hm.ChangeDate

    FROM Hardware.HddRetention h
    JOIN InputAtoms.Wg wg on wg.id = h.Wg
    LEFT JOIN InputAtoms.Sog sog on sog.id = wg.SogId
    LEFT JOIN Hardware.HddRetentionManualCost hm on hm.WgId = h.Wg
    LEFT JOIN [dbo].[User] u on u.Id = hm.ChangeUserId
    WHERE h.DeactivatedDateTime is null 
      AND h.Year = (select id from Dependencies.Year where Value = 5 and IsProlongation = 0)

GO

alter table Hardware.FieldServiceCost
    add TimeAndMaterialShare_norm          as (TimeAndMaterialShare / 100)
      , TimeAndMaterialShare_norm_Approved as (TimeAndMaterialShare_Approved / 100)
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

    CONSTRAINT PK_FieldServiceCalc PRIMARY KEY CLUSTERED (Country, Wg, ServiceLocation)
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

    CONSTRAINT PK_FieldServiceTimeCalc PRIMARY KEY CLUSTERED (Country, Wg, ReactionTimeType)
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

CREATE NONCLUSTERED INDEX ix_FieldServiceCalc_Country
ON [Hardware].[FieldServiceCalc] ([Country])
INCLUDE ([Wg],[ServiceLocation],[RepairTime],[RepairTime_Approved],[TravelTime],[TravelTime_Approved],[TravelCost],[TravelCost_Approved],[LabourCost],[LabourCost_Approved])
GO

CREATE NONCLUSTERED INDEX ix_FieldServiceTimeCalc_Country
ON [Hardware].[FieldServiceTimeCalc] ([Country])
INCLUDE ([Wg],[ReactionTimeType],[PerformanceRate],[PerformanceRate_Approved],[TimeAndMaterialShare_norm],[TimeAndMaterialShare_norm_Approved])
GO

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
           pla.ClusterPlaId as ClusterPla,
           wg.RoleCodeId
    from InputAtoms.Wg wg
    inner join InputAtoms.Pla pla on pla.id = wg.PlaId
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

    where lc.DeactivatedDateTime is null
GO

alter table Hardware.MarkupOtherCosts
    add
      [ProlongationMarkup]                float    
    , [ProlongationMarkup_Approved]       float
    , [ProlongationMarkupFactor]          float
    , [ProlongationMarkupFactor_Approved] float

    , [MarkupFactor_norm]  AS ([MarkupFactor]/(100)) PERSISTED
    , [MarkupFactor_norm_Approved]  AS ([MarkupFactor_Approved]/(100)) PERSISTED
    , [ProlongationMarkupFactor_norm]  AS ([ProlongationMarkupFactor]/(100)) PERSISTED
    , [ProlongationMarkupFactor_norm_Approved]  AS ([ProlongationMarkupFactor_Approved]/(100)) PERSISTED

    , Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
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

CREATE VIEW [Hardware].[ProActiveView] as 
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







