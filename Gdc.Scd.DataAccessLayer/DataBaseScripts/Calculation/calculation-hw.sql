alter table Hardware.ServiceCostCalculation
   drop column DealerPrice, DealerPrice_Approved;

alter table Hardware.ServiceCostCalculation
   add DealerPrice as (ListPrice - (ListPrice * DealerDiscount / 100)),
       DealerPrice_Approved as (ListPrice_Approved - (ListPrice_Approved * DealerDiscount_Approved / 100));
go

IF OBJECT_ID('Hardware.ExecCalculation') IS NOT NULL
  DROP PROCEDURE Hardware.ExecCalculation;
go 

IF OBJECT_ID('Hardware.GetCalcMember') IS NOT NULL
  DROP FUNCTION Hardware.GetCalcMember;
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

IF OBJECT_ID('Atom.AfrYearView', 'V') IS NOT NULL
  DROP VIEW Atom.AfrYearView;
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

IF OBJECT_ID('Atom.AfrByDurationView', 'V') IS NOT NULL
  DROP VIEW Atom.AfrByDurationView;
go

IF OBJECT_ID('Hardware.HddFrByDurationView', 'V') IS NOT NULL
  DROP VIEW Hardware.HddFrByDurationView;
go

IF OBJECT_ID('Hardware.HddRetByDurationView', 'V') IS NOT NULL
  DROP VIEW Hardware.HddRetByDurationView;
go

IF OBJECT_ID('Atom.InstallBaseByCountryView', 'V') IS NOT NULL
  DROP VIEW Atom.InstallBaseByCountryView;
go

IF OBJECT_ID('Dependencies.DurationToYearView', 'V') IS NOT NULL
  DROP VIEW Dependencies.DurationToYearView;
go

IF OBJECT_ID('Hardware.LogisticsCostView', 'V') IS NOT NULL
  DROP VIEW Hardware.LogisticsCostView;
go

IF OBJECT_ID('InputAtoms.CountryClusterRegionView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.CountryClusterRegionView;
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

IF OBJECT_ID('Atom.MarkupOtherCostsView', 'V') IS NOT NULL
  DROP VIEW Atom.MarkupOtherCostsView;
go

IF OBJECT_ID('Atom.MarkupStandardWarantyView', 'V') IS NOT NULL
  DROP VIEW Atom.MarkupStandardWarantyView;
go

IF OBJECT_ID('Atom.TaxAndDutiesView', 'V') IS NOT NULL
  DROP VIEW Atom.TaxAndDutiesView;
go

IF OBJECT_ID('InputAtoms.WgView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgView;
go

IF OBJECT_ID('Hardware.CalcReinsuranceCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcReinsuranceCost;
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

CREATE FUNCTION [Hardware].[CalcReinsuranceCost](@fee float, @upliftFactor float, @exchangeRate float)
RETURNS float
AS
BEGIN
    RETURN @fee * @upliftFactor * @exchangeRate
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

CREATE VIEW [Dependencies].[DurationToYearView] WITH SCHEMABINDING as 
    select dur.Id as DurID,
           dur.Name as DurName,
           y.Id as YearID,
           y.Name as YearName,
           dur.Value,
           dur.IsProlongation
    from Dependencies.Duration dur
    join Dependencies.Year y on dur.Value = y.Value and dur.IsProlongation = y.IsProlongation
GO

CREATE VIEW [Hardware].[FieldServiceCostView] AS
    SELECT  fsc.Country,
            fsc.Wg,
            wg.IsMultiVendor,
            
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
    LEFT JOIN Atom.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
GO

CREATE VIEW Atom.TaxAndDutiesVIEW as
    select Country,
           (TaxAndDuties / 100) as TaxAndDuties, 
           (TaxAndDuties_Approved / 100) as TaxAndDuties_Approved 
    from Atom.TaxAndDuties
    where DeactivatedDateTime is null
GO

CREATE VIEW [InputAtoms].[WgView] WITH SCHEMABINDING as
    SELECT wg.Id, wg.Name, wg.IsMultiVendor, pla.Id as Pla, cpla.Id as ClusterPla
            from InputAtoms.Wg wg,
                 InputAtoms.Pla pla,
                 InputAtoms.ClusterPla cpla
            where wg.PlaId = pla.Id and cpla.id = pla.ClusterPlaId
GO

CREATE VIEW [Atom].[AfrByDurationView] WITH SCHEMABINDING as 
    select wg.Id as WgID,
           d.Id as DurID, 

           (select sum(a.AFR / 100) 
            from Atom.AFR a
            JOIN Dependencies.Year y on y.Id = a.Year
            where a.Wg = wg.Id
                  and y.IsProlongation = d.IsProlongation
                  and y.Value <= d.Value) as TotalAFR,

           (select sum(a.AFR_Approved / 100) 
            from Atom.AFR a
            JOIN Dependencies.Year y on y.Id = a.Year
            where a.Wg = wg.Id
                  and y.IsProlongation = d.IsProlongation
                  and y.Value <= d.Value) as TotalAFR_Approved

    from Dependencies.Duration d,
         InputAtoms.Wg wg
GO

CREATE VIEW [Hardware].[HddFrByDurationView] WITH SCHEMABINDING as 
     select wg.Id as WgID,
            d.Id as DurID, 

            (select sum(h.HddFr / 100) 
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as TotalFr, 

            (select sum(h.HddFr_Approved / 100) 
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as TotalFr_Approved

        from Dependencies.Duration d,
             InputAtoms.Wg wg
GO

CREATE VIEW [Hardware].[HddRetByDurationView] WITH SCHEMABINDING as 
     select wg.Id as WgID,
            d.Id as DurID, 

            (select sum(h.HddMaterialCost * h.HddFr / 100)
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as HddRet,

            (select sum(h.HddMaterialCost_Approved * h.HddFr_Approved / 100)
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as HddRet_Approved

     from Dependencies.Duration d,
          InputAtoms.Wg wg
go

CREATE VIEW [Atom].[InstallBaseByCountryView] WITH SCHEMABINDING as

    with InstallBasePlaCte (Country, Pla, totalIB)
    as
    (
        select Country, Pla, sum(InstalledBaseCountry) as totalIB
        from Atom.InstallBase 
        where InstalledBaseCountry is not null
        group by Country, Pla
    )
    , InstallBasePla_Approved_Cte (Country, Pla, totalIB)
    as
    (
        select Country, Pla, sum(InstalledBaseCountry_Approved) as totalIB
        from Atom.InstallBase 
        where InstalledBaseCountry_Approved is not null
        group by Country, Pla
    )
    select ib.Wg,
            ib.Country,
            ib.InstalledBaseCountry as ibCnt,
            ib.InstalledBaseCountry_Approved as ibCnt_Approved,
            ibp.totalIB as ib_Cnt_PLA,
            ibp2.totalIB as ib_Cnt_PLA_Approved
    from Atom.InstallBase ib
    LEFT JOIN InstallBasePlaCte ibp on ibp.Pla = ib.Pla and ibp.Country = ib.Country
    LEFT JOIN InstallBasePla_Approved_Cte ibp2 on ibp2.Pla = ib.Pla and ibp2.Country = ib.Country
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

CREATE VIEW [Atom].[MarkupOtherCostsView] as 
    select   m.Country
           , m.Wg
           , tta.ReactionTimeId
           , tta.ReactionTypeId
           , tta.AvailabilityId
           , m.Markup
           , m.Markup_Approved
           , (m.MarkupFactor / 100) as MarkupFactor
           , (m.MarkupFactor_Approved / 100) as MarkupFactor_Approved
    from Atom.MarkupOtherCosts m
    join Dependencies.ReactionTime_ReactionType_Avalability tta on tta.id = m.ReactionTimeTypeAvailability
GO

CREATE VIEW [Atom].[MarkupStandardWarantyView] as 
    select m.Country,
           m.Wg,
           tta.ReactionTimeId,
           tta.ReactionTypeId,
           tta.AvailabilityId,
           (m.MarkupFactorStandardWarranty / 100) as MarkupFactorStandardWarranty,
           (m.MarkupFactorStandardWarranty_Approved / 100) as MarkupFactorStandardWarranty_Approved,
           m.MarkupStandardWarranty,
           m.MarkupStandardWarranty_Approved
    from Atom.MarkupStandardWaranty m
    join Dependencies.ReactionTime_ReactionType_Avalability tta on tta.id = m.ReactionTimeTypeAvailability
GO

CREATE VIEW [InputAtoms].[CountryClusterRegionView] WITH SCHEMABINDING as
    with cte (id, IsImeia, IsJapan, IsAsia, IsLatinAmerica, IsOceania, IsUnitedStates) as (
        select cr.Id, 
                (case UPPER(cr.Name)
                    when 'EMEIA' then 1
                    else 0
                end),
         
                (case UPPER(cr.Name)
                    when 'JAPAN' then 1
                    else 0
                end),
         
                (case UPPER(cr.Name)
                    when 'ASIA' then 1
                    else 0
                end),
         
                    (case UPPER(cr.Name)
                    when 'LATIN AMERICA' then 1
                    else 0
                end),
         
                (case UPPER(cr.Name)
                    when 'OCEANIA' then 1
                    else 0
                end),
         
                (case UPPER(cr.Name)
                    when 'UNITED STATES' then 1
                    else 0
                end)
        from InputAtoms.ClusterRegion cr
    )
    select c.Id,
           c.Name,
           c.ClusterRegionId,
           cr.IsAsia,
           cr.IsImeia,
           cr.IsJapan,
           cr.IsLatinAmerica,
           cr.IsOceania,
           cr.IsUnitedStates
    from InputAtoms.Country c
    join cte cr on cr.Id = c.ClusterRegionId
GO

CREATE VIEW [Hardware].[ServiceSupportCostView] as
    select ssc.Country,
           
           wg.Id as Wg,
           wg.IsMultiVendor,
           
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
    join InputAtoms.WgView wg on wg.ClusterPla = ssc.ClusterPla
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
GO

CREATE VIEW [Hardware].[ReinsuranceView] as
    SELECT r.Wg, 
           dur.DurID as Duration,
           rta.AvailabilityId, 
           rta.ReactionTimeId,

           Hardware.CalcReinsuranceCost(r.ReinsuranceFlatfee, r.ReinsuranceUpliftFactor / 100, er.Value) as Cost,
           
           Hardware.CalcReinsuranceCost(r.ReinsuranceFlatfee_Approved, r.ReinsuranceUpliftFactor_Approved / 100, er2.Value) as Cost_Approved

    FROM Hardware.Reinsurance r
    JOIN Dependencies.ReactionTime_Avalability rta on rta.Id = r.ReactionTimeAvailability
    JOIN Dependencies.Year y on y.Id = r.Year
    JOIN Dependencies.DurationToYearVIEW dur on dur.YearID = y.Id
    JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
    JOIN [References].ExchangeRate er2 on er2.CurrencyId = r.CurrencyReinsurance_Approved
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

CREATE VIEW [Atom].[AfrYearView] as
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
        from Atom.AFR afr, Dependencies.Year y 
        where y.Id = afr.Year
        group by afr.Wg
GO

CREATE FUNCTION [Hardware].[GetCalcMember](@country bigint, @wg bigint)
RETURNS TABLE 
AS
RETURN 
(
    SELECT
          m.Id as MatrixId
        , m.WgId
        , dur.Value as Year
        , dur.IsProlongation

        , afr.*

        , hdd.HddRet
        , hdd.HddRet_Approved

        , mcw.MaterialCostWarranty
        , mcw.MaterialCostWarranty_Approved

        , mco.MaterialCostOow
        , mco.MaterialCostOow_Approved

        , Hardware.CalcTaxAndDutiesWar(mcw.MaterialCostWarranty, tax.TaxAndDuties) as TaxAndDutiesW
        , Hardware.CalcTaxAndDutiesWar(mcw.MaterialCostWarranty_Approved, tax.TaxAndDuties_Approved) as TaxAndDutiesW_Approved

        , Hardware.CalcTaxAndDutiesWar(mco.MaterialCostOow, tax.TaxAndDuties) as TaxAndDutiesOow
        , Hardware.CalcTaxAndDutiesWar(mco.MaterialCostOow_Approved, tax.TaxAndDuties_Approved) as TaxAndDutiesOow_Approved
        
        , coalesce(r.Cost, 0) as Reinsurance
        , coalesce(r.Cost_Approved, 0) as Reinsurance_Approved

        , fsc.LabourCost as LabourCost 
        , fsc.LabourCost_Approved as LabourCost_Approved 
        , fsc.TravelCost as TravelCost
        , fsc.TravelCost_Approved as TravelCost_Approved
        , fsc.TimeAndMaterialShare 
        , fsc.TimeAndMaterialShare_Approved 
        , fsc.PerformanceRate 
        , fsc.PerformanceRate_Approved 
        , fsc.TravelTime 
        , fsc.TravelTime_Approved 
        , fsc.RepairTime 
        , fsc.RepairTime_Approved 
        , fsc.OnsiteHourlyRates 
        , fsc.OnsiteHourlyRates_Approved

        , Hardware.CalcSrvSupportCost(
                                    ssc.[1stLevelSupportCosts], 
                                    ssc.[2ndLevelSupportCosts], 
                                    ib.ibCnt, 
                                    ib.ib_Cnt_PLA
                                ) as ServiceSupport
        , Hardware.CalcSrvSupportCost(
                            ssc.[1stLevelSupportCosts_Approved], 
                            ssc.[2ndLevelSupportCosts_Approved], 
                            ib.ibCnt_Approved, 
                            ib.ib_Cnt_PLA_Approved
                        ) as ServiceSupport_Approved

        , lc.ExpressDelivery
        , lc.ExpressDelivery_Approved
        , lc.HighAvailabilityHandling
        , lc.HighAvailabilityHandling_Approved
        , lc.StandardDelivery
        , lc.StandardDelivery_Approved
        , lc.StandardHandling
        , lc.StandardHandling_Approved
        , lc.ReturnDeliveryFactory
        , lc.ReturnDeliveryFactory_Approved
        , lc.TaxiCourierDelivery_Approved
        , lc.TaxiCourierDelivery

        , (case 
                when afEx.id is null then af.Fee
                else 0
            end) as AvailabilityFee
        , (case 
                when afEx.id is null then af.Fee_Approved
                else 0
            end) as AvailabilityFee_Approved

        , moc.Markup
        , moc.Markup_Approved
        , moc.MarkupFactor
        , moc.MarkupFactor_Approved

        , msw.MarkupFactorStandardWarranty 
        , msw.MarkupFactorStandardWarranty_Approved 
        , msw.MarkupStandardWarranty
        , msw.MarkupStandardWarranty_Approved

        , Hardware.CalcProActive(pro.Setup, pro.Service, dur.Value) as ProActive
        , Hardware.CalcProActive(pro.Setup_Approved, pro.Service_Approved, dur.Value) as ProActive_Approved

    FROM Matrix m

    INNER JOIN Dependencies.Duration dur on dur.Id = m.DurationId

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    LEFT JOIN Atom.AfrYearView afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.HddRetByDurationView hdd on hdd.WgID = m.WgId AND hdd.DurID = m.DurationId

    LEFT JOIN Atom.InstallBaseByCountryView ib on ib.Wg = m.WgId AND ib.Country = m.CountryId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.Wg = m.WgId

    LEFT JOIN Atom.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Atom.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Atom.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Atom.MarkupOtherCostsView moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeId = m.ReactionTimeId AND moc.ReactionTypeId = m.ReactionTypeId AND moc.AvailabilityId = m.AvailabilityId

    LEFT JOIN Atom.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.AvailabilityFeeCalcView af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActiveView pro ON pro.Country = m.CountryId AND pro.Wg = m.WgId

    where m.CountryId = @country 
     and (@wg is null or m.WgId = @wg)
)
GO

CREATE PROCEDURE Hardware.ExecCalculation
    @country bigint,
    @wg bigint
AS
BEGIN

    --1 year
    declare @mat1 float;
    declare @matO1 float;
    declare @FieldServiceCost1 float;
    declare @Logistic1 float;
    declare @OtherDirect1 float;
    declare @LocalServiceStandardWarranty1 float;
    declare @Credit1 float;
    declare @ServiceTC1 float;
    declare @ServiceTP1 float;

    declare @mat1_Approved float;
    declare @matO1_Approved float;
    declare @FieldServiceCost1_Approved float;
    declare @Logistic1_Approved float;
    declare @OtherDirect1_Approved float;
    declare @LocalServiceStandardWarranty1_Approved float;
    declare @Credit1_Approved float;
    declare @ServiceTC1_Approved float;
    declare @ServiceTP1_Approved float;

    --2 year
    declare @mat2 float;
    declare @matO2 float;
    declare @FieldServiceCost2 float;
    declare @Logistic2 float;
    declare @OtherDirect2 float;
    declare @LocalServiceStandardWarranty2 float;
    declare @Credit2 float;
    declare @ServiceTC2 float;
    declare @ServiceTP2 float;

    declare @mat2_Approved float;
    declare @matO2_Approved float;
    declare @FieldServiceCost2_Approved float;
    declare @Logistic2_Approved float;
    declare @OtherDirect2_Approved float;
    declare @LocalServiceStandardWarranty2_Approved float;
    declare @Credit2_Approved float;
    declare @ServiceTC2_Approved float;
    declare @ServiceTP2_Approved float;

    --3 year
    declare @mat3 float;
    declare @matO3 float;
    declare @FieldServiceCost3 float;
    declare @Logistic3 float;
    declare @OtherDirect3 float;
    declare @LocalServiceStandardWarranty3 float;
    declare @Credit3 float;
    declare @ServiceTC3 float;
    declare @ServiceTP3 float;

    declare @mat3_Approved float;
    declare @matO3_Approved float;
    declare @FieldServiceCost3_Approved float;
    declare @Logistic3_Approved float;
    declare @OtherDirect3_Approved float;
    declare @LocalServiceStandardWarranty3_Approved float;
    declare @Credit3_Approved float;
    declare @ServiceTC3_Approved float;
    declare @ServiceTP3_Approved float;

    --4 year
    declare @mat4 float;
    declare @matO4 float;
    declare @FieldServiceCost4 float;
    declare @Logistic4 float;
    declare @OtherDirect4 float;
    declare @LocalServiceStandardWarranty4 float;
    declare @Credit4 float;
    declare @ServiceTC4 float;
    declare @ServiceTP4 float;

    declare @mat4_Approved float;
    declare @matO4_Approved float;
    declare @FieldServiceCost4_Approved float;
    declare @Logistic4_Approved float;
    declare @OtherDirect4_Approved float;
    declare @LocalServiceStandardWarranty4_Approved float;
    declare @Credit4_Approved float;
    declare @ServiceTC4_Approved float;
    declare @ServiceTP4_Approved float;

    --5 year
    declare @mat5 float;
    declare @matO5 float;
    declare @FieldServiceCost5 float;
    declare @Logistic5 float;
    declare @OtherDirect5 float;
    declare @LocalServiceStandardWarranty5 float;
    declare @Credit5 float;
    declare @ServiceTC5 float;
    declare @ServiceTP5 float;

    declare @mat5_Approved float;
    declare @matO5_Approved float;
    declare @FieldServiceCost5_Approved float;
    declare @Logistic5_Approved float;
    declare @OtherDirect5_Approved float;
    declare @LocalServiceStandardWarranty5_Approved float;
    declare @Credit5_Approved float;
    declare @ServiceTC5_Approved float;
    declare @ServiceTP5_Approved float;

    --1year prolongation
    declare @mat1P float;
    declare @matO1P float;
    declare @FieldServiceCost1P float;
    declare @Logistic1P float;
    declare @OtherDirect1P float;
    declare @LocalServiceStandardWarranty1P float;
    declare @Credit1P float;
    declare @ServiceTC1P float;
    declare @ServiceTP1P float;

    declare @mat1P_Approved float;
    declare @matO1P_Approved float;
    declare @FieldServiceCost1P_Approved float;
    declare @Logistic1P_Approved float;
    declare @OtherDirect1P_Approved float;
    declare @LocalServiceStandardWarranty1P_Approved float;
    declare @Credit1P_Approved float;
    declare @ServiceTC1P_Approved float;
    declare @ServiceTP1P_Approved float;

    update sc
        SET   sc.AvailabilityFee = m.AvailabilityFee
            , sc.AvailabilityFee_Approved = m.AvailabilityFee_Approved

            , sc.HddRetention = m.HddRet
            , sc.HddRetention_Approved = m.HddRet_Approved

            , sc.ProActive = m.ProActive
            , sc.ProActive_Approved = m.ProActive_Approved

            , sc.Reinsurance = m.Reinsurance
            , sc.Reinsurance_Approved = m.Reinsurance_Approved

            , sc.ServiceSupport = m.Year * m.ServiceSupport
            , sc.ServiceSupport_Approved = m.Year * m.ServiceSupport_Approved

            , sc.TaxAndDutiesW = m.TaxAndDutiesW
            , sc.TaxAndDutiesW_Approved = m.TaxAndDutiesW_Approved

            , sc.TaxAndDutiesOow = m.TaxAndDutiesOow
            , sc.TaxAndDutiesOow_Approved = m.TaxAndDutiesOow_Approved

            --calculated

            --1 year

            , @mat1 = Hardware.CalcMaterialCost(m.MaterialCostWarranty, m.AFR1)
            , @matO1 = Hardware.CalcMaterialCost(m.MaterialCostOow, m.AFR1)
            , @FieldServiceCost1 = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare, m.TravelCost, m.LabourCost, m.PerformanceRate, m.TravelTime, m.RepairTime, m.OnsiteHourlyRates, m.AFR1)
            , @Logistic1 = Hardware.CalcLogisticCost(m.StandardHandling, m.HighAvailabilityHandling, m.StandardDelivery, m.ExpressDelivery, m.TaxiCourierDelivery, m.ReturnDeliveryFactory, m.AFR1)
            , @OtherDirect1 = Hardware.CalcOtherDirectCost(@FieldServiceCost1, m.ServiceSupport, 1, @Logistic1, m.Reinsurance, m.MarkupFactor, m.Markup)
            , @LocalServiceStandardWarranty1 = Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, @Logistic1, m.TaxAndDutiesW, m.AFR1, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
            , @Credit1 = @mat1 + @LocalServiceStandardWarranty1
            , @ServiceTC1 = Hardware.CalcServiceTC(@FieldServiceCost1, m.ServiceSupport, @mat1, @Logistic1, m.TaxAndDutiesW, m.Reinsurance, m.AvailabilityFee, @Credit1)
            , @ServiceTP1 = Hardware.CalcServiceTP(@ServiceTC1, m.MarkupFactor, m.Markup)

            , @mat1_Approved = Hardware.CalcMaterialCost(m.MaterialCostWarranty_Approved, m.AFR1_Approved)
            , @matO1_Approved = Hardware.CalcMaterialCost(m.MaterialCostOow_Approved, m.AFR1_Approved)
            , @FieldServiceCost1_Approved = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare_Approved, m.TravelCost_Approved, m.LabourCost_Approved, m.PerformanceRate_Approved, m.TravelTime_Approved, m.RepairTime_Approved, m.OnsiteHourlyRates_Approved, m.AFR1_Approved)
            , @Logistic1_Approved = Hardware.CalcLogisticCost(m.StandardHandling_Approved, m.HighAvailabilityHandling_Approved, m.StandardDelivery_Approved, m.ExpressDelivery_Approved, m.TaxiCourierDelivery_Approved, m.ReturnDeliveryFactory_Approved, m.AFR1_Approved)
            , @OtherDirect1_Approved = Hardware.CalcOtherDirectCost(@FieldServiceCost1_Approved, m.ServiceSupport_Approved, 1, @Logistic1_Approved, m.Reinsurance_Approved, m.MarkupFactor_Approved, m.Markup_Approved)
            , @LocalServiceStandardWarranty1_Approved = Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, @Logistic1_Approved, m.TaxAndDutiesW_Approved, m.AFR1_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
            , @Credit1_Approved = @mat1_Approved + @LocalServiceStandardWarranty1_Approved
            , @ServiceTC1_Approved = Hardware.CalcServiceTC(@FieldServiceCost1_Approved, m.ServiceSupport_Approved, @mat1_Approved, @Logistic1_Approved, m.TaxAndDutiesW_Approved, m.Reinsurance_Approved, m.AvailabilityFee_Approved, @Credit1_Approved)
            , @ServiceTP1_Approved = Hardware.CalcServiceTP(@ServiceTC1_Approved, m.MarkupFactor_Approved, m.Markup_Approved)

            --2 year

            , @mat2 = Hardware.CalcMaterialCost(m.MaterialCostWarranty, m.AFR2)
            , @matO2 = Hardware.CalcMaterialCost(m.MaterialCostOow, m.AFR2)
            , @FieldServiceCost2 = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare, m.TravelCost, m.LabourCost, m.PerformanceRate, m.TravelTime, m.RepairTime, m.OnsiteHourlyRates, m.AFR2)
            , @Logistic2 = Hardware.CalcLogisticCost(m.StandardHandling, m.HighAvailabilityHandling, m.StandardDelivery, m.ExpressDelivery, m.TaxiCourierDelivery, m.ReturnDeliveryFactory, m.AFR2)
            , @OtherDirect2 = Hardware.CalcOtherDirectCost(@FieldServiceCost2, m.ServiceSupport, 1, @Logistic2, m.Reinsurance, m.MarkupFactor, m.Markup)
            , @LocalServiceStandardWarranty2 = Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, @Logistic2, m.TaxAndDutiesW, m.AFR2, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
            , @Credit2 = @mat2 + @LocalServiceStandardWarranty2
            , @ServiceTC2 = Hardware.CalcServiceTC(@FieldServiceCost2, m.ServiceSupport, @mat2, @Logistic2, m.TaxAndDutiesW, m.Reinsurance, m.AvailabilityFee, @Credit2)
            , @ServiceTP2 = Hardware.CalcServiceTP(@ServiceTC2, m.MarkupFactor, m.Markup)

            , @mat2_Approved = Hardware.CalcMaterialCost(m.MaterialCostWarranty_Approved, m.AFR2_Approved)
            , @matO2_Approved = Hardware.CalcMaterialCost(m.MaterialCostOow_Approved, m.AFR2_Approved)
            , @FieldServiceCost2_Approved = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare_Approved, m.TravelCost_Approved, m.LabourCost_Approved, m.PerformanceRate_Approved, m.TravelTime_Approved, m.RepairTime_Approved, m.OnsiteHourlyRates_Approved, m.AFR2_Approved)
            , @Logistic2_Approved = Hardware.CalcLogisticCost(m.StandardHandling_Approved, m.HighAvailabilityHandling_Approved, m.StandardDelivery_Approved, m.ExpressDelivery_Approved, m.TaxiCourierDelivery_Approved, m.ReturnDeliveryFactory_Approved, m.AFR2_Approved)
            , @OtherDirect2_Approved = Hardware.CalcOtherDirectCost(@FieldServiceCost2_Approved, m.ServiceSupport_Approved, 1, @Logistic2_Approved, m.Reinsurance_Approved, m.MarkupFactor_Approved, m.Markup_Approved)
            , @LocalServiceStandardWarranty2_Approved = Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, @Logistic2_Approved, m.TaxAndDutiesW_Approved, m.AFR2_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
            , @Credit2_Approved = @mat2_Approved + @LocalServiceStandardWarranty2_Approved
            , @ServiceTC2_Approved = Hardware.CalcServiceTC(@FieldServiceCost2_Approved, m.ServiceSupport_Approved, @mat2_Approved, @Logistic2_Approved, m.TaxAndDutiesW_Approved, m.Reinsurance_Approved, m.AvailabilityFee_Approved, @Credit2_Approved)
            , @ServiceTP2_Approved = Hardware.CalcServiceTP(@ServiceTC2_Approved, m.MarkupFactor_Approved, m.Markup_Approved)

            --3 year

            , @mat3 = Hardware.CalcMaterialCost(m.MaterialCostWarranty, m.AFR3)
            , @matO3 = Hardware.CalcMaterialCost(m.MaterialCostOow, m.AFR3)
            , @FieldServiceCost3 = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare, m.TravelCost, m.LabourCost, m.PerformanceRate, m.TravelTime, m.RepairTime, m.OnsiteHourlyRates, m.AFR3)
            , @Logistic3 = Hardware.CalcLogisticCost(m.StandardHandling, m.HighAvailabilityHandling, m.StandardDelivery, m.ExpressDelivery, m.TaxiCourierDelivery, m.ReturnDeliveryFactory, m.AFR3)
            , @OtherDirect3 = Hardware.CalcOtherDirectCost(@FieldServiceCost3, m.ServiceSupport, 1, @Logistic3, m.Reinsurance, m.MarkupFactor, m.Markup)
            , @LocalServiceStandardWarranty3 = Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, @Logistic3, m.TaxAndDutiesW, m.AFR3, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
            , @Credit3 = @mat3 + @LocalServiceStandardWarranty3
            , @ServiceTC3 = Hardware.CalcServiceTC(@FieldServiceCost3, m.ServiceSupport, @mat3, @Logistic3, m.TaxAndDutiesW, m.Reinsurance, m.AvailabilityFee, @Credit3)
            , @ServiceTP3 = Hardware.CalcServiceTP(@ServiceTC3, m.MarkupFactor, m.Markup)

            , @mat3_Approved = Hardware.CalcMaterialCost(m.MaterialCostWarranty_Approved, m.AFR3_Approved)
            , @matO3_Approved = Hardware.CalcMaterialCost(m.MaterialCostOow_Approved, m.AFR3_Approved)
            , @FieldServiceCost3_Approved = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare_Approved, m.TravelCost_Approved, m.LabourCost_Approved, m.PerformanceRate_Approved, m.TravelTime_Approved, m.RepairTime_Approved, m.OnsiteHourlyRates_Approved, m.AFR3_Approved)
            , @Logistic3_Approved = Hardware.CalcLogisticCost(m.StandardHandling_Approved, m.HighAvailabilityHandling_Approved, m.StandardDelivery_Approved, m.ExpressDelivery_Approved, m.TaxiCourierDelivery_Approved, m.ReturnDeliveryFactory_Approved, m.AFR3_Approved)
            , @OtherDirect3_Approved = Hardware.CalcOtherDirectCost(@FieldServiceCost3_Approved, m.ServiceSupport_Approved, 1, @Logistic3_Approved, m.Reinsurance_Approved, m.MarkupFactor_Approved, m.Markup_Approved)
            , @LocalServiceStandardWarranty3_Approved = Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, @Logistic3_Approved, m.TaxAndDutiesW_Approved, m.AFR3_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
            , @Credit3_Approved = @mat3_Approved + @LocalServiceStandardWarranty3_Approved
            , @ServiceTC3_Approved = Hardware.CalcServiceTC(@FieldServiceCost3_Approved, m.ServiceSupport_Approved, @mat3_Approved, @Logistic3_Approved, m.TaxAndDutiesW_Approved, m.Reinsurance_Approved, m.AvailabilityFee_Approved, @Credit3_Approved)
            , @ServiceTP3_Approved = Hardware.CalcServiceTP(@ServiceTC3_Approved, m.MarkupFactor_Approved, m.Markup_Approved)

            --4 year

            , @mat4 = Hardware.CalcMaterialCost(m.MaterialCostWarranty, m.AFR4)
            , @matO4 = Hardware.CalcMaterialCost(m.MaterialCostOow, m.AFR4)
            , @FieldServiceCost4 = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare, m.TravelCost, m.LabourCost, m.PerformanceRate, m.TravelTime, m.RepairTime, m.OnsiteHourlyRates, m.AFR4)
            , @Logistic4 = Hardware.CalcLogisticCost(m.StandardHandling, m.HighAvailabilityHandling, m.StandardDelivery, m.ExpressDelivery, m.TaxiCourierDelivery, m.ReturnDeliveryFactory, m.AFR4)
            , @OtherDirect4 = Hardware.CalcOtherDirectCost(@FieldServiceCost4, m.ServiceSupport, 1, @Logistic4, m.Reinsurance, m.MarkupFactor, m.Markup)
            , @LocalServiceStandardWarranty4 = Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, @Logistic4, m.TaxAndDutiesW, m.AFR4, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
            , @Credit4 = @mat4 + @LocalServiceStandardWarranty4
            , @ServiceTC4 = Hardware.CalcServiceTC(@FieldServiceCost4, m.ServiceSupport, @mat4, @Logistic4, m.TaxAndDutiesW, m.Reinsurance, m.AvailabilityFee, @Credit4)
            , @ServiceTP4 = Hardware.CalcServiceTP(@ServiceTC4, m.MarkupFactor, m.Markup)

            , @mat4_Approved = Hardware.CalcMaterialCost(m.MaterialCostWarranty_Approved, m.AFR4_Approved)
            , @matO4_Approved = Hardware.CalcMaterialCost(m.MaterialCostOow_Approved, m.AFR4_Approved)
            , @FieldServiceCost4_Approved = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare_Approved, m.TravelCost_Approved, m.LabourCost_Approved, m.PerformanceRate_Approved, m.TravelTime_Approved, m.RepairTime_Approved, m.OnsiteHourlyRates_Approved, m.AFR4_Approved)
            , @Logistic4_Approved = Hardware.CalcLogisticCost(m.StandardHandling_Approved, m.HighAvailabilityHandling_Approved, m.StandardDelivery_Approved, m.ExpressDelivery_Approved, m.TaxiCourierDelivery_Approved, m.ReturnDeliveryFactory_Approved, m.AFR4_Approved)
            , @OtherDirect4_Approved = Hardware.CalcOtherDirectCost(@FieldServiceCost4_Approved, m.ServiceSupport_Approved, 1, @Logistic4_Approved, m.Reinsurance_Approved, m.MarkupFactor_Approved, m.Markup_Approved)
            , @LocalServiceStandardWarranty4_Approved = Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, @Logistic4_Approved, m.TaxAndDutiesW_Approved, m.AFR4_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
            , @Credit4_Approved = @mat4_Approved + @LocalServiceStandardWarranty4_Approved
            , @ServiceTC4_Approved = Hardware.CalcServiceTC(@FieldServiceCost4_Approved, m.ServiceSupport_Approved, @mat4_Approved, @Logistic4_Approved, m.TaxAndDutiesW_Approved, m.Reinsurance_Approved, m.AvailabilityFee_Approved, @Credit4_Approved)
            , @ServiceTP4_Approved = Hardware.CalcServiceTP(@ServiceTC4_Approved, m.MarkupFactor_Approved, m.Markup_Approved)

            --5 year

            , @mat5 = Hardware.CalcMaterialCost(m.MaterialCostWarranty, m.AFR5)
            , @matO5 = Hardware.CalcMaterialCost(m.MaterialCostOow, m.AFR5)
            , @FieldServiceCost5 = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare, m.TravelCost, m.LabourCost, m.PerformanceRate, m.TravelTime, m.RepairTime, m.OnsiteHourlyRates, m.AFR5)
            , @Logistic5 = Hardware.CalcLogisticCost(m.StandardHandling, m.HighAvailabilityHandling, m.StandardDelivery, m.ExpressDelivery, m.TaxiCourierDelivery, m.ReturnDeliveryFactory, m.AFR5)
            , @OtherDirect5 = Hardware.CalcOtherDirectCost(@FieldServiceCost5, m.ServiceSupport, 1, @Logistic5, m.Reinsurance, m.MarkupFactor, m.Markup)
            , @LocalServiceStandardWarranty5 = Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, @Logistic5, m.TaxAndDutiesW, m.AFR5, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
            , @Credit5 = @mat5 + @LocalServiceStandardWarranty5
            , @ServiceTC5 = Hardware.CalcServiceTC(@FieldServiceCost5, m.ServiceSupport, @mat5, @Logistic5, m.TaxAndDutiesW, m.Reinsurance, m.AvailabilityFee, @Credit5)
            , @ServiceTP5 = Hardware.CalcServiceTP(@ServiceTC5, m.MarkupFactor, m.Markup)

            , @mat5_Approved = Hardware.CalcMaterialCost(m.MaterialCostWarranty_Approved, m.AFR5_Approved)
            , @matO5_Approved = Hardware.CalcMaterialCost(m.MaterialCostOow_Approved, m.AFR5_Approved)
            , @FieldServiceCost5_Approved = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare_Approved, m.TravelCost_Approved, m.LabourCost_Approved, m.PerformanceRate_Approved, m.TravelTime_Approved, m.RepairTime_Approved, m.OnsiteHourlyRates_Approved, m.AFR5_Approved)
            , @Logistic5_Approved = Hardware.CalcLogisticCost(m.StandardHandling_Approved, m.HighAvailabilityHandling_Approved, m.StandardDelivery_Approved, m.ExpressDelivery_Approved, m.TaxiCourierDelivery_Approved, m.ReturnDeliveryFactory_Approved, m.AFR5_Approved)
            , @OtherDirect5_Approved = Hardware.CalcOtherDirectCost(@FieldServiceCost5_Approved, m.ServiceSupport_Approved, 1, @Logistic5_Approved, m.Reinsurance_Approved, m.MarkupFactor_Approved, m.Markup_Approved)
            , @LocalServiceStandardWarranty5_Approved = Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, @Logistic5_Approved, m.TaxAndDutiesW_Approved, m.AFR5_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
            , @Credit5_Approved = @mat5_Approved + @LocalServiceStandardWarranty5_Approved
            , @ServiceTC5_Approved = Hardware.CalcServiceTC(@FieldServiceCost5_Approved, m.ServiceSupport_Approved, @mat5_Approved, @Logistic5_Approved, m.TaxAndDutiesW_Approved, m.Reinsurance_Approved, m.AvailabilityFee_Approved, @Credit5_Approved)
            , @ServiceTP5_Approved = Hardware.CalcServiceTP(@ServiceTC5_Approved, m.MarkupFactor_Approved, m.Markup_Approved)

            --prolongation for 1year

            , @mat1P = Hardware.CalcMaterialCost(m.MaterialCostWarranty, m.AFRP1)
            , @matO1P = Hardware.CalcMaterialCost(m.MaterialCostOow, m.AFRP1)
            , @FieldServiceCost1P = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare, m.TravelCost, m.LabourCost, m.PerformanceRate, m.TravelTime, m.RepairTime, m.OnsiteHourlyRates, m.AFRP1)
            , @Logistic1P = Hardware.CalcLogisticCost(m.StandardHandling, m.HighAvailabilityHandling, m.StandardDelivery, m.ExpressDelivery, m.TaxiCourierDelivery, m.ReturnDeliveryFactory, m.AFRP1)
            , @OtherDirect1P = Hardware.CalcOtherDirectCost(@FieldServiceCost1P, m.ServiceSupport, 1, @Logistic1P, m.Reinsurance, m.MarkupFactor, m.Markup)
            , @LocalServiceStandardWarranty1P = Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, @Logistic1P, m.TaxAndDutiesW, m.AFRP1, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
            , @Credit1P = @mat1P + @LocalServiceStandardWarranty1P
            , @ServiceTC1P = Hardware.CalcServiceTC(@FieldServiceCost1P, m.ServiceSupport, @mat1P, @Logistic1P, m.TaxAndDutiesW, m.Reinsurance, m.AvailabilityFee, @Credit1P)
            , @ServiceTP1P = Hardware.CalcServiceTP(@ServiceTC1P, m.MarkupFactor, m.Markup)

            , @mat1P_Approved = Hardware.CalcMaterialCost(m.MaterialCostWarranty_Approved, m.AFRP1_Approved)
            , @matO1P_Approved = Hardware.CalcMaterialCost(m.MaterialCostOow_Approved, m.AFRP1_Approved)
            , @FieldServiceCost1P_Approved = Hardware.CalcFieldServiceCost(m.TimeAndMaterialShare_Approved, m.TravelCost_Approved, m.LabourCost_Approved, m.PerformanceRate_Approved, m.TravelTime_Approved, m.RepairTime_Approved, m.OnsiteHourlyRates_Approved, m.AFRP1_Approved)
            , @Logistic1P_Approved = Hardware.CalcLogisticCost(m.StandardHandling_Approved, m.HighAvailabilityHandling_Approved, m.StandardDelivery_Approved, m.ExpressDelivery_Approved, m.TaxiCourierDelivery_Approved, m.ReturnDeliveryFactory_Approved, m.AFRP1_Approved)
            , @OtherDirect1P_Approved = Hardware.CalcOtherDirectCost(@FieldServiceCost1P_Approved, m.ServiceSupport_Approved, 1, @Logistic1P_Approved, m.Reinsurance_Approved, m.MarkupFactor_Approved, m.Markup_Approved)
            , @LocalServiceStandardWarranty1P_Approved = Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, @Logistic1P_Approved, m.TaxAndDutiesW_Approved, m.AFRP1_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
            , @Credit1P_Approved = @mat1P_Approved + @LocalServiceStandardWarranty1P_Approved
            , @ServiceTC1P_Approved = Hardware.CalcServiceTC(@FieldServiceCost1P_Approved, m.ServiceSupport_Approved, @mat1P_Approved, @Logistic1P_Approved, m.TaxAndDutiesW_Approved, m.Reinsurance_Approved, m.AvailabilityFee_Approved, @Credit1P_Approved)
            , @ServiceTP1P_Approved = Hardware.CalcServiceTP(@ServiceTC1P_Approved, m.MarkupFactor_Approved, m.Markup_Approved)

            --sum

            , sc.MaterialW = Hardware.CalcByDur(m.Year, m.IsProlongation, @mat1, @mat2, @mat3, @mat4, @mat5, @mat1P)
            , sc.MaterialW_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @mat1_Approved, @mat2_Approved, @mat3_Approved, @mat4_Approved, @mat5_Approved, @mat1P_Approved)

            , sc.MaterialOow = Hardware.CalcByDur(m.Year, m.IsProlongation, @matO1, @matO2, @matO3, @matO4, @matO5, @matO1P)
            , sc.MaterialOow_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @matO1_Approved, @matO2_Approved, @matO3_Approved, @matO4_Approved, @matO5_Approved, @matO1P_Approved)

            , sc.FieldServiceCost = Hardware.CalcByDur(m.Year, m.IsProlongation, @FieldServiceCost1, @FieldServiceCost2, @FieldServiceCost3, @FieldServiceCost4, @FieldServiceCost5, @FieldServiceCost1P)
            , sc.FieldServiceCost_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @FieldServiceCost1_Approved, @FieldServiceCost2_Approved, @FieldServiceCost3_Approved, @FieldServiceCost4_Approved, @FieldServiceCost5_Approved, @FieldServiceCost1P_Approved)

            , sc.Logistic = Hardware.CalcByDur(m.Year, m.IsProlongation, @Logistic1, @Logistic2, @Logistic3, @Logistic4, @Logistic5, @Logistic1P)
            , sc.Logistic_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @Logistic1_Approved, @Logistic2_Approved, @Logistic3_Approved, @Logistic4_Approved, @Logistic5_Approved, @Logistic1P_Approved)

            , sc.OtherDirect = Hardware.CalcByDur(m.Year, m.IsProlongation, @OtherDirect1, @OtherDirect2, @OtherDirect3, @OtherDirect4, @OtherDirect5, @OtherDirect1P)
            , sc.OtherDirect_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @OtherDirect1_Approved, @OtherDirect2_Approved, @OtherDirect3_Approved, @OtherDirect4_Approved, @OtherDirect5_Approved, @OtherDirect1P_Approved)

            , sc.LocalServiceStandardWarranty = Hardware.CalcByDur(m.Year, m.IsProlongation, @LocalServiceStandardWarranty1, @LocalServiceStandardWarranty2, @LocalServiceStandardWarranty3, @LocalServiceStandardWarranty4, @LocalServiceStandardWarranty5, @LocalServiceStandardWarranty1P)
            , sc.LocalServiceStandardWarranty_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @LocalServiceStandardWarranty1_Approved, @LocalServiceStandardWarranty2_Approved, @LocalServiceStandardWarranty3_Approved, @LocalServiceStandardWarranty4_Approved, @LocalServiceStandardWarranty5_Approved, @LocalServiceStandardWarranty1P_Approved)

            , sc.Credits = Hardware.CalcByDur(m.Year, m.IsProlongation, @Credit1, @Credit2, @Credit3, @Credit4, @Credit5, @Credit1P)
            , sc.Credits_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @Credit1_Approved, @Credit2_Approved, @Credit3_Approved, @Credit4_Approved, @Credit5_Approved, @Credit1P_Approved)

            , sc.ServiceTC = Hardware.CalcByDur(m.Year, m.IsProlongation, @ServiceTC1, @ServiceTC2, @ServiceTC3, @ServiceTC4, @ServiceTC5, @ServiceTC1P)
            , sc.ServiceTC_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @ServiceTC1_Approved, @ServiceTC2_Approved, @ServiceTC3_Approved, @ServiceTC4_Approved, @ServiceTC5_Approved, @ServiceTC1P_Approved)
            , sc.ServiceTC_Str = Hardware.ConcatByDur(m.Year, m.IsProlongation, @ServiceTC1, @ServiceTC2, @ServiceTC3, @ServiceTC4, @ServiceTC5, @ServiceTC1P)
            , sc.ServiceTC_Str_Approved = Hardware.ConcatByDur(m.Year, m.IsProlongation, @ServiceTC1_Approved, @ServiceTC2_Approved, @ServiceTC3_Approved, @ServiceTC4_Approved, @ServiceTC5_Approved, @ServiceTC1P_Approved)
        
            , sc.ServiceTP = Hardware.CalcByDur(m.Year, m.IsProlongation, @ServiceTP1, @ServiceTP2, @ServiceTP3, @ServiceTP4, @ServiceTP5, @ServiceTP1P)
            , sc.ServiceTP_Approved = Hardware.CalcByDur(m.Year, m.IsProlongation, @ServiceTP1_Approved, @ServiceTP2_Approved, @ServiceTP3_Approved, @ServiceTP4_Approved, @ServiceTP5_Approved, @ServiceTP1P_Approved)
            , sc.ServiceTP_Str = Hardware.ConcatByDur(m.Year, m.IsProlongation, @ServiceTP1, @ServiceTP2, @ServiceTP3, @ServiceTP4, @ServiceTP5, @ServiceTP1P)
            , sc.ServiceTP_Str_Approved = Hardware.ConcatByDur(m.Year, m.IsProlongation, @ServiceTP1_Approved, @ServiceTP2_Approved, @ServiceTP3_Approved, @ServiceTP4_Approved, @ServiceTP5_Approved, @ServiceTP1P_Approved)

    from Hardware.ServiceCostCalculation sc
    join Hardware.GetCalcMember(@country, @wg) m on m.MatrixId = sc.MatrixId


END
