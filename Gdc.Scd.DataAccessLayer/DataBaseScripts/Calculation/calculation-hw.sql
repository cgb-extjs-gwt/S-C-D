ALTER TABLE Hardware.ManualCost
   DROP COLUMN DealerPrice, DealerPrice_Approved;

ALTER TABLE Hardware.ManualCost
   ADD DealerPrice as (ListPrice - (ListPrice * DealerDiscount / 100)),
       DealerPrice_Approved as (ListPrice_Approved - (ListPrice_Approved * DealerDiscount_Approved / 100));
GO

ALTER TABLE Hardware.InstallBase
    ADD InstalledBaseCountryPla float,
        InstalledBaseCountryPla_Approved float;
GO

ALTER TABLE Hardware.HddRetention
     ADD HddRet float,
         HddRet_Approved float
go

CREATE NONCLUSTERED INDEX ix_Hardware_FieldServiceCost
    ON [Hardware].[FieldServiceCost] ([Country],[Wg])
    INCLUDE ([ServiceLocation],[ReactionTimeType],[RepairTime],[TravelTime],[LabourCost],[TravelCost],[PerformanceRate],[TimeAndMaterialShare])
GO

CREATE NONCLUSTERED INDEX ix_Atom_InstallBase
    ON [Hardware].[InstallBase] ([Country],[Wg])
    INCLUDE ([InstalledBaseCountry],[InstalledBaseCountry_Approved],[InstalledBaseCountryPla],[InstalledBaseCountryPla_Approved])
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

IF OBJECT_ID('Hardware.GetCosts') IS NOT NULL
  DROP FUNCTION Hardware.GetCosts;
go 

IF OBJECT_ID('Hardware.GetCostsFull') IS NOT NULL
  DROP FUNCTION Hardware.GetCostsFull;
go 

IF OBJECT_ID('Hardware.GetCalcMember') IS NOT NULL
  DROP FUNCTION Hardware.GetCalcMember;
go 

IF OBJECT_ID('Matrix.GetBySla') IS NOT NULL
  DROP FUNCTION Matrix.GetBySla;
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

IF OBJECT_ID('Hardware.AFR_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.AFR_Updated;
go

CREATE TRIGGER Hardware.AFR_Updated
ON Hardware.AFR
After INSERT, UPDATE
AS BEGIN

    delete from Hardware.AfrYear;

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

    if @markupFactor is null
        begin
            set @value = @value + @markup;
        end
    else
        begin
            set @value = @value * @markupFactor;
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
                   @timeAndMaterialShare * (@travelTime + @repairTime) * @onsiteHourlyRate + 
                   @performanceRate
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
    @labourCost float,
    @travelCost float,
    @srvSupportCost float,
    @logisticCost float,
    @taxAndDutiesW float,
    @afr float,
    @availabilityFee float,
    @markupFactor float,
    @markup float
)
RETURNS float
AS
BEGIN
    declare @totalCost float = Hardware.AddMarkup(@labourCost + @travelCost + @srvSupportCost + @logisticCost, @markupFactor, @markup);
    declare @fee float = Hardware.AddMarkup(@availabilityFee, @markupFactor, @markup);

    return @afr * (@totalCost + @taxAndDutiesW) + @fee;
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
           
           fee.TotalLogisticsInfrastructureCost          * er.Value as TotalLogisticsInfrastructureCost,
           fee.TotalLogisticsInfrastructureCost_Approved * er.Value as TotalLogisticsInfrastructureCost_Approved,

           (case 
                when wg.IsMultiVendor = 1 then fee.StockValueMv 
                else fee.StockValueFj 
            end) * er.Value as StockValue,

           (case  
                when wg.IsMultiVendor = 1 then fee.StockValueMv_Approved 
                else fee.StockValueFj_Approved 
            end) * er.Value as StockValue_Approved,
       
           fee.AverageContractDuration,
           fee.AverageContractDuration_Approved,
       
           (case  
                when fee.JapanBuy = 1 then fee.CostPerKitJapanBuy
                else fee.CostPerKit
            end) as CostPerKit,
       
           (case  
                when fee.JapanBuy = 1 then fee.CostPerKitJapanBuy_Approved
                else fee.CostPerKit_Approved
            end) as CostPerKit_Approved,
       
            fee.MaxQty,
            fee.MaxQty_Approved

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

                   sum(fee.IsMultiVendor * fee.CostPerKit / fee.MaxQty * fee.IB) as Total_KC_MQ_IB_MVS,
                   sum(fee.IsMultiVendor * fee.CostPerKit_Approved / fee.MaxQty_Approved * fee.IB_Approved) as Total_KC_MQ_IB_MVS_Approved,

                   sum((1 - fee.IsMultiVendor) * fee.CostPerKit / fee.MaxQty * fee.IB) as Total_KC_MQ_IB_FTS,
                   sum((1 - fee.IsMultiVendor) * fee.CostPerKit_Approved / fee.MaxQty_Approved * fee.IB_Approved) as Total_KC_MQ_IB_FTS_Approved

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
           Hardware.CalcAvailabilityFee(fee.CostPerKit, fee.MaxQty, fee.TISC, fee.YI, fee.Total_KC_MQ_IB_VENDOR) as Fee,
           Hardware.CalcAvailabilityFee(fee.CostPerKit_Approved, fee.MaxQty_Approved, fee.TISC_Approved, fee.YI_Approved, fee.Total_KC_MQ_IB_VENDOR_Approved) as Fee_Approved
    from AvFeeCte2 fee
GO

IF OBJECT_ID('Hardware.InstallBaseUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.InstallBaseUpdated;
go

CREATE TRIGGER Hardware.InstallBaseUpdated
ON Hardware.InstallBase
After INSERT, UPDATE
AS BEGIN

    with InstallBasePlaCte (Country, Pla, totalIB)
    as
    (
        select Country, Pla, sum(InstalledBaseCountry) as totalIB
        from Hardware.InstallBase 
        where InstalledBaseCountry is not null
        group by Country, Pla
    )
    , InstallBasePla_Approved_Cte (Country, Pla, totalIB)
    as
    (
        select Country, Pla, sum(InstalledBaseCountry_Approved) as totalIB
        from Hardware.InstallBase 
        where InstalledBaseCountry_Approved is not null
        group by Country, Pla
    )
    update ib
        set 
            ib.InstalledBaseCountryPla = ibp.totalIB,
            ib.InstalledBaseCountryPla_Approved = ibp2.totalIB
    from Hardware.InstallBase ib
    LEFT JOIN InstallBasePlaCte ibp on ibp.Pla = ib.Pla and ibp.Country = ib.Country
    LEFT JOIN InstallBasePla_Approved_Cte ibp2 on ibp2.Pla = ib.Pla and ibp2.Country = ib.Country

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

            fsc.LabourCost          * er.Value as LabourCost,
            fsc.LabourCost_Approved * er.Value as LabourCost_Approved,

            fsc.TravelCost          * er.Value as TravelCost,
            fsc.TravelCost_Approved * er.Value as TravelCost_Approved,

            fsc.PerformanceRate          * er.Value as PerformanceRate,
            fsc.PerformanceRate_Approved * er.Value as PerformanceRate_Approved,

            (fsc.TimeAndMaterialShare / 100) as TimeAndMaterialShare,
            (fsc.TimeAndMaterialShare_Approved / 100) as TimeAndMaterialShare_Approved

    FROM Hardware.FieldServiceCost fsc
    JOIN InputAtoms.Country c on c.Id = fsc.Country
    JOIN InputAtoms.Wg on wg.Id = fsc.Wg
    JOIN Dependencies.ReactionTime_ReactionType rt on rt.Id = fsc.ReactionTimeType
    LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId
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
    from InputAtoms.Wg wg,
            InputAtoms.Pla pla,
            InputAtoms.ClusterPla cpla
    where wg.PlaId = pla.Id and cpla.id = pla.ClusterPlaId
GO

CREATE VIEW [Hardware].[LogisticsCostView] AS
    SELECT lc.Country,
           lc.Wg, 
           rt.ReactionTypeId as ReactionType, 
           rt.ReactionTimeId as ReactionTime,
           
           lc.StandardHandling          * er.Value as StandardHandling,
           lc.StandardHandling_Approved * er.Value as StandardHandling_Approved,

           lc.HighAvailabilityHandling          * er.Value as HighAvailabilityHandling,
           lc.HighAvailabilityHandling_Approved * er.Value as HighAvailabilityHandling_Approved,

           lc.StandardDelivery          * er.Value as StandardDelivery,
           lc.StandardDelivery_Approved * er.Value as StandardDelivery_Approved,

           lc.ExpressDelivery          * er.Value as ExpressDelivery,
           lc.ExpressDelivery_Approved * er.Value as ExpressDelivery_Approved,

           lc.TaxiCourierDelivery          * er.Value as TaxiCourierDelivery,
           lc.TaxiCourierDelivery_Approved * er.Value as TaxiCourierDelivery_Approved,

           lc.ReturnDeliveryFactory          * er.Value as ReturnDeliveryFactory,
           lc.ReturnDeliveryFactory_Approved * er.Value as ReturnDeliveryFactory_Approved

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
    select ssc.Country,
           ssc.ClusterRegion,
           ssc.ClusterPla,

           ssc.[1stLevelSupportCostsCountry] * er.Value as '1stLevelSupportCosts',
           ssc.[1stLevelSupportCostsCountry_Approved] * er.Value as '1stLevelSupportCosts_Approved',

           (case 
                when ssc.[2ndLevelSupportCostsLocal] is null then ssc.[2ndLevelSupportCostsClusterRegion]
                else ssc.[2ndLevelSupportCostsLocal] * er.Value
            end) as '2ndLevelSupportCosts', 

           (case 
                when ssc.[2ndLevelSupportCostsLocal_Approved] is null then ssc.[2ndLevelSupportCostsClusterRegion_Approved]
                else ssc.[2ndLevelSupportCostsLocal_Approved] * er.Value
            end) as '2ndLevelSupportCosts_Approved'

    from Hardware.ServiceSupportCost ssc
    join InputAtoms.Country c on c.Id = ssc.Country
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

GO

CREATE VIEW [Hardware].[ReinsuranceView] as
    SELECT r.Wg, 
           r.Year,
           rta.AvailabilityId, 
           rta.ReactionTimeId,

           r.ReinsuranceFlatfee * r.ReinsuranceUpliftFactor / 100 * er.Value as Cost,

           r.ReinsuranceFlatfee_Approved * r.ReinsuranceUpliftFactor_Approved / 100 * er2.Value as Cost_Approved

    FROM Hardware.Reinsurance r
    JOIN Dependencies.ReactionTime_Avalability rta on rta.Id = r.ReactionTimeAvailability
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
    LEFT JOIN [References].ExchangeRate er2 on er2.CurrencyId = r.CurrencyReinsurance_Approved
GO

CREATE VIEW [Hardware].[ProActiveView] AS
with ProActiveCte as 
(
    select pro.Country,
           pro.Wg,

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

    from Hardware.ProActive pro
    left join Fsp.HwFspCodeTranslation fsp on fsp.WgId = pro.Wg
    left join Dependencies.ProActiveSla sla on sla.id = fsp.ProactiveSlaId
)
select pro.Country,
       pro.Wg,

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

from ProActiveCte pro
GO

CREATE FUNCTION Matrix.GetBySla(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    select top(@limit) m.*
        from Matrix.Matrix m
        where m.Id > @lastid
            and m.Denied = 0
            and (@cnt is null or m.CountryId = @cnt)
            and (@wg is null or m.WgId = @wg)
            and (@av is null or m.AvailabilityId = @av)
            and (@dur is null or m.DurationId = @dur)
            and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
            and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
            and (@loc is null or m.ServiceLocationId = @loc)
            order by m.Id
)
GO

CREATE FUNCTION [Hardware].[GetCalcMember] (
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT m.Id

        --SLA

         , c.Name as Country
         , wg.Name as Wg
         , dur.Name as Duration
         , dur.Value as Year
         , dur.IsProlongation
         , av.Name as Availability
         , rtime.Name as ReactionTime
         , rtype.Name as ReactionType
         , loc.Name as ServiceLocation

         , case when @approved = 0 then afr.AFR1 else AFR1_Approved       end as AFR1 
         , case when @approved = 0 then afr.AFR2 else AFR2_Approved       end as AFR2 
         , case when @approved = 0 then afr.AFR3 else afr.AFR3_Approved   end as AFR3 
         , case when @approved = 0 then afr.AFR4 else afr.AFR4_Approved   end as AFR4 
         , case when @approved = 0 then afr.AFR5 else afr.AFR5_Approved   end as AFR5 
         , case when @approved = 0 then afr.AFRP1 else afr.AFRP1_Approved end as AFRP1
       
         , case when @approved = 0 then hdd.HddRet                         else hdd.HddRet_Approved                  end as HddRet              
         
         , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved    end as MaterialCostWarranty
         , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved         end as MaterialCostOow     

         , case when @approved = 0 
                 then mcw.MaterialCostWarranty * tax.TaxAndDuties
                 else mcw.MaterialCostWarranty_Approved * tax.TaxAndDuties_Approved         
            end as TaxAndDutiesW

         , case when @approved = 0 
                 then mco.MaterialCostOow * tax.TaxAndDuties
                 else mco.MaterialCostOow_Approved * tax.TaxAndDuties_Approved
            end as TaxAndDutiesOow

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
         , case when @approved = 0 then ib.InstalledBaseCountry            else ib.InstalledBaseCountry_Approved     end as InstalledBaseCountry    
         , case when @approved = 0 then ib.InstalledBaseCountryPla         else ib.InstalledBaseCountryPla_Approved  end as InstalledBaseCountryPla 

         , case 
                when @approved = 0 then 
                    (case 
                         when ib.InstalledBaseCountry <> 0 and ib.InstalledBaseCountryPla <> 0 
                            then ssc.[1stLevelSupportCosts] / ib.InstalledBaseCountry + ssc.[2ndLevelSupportCosts] / ib.InstalledBaseCountryPla
                     end)
                else 
                    (case 
                         when ib.InstalledBaseCountry_Approved <> 0 and ib.InstalledBaseCountryPla_Approved <> 0 
                            then ssc.[1stLevelSupportCosts_Approved] / ib.InstalledBaseCountry_Approved + ssc.[2ndLevelSupportCosts_Approved] / ib.InstalledBaseCountryPla_Approved
                     end)
            end  as ServiceSupport
         
         , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved          end as ExpressDelivery         
         , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling
         , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved         end as StandardDelivery        
         , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved         end as StandardHandling        
         , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved    end as ReturnDeliveryFactory   
         , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved      end as TaxiCourierDelivery     

         , case 
                 when afEx.id is null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                 else 0
           end as AvailabilityFee

         , case when @approved = 0 then moc.Markup                         else moc.Markup_Approved                       end as Markup                      
         , case when @approved = 0 then moc.MarkupFactor                   else moc.MarkupFactor_Approved                 end as MarkupFactor                
         , case when @approved = 0 then msw.MarkupFactorStandardWarranty   else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty
         , case when @approved = 0 then msw.MarkupStandardWarranty         else msw.MarkupStandardWarranty_Approved       end as MarkupStandardWarranty      
         
         , case when @approved = 0 
                then (pro.Setup + pro.Service * dur.Value)
                else (pro.Setup_Approved + pro.Service_Approved * dur.Value)
            end as ProActive
         
         , case when @approved = 0 then man.ListPrice                      else man.ListPrice_Approved                    end as ListPrice                   
         , case when @approved = 0 then man.DealerDiscount                 else man.DealerDiscount_Approved               end as DealerDiscount              
         , case when @approved = 0 then man.DealerPrice                    else man.DealerPrice_Approved                  end as DealerPrice                 
         , case when @approved = 0 then man.ServiceTC                      else man.ServiceTC_Approved                    end as ServiceTCManual                   
         , case when @approved = 0 then man.ServiceTP                      else man.ServiceTP_Approved                    end as ServiceTPManual                   

    FROM Matrix.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @lastid, @limit) m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.HddRetention hdd on hdd.Wg = m.WgId AND hdd.Year = m.DurationId

    LEFT JOIN Hardware.InstallBase ib on ib.Wg = m.WgId AND ib.Country = m.CountryId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Year = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeId = m.ReactionTimeId AND moc.ReactionTypeId = m.ReactionTypeId AND moc.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.AvailabilityFeeCalcView af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActiveView pro ON pro.Country = m.CountryId AND pro.Wg = m.WgId

    LEFT JOIN Hardware.ManualCost man on man.MatrixId = m.Id
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
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*
                , m.Year * m.ServiceSupport as ServiceSupportCost
                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * (m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate as FieldServicePerYear
                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear
        from Hardware.GetCalcMember(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @lastid, @limit) m
    )
    , CostCte2 as (
        select    m.*

                , m.MaterialCostWarranty * m.AFR1 as mat1
                , m.MaterialCostWarranty * m.AFR2 as mat2
                , m.MaterialCostWarranty * m.AFR3 as mat3
                , m.MaterialCostWarranty * m.AFR4 as mat4
                , m.MaterialCostWarranty * m.AFR5 as mat5
                , m.MaterialCostWarranty * m.AFRP1 as mat1P

                , m.MaterialCostOow * m.AFR1 as matO1
                , m.MaterialCostOow * m.AFR2 as matO2
                , m.MaterialCostOow * m.AFR3 as matO3
                , m.MaterialCostOow * m.AFR4 as matO4
                , m.MaterialCostOow * m.AFR5 as matO5
                , m.MaterialCostOow * m.AFRP1 as matO1P

                , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYear * m.AFR1 as Logistic1
                , m.LogisticPerYear * m.AFR2 as Logistic2
                , m.LogisticPerYear * m.AFR3 as Logistic3
                , m.LogisticPerYear * m.AFR4 as Logistic4
                , m.LogisticPerYear * m.AFR5 as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte3 as (
        select    m.*
                , Hardware.AddMarkup(m.FieldServiceCost1 + m.ServiceSupport + 1 + m.Logistic1 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect1
                , Hardware.AddMarkup(m.FieldServiceCost2 + m.ServiceSupport + 1 + m.Logistic2 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect2
                , Hardware.AddMarkup(m.FieldServiceCost3 + m.ServiceSupport + 1 + m.Logistic3 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect3
                , Hardware.AddMarkup(m.FieldServiceCost4 + m.ServiceSupport + 1 + m.Logistic4 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect4
                , Hardware.AddMarkup(m.FieldServiceCost5 + m.ServiceSupport + 1 + m.Logistic5 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect5
                , Hardware.AddMarkup(m.FieldServiceCost1P + m.ServiceSupport + 1 + m.Logistic1P + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect1P

                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic1, m.TaxAndDutiesW, m.AFR1, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty1
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic2, m.TaxAndDutiesW, m.AFR2, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty2
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic3, m.TaxAndDutiesW, m.AFR3, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty3
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic4, m.TaxAndDutiesW, m.AFR4, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty4
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic5, m.TaxAndDutiesW, m.AFR5, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty5
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic1P, m.TaxAndDutiesW, m.AFRP1, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty1P

        from CostCte2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
             , m.mat1P + m.LocalServiceStandardWarranty1P as Credit1P
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*
             , m.FieldServiceCost1 + m.ServiceSupport + m.mat1 + m.Logistic1 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit1 as ServiceTC1
             , m.FieldServiceCost2 + m.ServiceSupport + m.mat2 + m.Logistic2 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit2 as ServiceTC2
             , m.FieldServiceCost3 + m.ServiceSupport + m.mat3 + m.Logistic3 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit3 as ServiceTC3
             , m.FieldServiceCost4 + m.ServiceSupport + m.mat4 + m.Logistic4 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit4 as ServiceTC4
             , m.FieldServiceCost5 + m.ServiceSupport + m.mat5 + m.Logistic5 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit5 as ServiceTC5
             , m.FieldServiceCost1P + m.ServiceSupport + m.mat1P + m.Logistic1P + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit1P as ServiceTC1P
        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
             , Hardware.AddMarkup(m.ServiceTC1, m.MarkupFactor, m.Markup) as ServiceTP1
             , Hardware.AddMarkup(m.ServiceTC2, m.MarkupFactor, m.Markup) as ServiceTP2
             , Hardware.AddMarkup(m.ServiceTC3, m.MarkupFactor, m.Markup) as ServiceTP3
             , Hardware.AddMarkup(m.ServiceTC4, m.MarkupFactor, m.Markup) as ServiceTP4
             , Hardware.AddMarkup(m.ServiceTC5, m.MarkupFactor, m.Markup) as ServiceTP5
             , Hardware.AddMarkup(m.ServiceTC1P, m.MarkupFactor, m.Markup) as ServiceTP1P
        from CostCte5 m
    )    
    select m.Id

         --SLA
         , m.Country
         , m.Wg
         , m.Availability
         , m.Duration
         , m.ReactionTime
         , m.ReactionType
         , m.ServiceLocation

         --Cost

         , m.AvailabilityFee
         , m.HddRet
         , m.TaxAndDutiesW
         , m.TaxAndDutiesOow
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

    from Hardware.GetCostsFull(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @lastid, @limit)
)