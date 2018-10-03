IF OBJECT_ID('Hardware.GetCalcResult') IS NOT NULL
  DROP FUNCTION Hardware.GetCalcResult;
go 

IF OBJECT_ID('Hardware.CalcFieldServiceCost') IS NOT NULL
  DROP FUNCTION Hardware.CalcFieldServiceCost;
go 

IF OBJECT_ID('Hardware.CalcHddRetention') IS NOT NULL
  DROP FUNCTION Hardware.CalcHddRetention;
go 

IF OBJECT_ID('Hardware.CalcMaterialCostWar') IS NOT NULL
  DROP FUNCTION Hardware.CalcMaterialCostWar;
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

CREATE FUNCTION [Hardware].[CalcMaterialCostWar](@cost float, @afr float)
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
    select Wg,
           Country,
           (TaxAndDuties / 100) as TaxAndDuties, 
           (TaxAndDuties_Approved / 100) as TaxAndDuties_Approved 
    from Atom.TaxAndDuties
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
    join InputAtoms.WgVIEW wg on wg.ClusterPla = ssc.ClusterPla
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

CREATE FUNCTION [Hardware].[GetCalcResult](
	@cnt bigint,
	@wg bigint,
	@av bigint,
	@dur bigint,
	@rtype bigint,
	@rtime bigint,
	@loc bigint
)
RETURNS TABLE
AS
RETURN 
    with cte as
    (
        select m.Id as MatrixId,
               m.CountryId,
               m.WgId,
               m.AvailabilityId,
               m.DurationId,
               m.ReactionTimeId,
               m.ReactionTypeId,
               m.ServiceLocationId,

               afr.TotalAFR,
               afr.TotalAFR_Approved,
               
               hdd.HddRet,
               hdd.HddRet_Approved,

               Hardware.CalcMaterialCostWar(mcw.MaterialCostWarranty, afr.TotalAFR) as MaterialW,
               Hardware.CalcMaterialCostWar(mcw.MaterialCostWarranty_Approved, afr.TotalAFR_Approved) as MaterialW_Approved,

               Hardware.CalcMaterialCostWar(mco.MaterialCostOow, afr.TotalAFR) as MaterialOow,
               Hardware.CalcMaterialCostWar(mco.MaterialCostOow_Approved, afr.TotalAFR_Approved) as MaterialOow_Approved,

               Hardware.CalcTaxAndDutiesWar(mcw.MaterialCostWarranty, tax.TaxAndDuties) as TaxAndDutiesW,
               Hardware.CalcTaxAndDutiesWar(mcw.MaterialCostWarranty_Approved, tax.TaxAndDuties_Approved) as TaxAndDutiesW_Approved,

               Hardware.CalcTaxAndDutiesWar(mco.MaterialCostOow, tax.TaxAndDuties) as TaxAndDutiesOow,
               Hardware.CalcTaxAndDutiesWar(mco.MaterialCostOow_Approved, tax.TaxAndDuties_Approved) as TaxAndDutiesOow_Approved,

               r.Cost as Reinsurance,
               r.Cost_Approved as Reinsurance_Approved,

               fsc.LabourCost as LabourCost, 
               fsc.LabourCost_Approved as LabourCost_Approved, 

               fsc.TravelCost as TravelCost,
               fsc.TravelCost_Approved as TravelCost_Approved,

               Hardware.CalcFieldServiceCost(
                    fsc.TimeAndMaterialShare, 
                    fsc.TravelCost, 
                    fsc.LabourCost, 
                    fsc.PerformanceRate, 
                    fsc.TravelTime, 
                    fsc.RepairTime, 
                    fsc.OnsiteHourlyRates, 
                    afr.TotalAFR
                ) as FieldServiceCost,
               Hardware.CalcFieldServiceCost(
                    fsc.TimeAndMaterialShare_Approved, 
                    fsc.TravelCost_Approved, 
                    fsc.LabourCost_Approved, 
                    fsc.PerformanceRate_Approved, 
                    fsc.TravelTime_Approved, 
                    fsc.RepairTime_Approved, 
                    fsc.OnsiteHourlyRates_Approved, 
                    afr.TotalAFR_Approved
                ) as FieldServiceCost_Approved,

                (dur.Value * Hardware.CalcSrvSupportCost(
                                    ssc.[1stLevelSupportCosts], 
                                    ssc.[2ndLevelSupportCosts], 
                                    ib.ibCnt, 
                                    ib.ib_Cnt_PLA
                                )) as ServiceSupport,
                (dur.Value * Hardware.CalcSrvSupportCost(
                                    ssc.[1stLevelSupportCosts_Approved], 
                                    ssc.[2ndLevelSupportCosts_Approved], 
                                    ib.ibCnt_Approved, 
                                    ib.ib_Cnt_PLA_Approved
                                )) as ServiceSupport_Approved,

                Hardware.CalcLogisticCost(
                    lc.StandardHandling,
                    lc.HighAvailabilityHandling,
                    lc.StandardDelivery,
                    lc.ExpressDelivery,
                    lc.TaxiCourierDelivery,
                    lc.ReturnDeliveryFactory,
                    afr.TotalAFR
                ) as Logistic,     
                Hardware.CalcLogisticCost(
                    lc.StandardHandling_Approved,
                    lc.HighAvailabilityHandling_Approved,
                    lc.StandardDelivery_Approved,
                    lc.ExpressDelivery_Approved,
                    lc.TaxiCourierDelivery_Approved,
                    lc.ReturnDeliveryFactory_Approved,
                    afr.TotalAFR_Approved
                ) as Logistic_Approved,

                (case 
                      when afEx.id is null then af.Fee
                      else 0
                 end) as AvailabilityFee,
                (case 
                      when afEx.id is null then af.Fee_Approved
                      else 0
                 end) as AvailabilityFee_Approved,

                moc.Markup,
                moc.Markup_Approved,

                moc.MarkupFactor,
                moc.MarkupFactor_Approved,

                msw.MarkupFactorStandardWarranty, 
                msw.MarkupFactorStandardWarranty_Approved, 

                msw.MarkupStandardWarranty,
                msw.MarkupStandardWarranty_Approved,

                Hardware.CalcProActive(pro.Setup, pro.Service, dur.Value) as ProActive,
                Hardware.CalcProActive(pro.Setup_Approved, pro.Service_Approved, dur.Value) as ProActive_Approved

        FROM Matrix m

        INNER JOIN Dependencies.Duration dur on dur.Id = m.DurationId

        INNER JOIN InputAtoms.Country c on c.id = m.CountryId

        LEFT JOIN Atom.AfrByDurationView afr on afr.WgID = m.WgId AND afr.DurID = m.DurationId

        LEFT JOIN Hardware.HddRetByDurationView hdd on hdd.WgID = m.WgId AND hdd.DurID = m.DurationId

        LEFT JOIN Atom.InstallBaseByCountryView ib on ib.Wg = m.WgId AND ib.Country = m.CountryId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.Wg = m.WgId

        LEFT JOIN Atom.TaxAndDutiesView tax on tax.Wg = m.WgId AND tax.Country = m.CountryId

        LEFT JOIN Atom.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Atom.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId 
                                                AND r.Duration = m.DurationId 
                                                AND r.AvailabilityId = m.AvailabilityId 
                                                AND r.ReactionTimeId = m.ReactionTimeId

        LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId 
                                                AND fsc.Country = m.CountryId 
                                                AND fsc.ServiceLocation = m.ServiceLocationId
                                                AND fsc.ReactionTypeId = m.ReactionTypeId
                                                AND fsc.ReactionTimeId = m.ReactionTimeId

        LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTime = m.ReactionTimeId
                                            AND lc.ReactionType = m.ReactionTypeId

        LEFT JOIN Atom.MarkupOtherCostsView moc on moc.Wg = m.WgId 
                                               AND moc.Country = m.CountryId
                                               AND moc.ReactionTimeId = m.ReactionTimeId
                                               AND moc.ReactionTypeId = m.ReactionTypeId
                                               AND moc.AvailabilityId = m.AvailabilityId
                                           
        LEFT JOIN Atom.MarkupStandardWarantyView msw on msw.Wg = m.WgId 
                                                    AND msw.Country = m.CountryId
                                                    AND msw.ReactionTimeId = m.ReactionTimeId
                                                    AND msw.ReactionTypeId = m.ReactionTypeId
                                                    AND msw.AvailabilityId = m.AvailabilityId

        LEFT JOIN Hardware.AvailabilityFeeCalcView af on af.Country = m.CountryId AND af.Wg = m.WgId

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId
                                                AND afEx.ReactionTimeId = m.ReactionTimeId
                                                AND afEx.ReactionTypeId = m.ReactionTypeId
                                                AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Hardware.ProActiveView pro ON pro.Country = m.CountryId AND pro.Wg = m.WgId

        where     (@wg is null or m.WgId = @wg)
              AND (@cnt is null or m.CountryId = @cnt)
              AND (@dur is null or m.DurationId = @dur)
              AND (@av is null or m.AvailabilityId = @av)
              AND (@rtime is null or m.ReactionTimeId = @rtime)
              AND (@rtype is null or m.ReactionTypeId = @rtype)
              AND (@loc is null or m.ServiceLocationId = @loc)
    )
    , cte2 as 
    (
        select sc.*,

               Hardware.CalcOtherDirectCost(
                    sc.FieldServiceCost, 
                    sc.ServiceSupport, 
                    1, 
                    sc.Logistic, 
                    sc.Reinsurance, 
                    sc.MarkupFactor, 
                    sc.Markup
                ) as OtherDirect,
                Hardware.CalcOtherDirectCost(
                    sc.FieldServiceCost_Approved, 
                    sc.ServiceSupport_Approved, 
                    1, 
                    sc.Logistic_Approved, 
                    sc.Reinsurance_Approved, 
                    sc.MarkupFactor_Approved, 
                    sc.Markup_Approved
                ) as OtherDirect_Approved,

               Hardware.CalcLocSrvStandardWarranty(
                    sc.LabourCost,
                    sc.TravelCost,
                    sc.ServiceSupport,
                    sc.Logistic,
                    sc.TaxAndDutiesW,
                    sc.TotalAFR,
                    sc.AvailabilityFee,
                    sc.MarkupFactorStandardWarranty, 
                    sc.MarkupStandardWarranty
                ) as LocalServiceStandardWarranty,
               Hardware.CalcLocSrvStandardWarranty(
                    sc.LabourCost_Approved,
                    sc.TravelCost_Approved,
                    sc.ServiceSupport_Approved,
                    sc.Logistic_Approved,
                    sc.TaxAndDutiesW_Approved,
                    sc.TotalAFR_Approved,
                    sc.AvailabilityFee_Approved,
                    sc.MarkupFactorStandardWarranty_Approved, 
                    sc.MarkupStandardWarranty_Approved
                ) as LocalServiceStandardWarranty_Approved
        from cte as sc
    )
    , cte3 as
    (
        select sc.*,

               (sc.MaterialW + sc.LocalServiceStandardWarranty) as Credits,
               (sc.MaterialW_Approved + sc.LocalServiceStandardWarranty_Approved) as Credits_Approved,

               Hardware.CalcServiceTC(
                    sc.FieldServiceCost,
                    sc.ServiceSupport,
                    sc.MaterialW,
                    sc.Logistic,
                    sc.TaxAndDutiesW,
                    sc.Reinsurance,
                    sc.AvailabilityFee,
                    sc.MaterialW + sc.LocalServiceStandardWarranty --Credits
                ) as ServiceTC,    
               Hardware.CalcServiceTC(
                    sc.FieldServiceCost_Approved,
                    sc.ServiceSupport_Approved,
                    sc.MaterialW_Approved,
                    sc.Logistic_Approved,
                    sc.TaxAndDutiesW_Approved,
                    sc.Reinsurance_Approved,
                    sc.AvailabilityFee_Approved,
                    sc.MaterialW_Approved + sc.LocalServiceStandardWarranty_Approved --Credits_Approved
                ) as ServiceTC_Approved

        from cte2 sc
    )
    select 
           sc.MatrixId,

           --dependencies
           sc.CountryId,
           sc.WgId,
           sc.AvailabilityId,
           sc.DurationId,
           sc.ReactionTimeId,
           sc.ReactionTypeId,
           sc.ServiceLocationId,

           --cost block results
           sc.FieldServiceCost, sc.FieldServiceCost_Approved,

           sc.ServiceSupport, sc.ServiceSupport_Approved,

           sc.Logistic, sc.Logistic_Approved,

           sc.AvailabilityFee, sc.AvailabilityFee_Approved,

           sc.HddRet, sc.HddRet_Approved,

           sc.Reinsurance, sc.Reinsurance_Approved,

           sc.TaxAndDutiesW, sc.TaxAndDutiesW_Approved,

           sc.TaxAndDutiesOow, sc.TaxAndDutiesOow_Approved,

           sc.MaterialW, sc.MaterialW_Approved,

           sc.MaterialOow, sc.MaterialOow_Approved,

           sc.ProActive, sc.ProActive_Approved,

           --resulting costs
           sc.ServiceTC, sc.ServiceTC_Approved,

           Hardware.CalcServiceTP(sc.ServiceTC, sc.MarkupFactor, sc.Markup) as ServiceTP,
           Hardware.CalcServiceTP(sc.ServiceTC_Approved, sc.MarkupFactor_Approved, sc.Markup_Approved) as ServiceTP_Approved,

           sc.OtherDirect, sc.OtherDirect_Approved,
           
           sc.LocalServiceStandardWarranty, sc.LocalServiceStandardWarranty_Approved,
           
           sc.Credits, sc.Credits_Approved

    from cte3 sc
GO
