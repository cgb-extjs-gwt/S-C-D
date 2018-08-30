IF OBJECT_ID('Hardware.GetAfr') IS NOT NULL
  DROP FUNCTION Hardware.GetAfr;
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

IF OBJECT_ID('Hardware.UpdateFieldServiceCost') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateFieldServiceCost;
go

IF OBJECT_ID('Hardware.UpdateHddRetention') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateHddRetention;
go

IF OBJECT_ID('Hardware.UpdateMaterialOow') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateMaterialOow;
go

IF OBJECT_ID('Hardware.UpdateMaterialW') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateMaterialW;
go

IF OBJECT_ID('Hardware.UpdateSrvSupportCost') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateSrvSupportCost;
go

IF OBJECT_ID('Hardware.UpdateTaxAndDutiesOow') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateTaxAndDutiesOow;
go

IF OBJECT_ID('Hardware.UpdateTaxAndDutiesW') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateTaxAndDutiesW;
go

IF OBJECT_ID('Hardware.UpdateReinsurance') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateReinsurance;
go

IF OBJECT_ID('Hardware.UpdateLogisticCost') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateLogisticCost;
go

IF OBJECT_ID('Hardware.UpdateOtherDirectCost') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateOtherDirectCost;
go

IF OBJECT_ID('Hardware.UpdateLocalServiceStandardWarranty') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateLocalServiceStandardWarranty;
go

IF OBJECT_ID('Hardware.UpdateCredits') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateCredits;
go

IF OBJECT_ID('Hardware.UpdateAvailabilityFee') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateAvailabilityFee;
go

IF OBJECT_ID('Hardware.UpdateServiceTC') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateServiceTC;
go

IF OBJECT_ID('Hardware.UpdateServiceTP') IS NOT NULL
    DROP PROCEDURE Hardware.UpdateServiceTP;
go

IF OBJECT_ID('Hardware.AvailabilityFeeCalcView', 'V') IS NOT NULL
  DROP VIEW Hardware.AvailabilityFeeCalcView;
go

IF OBJECT_ID('Hardware.AvailabilityFeeView', 'V') IS NOT NULL
  DROP VIEW Hardware.AvailabilityFeeView;
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

IF OBJECT_ID('Atom.MarkupOtherCostsView', 'V') IS NOT NULL
  DROP VIEW Atom.MarkupOtherCostsView;
go

IF OBJECT_ID('Atom.MarkupStandardWarantyView', 'V') IS NOT NULL
  DROP VIEW Atom.MarkupStandardWarantyView;
go

IF OBJECT_ID('Atom.TaxAndDutiesView', 'V') IS NOT NULL
  DROP VIEW Atom.TaxAndDutiesView;
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

CREATE view [Hardware].[AvailabilityFeeView] as 
select fee.Country,
           fee.Wg,
           wg.IsMultiVendor, 
           
           fee.InstalledBaseHighAvailability as IB,
           fee.InstalledBaseHighAvailability_Approved as IB_Approved,
           
           fee.TotalLogisticsInfrastructureCost,
           fee.TotalLogisticsInfrastructureCost_Approved,

           (case wg.IsMultiVendor 
                when 1 then fee.StockValueMv 
                else fee.StockValueFj 
            end) as StockValue,

           (case wg.IsMultiVendor 
                when 1 then fee.StockValueMv_Approved 
                else fee.StockValueFj_Approved 
            end) as StockValue_Approved,
       
           fee.AverageContractDuration,
           fee.AverageContractDuration_Approved,
       
           (case fee.JapanBuy 
                when 1 then fee.CostPerKitJapanBuy
                else fee.CostPerKit
            end) as CostPerKit,
       
           (case fee.JapanBuy 
                when 1 then fee.CostPerKitJapanBuy_Approved
                else fee.CostPerKit_Approved
            end) as CostPerKit_Approved,
       
            fee.MaxQty,
            fee.MaxQty_Approved

    from Hardware.AvailabilityFee fee
    join InputAtoms.Wg wg on wg.Id = fee.Wg
GO

CREATE view [Hardware].[AvailabilityFeeCalcView] as 
    with InstallByCountryCte as (
        SELECT T.Country as CountryID, 
               T.Total_IB, 
               T.Total_IB - coalesce(T.Total_IB_MVS, 0) as Total_IB_FTS, 
               T.Total_IB_MVS,
               T.Total_KC_MQ_IB_FTS,
               T.Total_KC_MQ_IB_MVS
         FROM (
            select fee.Country, 
                   sum(fee.IB) as Total_IB, 
                   sum(fee.IsMultiVendor * fee.IB) as Total_IB_MVS,
                   sum(fee.IsMultiVendor * fee.CostPerKit / fee.MaxQty * fee.IB) as Total_KC_MQ_IB_MVS,
                   sum((1 - fee.IsMultiVendor) * fee.CostPerKit / fee.MaxQty * fee.IB) as Total_KC_MQ_IB_FTS
            from Hardware.AvailabilityFeeView fee
            group by fee.Country 
        ) T
    )
    , AvFeeCte as (
        select fee.*,
               ib.Total_IB,

               iif(fee.IsMultiVendor = 1, ib.Total_IB_MVS, ib.Total_IB_FTS) as Total_IB_VENDOR,               

               iif(fee.IsMultiVendor = 1, ib.Total_KC_MQ_IB_MVS, ib.Total_KC_MQ_IB_FTS) as Total_KC_MQ_IB_VENDOR

        from Hardware.AvailabilityFeeView fee
        join InstallByCountryCte ib on ib.CountryID = fee.Country
    )
    , AvFeeCte2 as (
        select fee.*,

               Hardware.CalcYI(fee.StockValue, fee.AverageContractDuration) as YI,
          
               Hardware.CalcTISC(fee.TotalLogisticsInfrastructureCost, fee.Total_IB, fee.Total_IB_VENDOR) as TISC

        from AvFeeCte fee
    )
    select fee.*, 
           Hardware.CalcAvailabilityFee(fee.CostPerKit, fee.MaxQty, fee.TISC, fee.YI, fee.Total_KC_MQ_IB_VENDOR) as Fee
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
            wg.RoleCodeId,
            fsc.ServiceLocation,
            rt.ReactionTypeId,
            rt.ReactionTimeId,
            fsc.RepairTime,
            fsc.TravelTime,
            fsc.LabourCost,
            fsc.TravelCost,
            fsc.PerformanceRate,
            (fsc.TimeAndMaterialShare / 100) as TimeAndMaterialShare
    FROM Hardware.FieldServiceCost fsc
    JOIN InputAtoms.Wg on wg.Id = fsc.Wg
    JOIN Dependencies.ReactionTime_ReactionType rt on rt.Id = fsc.ReactionTimeType
GO

CREATE VIEW Atom.TaxAndDutiesView as
    select Wg,
           Country,
           (TaxAndDuties / 100) as TaxAndDuties, 
           (TaxAndDuties_Approved / 100) as TaxAndDuties_Approved 
    from Atom.TaxAndDuties
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

CREATE function [Hardware].[CalcFieldServiceCost] (
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

CREATE function [Hardware].[CalcSrvSupportCost] (
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

create view [Atom].[AfrByDurationView] WITH SCHEMABINDING as 
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

CREATE view [Hardware].[HddFrByDurationView] as 
     select wg.Id as WgID,
            d.Id as DurID, 
            (select sum(h.HddFr / 100) 
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as TotalFr
        from Dependencies.Duration d,
             InputAtoms.Wg wg
GO

CREATE view [Hardware].[HddRetByDurationView] as 
     select wg.Id as WgID,
            d.Id as DurID, 
            (select sum(Hardware.CalcHddRetention(h.HddMaterialCost, h.HddFr / 100))
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as HddRet
        from Dependencies.Duration d,
             InputAtoms.Wg wg
go

create  view [Atom].[InstallBaseByCountryView] WITH SCHEMABINDING as

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
           lc.StandardHandling,
           lc.HighAvailabilityHandling,
           lc.StandardDelivery,
           lc.ExpressDelivery,
           lc.TaxiCourierDelivery,
           lc.ReturnDeliveryFactory
    FROM Hardware.LogisticsCosts lc
    JOIN Dependencies.ReactionTime_ReactionType rt on rt.Id = lc.ReactionTimeType
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

CREATE VIEW [InputAtoms].[CountryClusterRegionView] as
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
            cr.IsAsia,
            cr.IsImeia,
            cr.IsJapan,
            cr.IsLatinAmerica,
            cr.IsOceania,
            cr.IsUnitedStates
    from InputAtoms.Country c
    join cte cr on cr.Id = c.ClusterRegionId
GO

CREATE FUNCTION [Hardware].[GetAfr](@wg bigint, @dur bigint)
RETURNS float
AS
BEGIN

    DECLARE @result float;

    SELECT @result = TotalAFR from Atom.AfrByDurationView where WgID = @wg and DurID = @dur

    RETURN @result;

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

CREATE VIEW [Hardware].[ReinsuranceView] as
    SELECT r.Wg, 
           dur.DurID as  Duration,
           rta.AvailabilityId, 
           rta.ReactionTimeId,
           Hardware.CalcReinsuranceCost(r.ReinsuranceFlatfee, r.ReinsuranceUpliftFactor / 100, er.Value) as Cost
    FROM Hardware.Reinsurance r
    JOIN Dependencies.ReactionTime_Avalability rta on rta.Id = r.ReactionTimeAvailability
    JOIN Dependencies.Year y on y.Id = r.Year
    JOIN Dependencies.DurationToYearView dur on dur.YearID = y.Id
    JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
GO

CREATE PROCEDURE [Hardware].[UpdateReinsurance]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET Reinsurance = r.Cost
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId 
              AND r.Duration = m.DurationId 
              AND r.AvailabilityId = m.AvailabilityId 
              AND r.ReactionTimeId = m.ReactionTimeId

END
GO

CREATE PROCEDURE [Hardware].[UpdateFieldServiceCost]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET FieldServiceCost = Hardware.CalcFieldServiceCost(
                                        fsc.TimeAndMaterialShare, 
                                        fsc.TravelCost, 
                                        fsc.LabourCost, 
                                        fsc.PerformanceRate, 
                                        fsc.TravelTime, 
                                        fsc.RepairTime, 
                                        hr.OnsiteHourlyRates, 
                                        afr.TotalAFR
                                    )
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Atom.AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId
    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId 
                                            and fsc.Country = m.CountryId 
                                            and fsc.ServiceLocation = m.ServiceLocationId
                                            and fsc.ReactionTypeId = m.ReactionTypeId
                                            and fsc.ReactionTimeId = m.ReactionTimeId
    LEFT JOIN Atom.RoleCodeHourlyRates hr on hr.RoleCode = fsc.RoleCodeId
END
GO

CREATE PROCEDURE [Hardware].[UpdateHddRetention]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET HddRetention = hr.HddRet
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Hardware.HddRetByDurationView hr on hr.WgID = m.WgId and hr.DurID = m.DurationId

END
GO

CREATE PROCEDURE [Hardware].[UpdateMaterialOow]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET MaterialOow = Hardware.CalcMaterialCostWar(mco.MaterialCostOow, afr.TotalAFR)
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
    LEFT JOIN Atom.MaterialCostOow mco on mco.Wg = m.WgId and mco.ClusterRegion = c.ClusterRegionId
    LEFT JOIN Atom.AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId

END
GO

CREATE PROCEDURE [Hardware].[UpdateMaterialW]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET MaterialW = Hardware.CalcMaterialCostWar(mcw.MaterialCostWarranty, afr.TotalAFR)
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
    LEFT JOIN Atom.MaterialCostWarranty mcw on mcw.Wg = m.WgId and mcw.ClusterRegion = c.ClusterRegionId
    LEFT JOIN Atom.AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId

END
GO

CREATE PROCEDURE [Hardware].[UpdateSrvSupportCost] 
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET ServiceSupport = dur.Value * Hardware.CalcSrvSupportCost(
                                ssc.[1stLevelSupportCostsCountry], 
                                iif(c.IsImeia = 1, ssc.[2ndLevelSupportCostsClusterRegion], ssc.[2ndLevelSupportCostsLocal]), 
                                ib.ibCnt, 
                                ib.ib_Cnt_PLA
                            )
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m on sc.MatrixId = m.Id
    INNER JOIN Dependencies.Duration dur on dur.Id = m.DurationId
    INNER JOIN InputAtoms.CountryClusterRegionView c on c.Id = m.CountryId
    LEFT JOIN Atom.InstallBaseByCountryView ib on ib.Wg = m.WgId and ib.Country = m.CountryId
    LEFT JOIN Hardware.ServiceSupportCost ssc on ssc.Country = m.CountryId
END
GO

CREATE PROCEDURE [Hardware].[UpdateTaxAndDutiesOow]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET TaxAndDutiesOow = Hardware.CalcTaxAndDutiesWar(mco.MaterialCostOow, tax.TaxAndDuties)
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
    LEFT JOIN Atom.TaxAndDutiesView tax on tax.Wg = m.WgId and tax.Country = m.CountryId
    LEFT JOIN Atom.MaterialCostOow mco on mco.Wg = m.WgId and mco.ClusterRegion = c.ClusterRegionId

END
GO

CREATE PROCEDURE [Hardware].[UpdateTaxAndDutiesW]
AS
BEGIN

    SET NOCOUNT ON;

        UPDATE sc SET TaxAndDutiesW = Hardware.CalcTaxAndDutiesWar(mcw.MaterialCostWarranty, tax.TaxAndDuties)
        FROM Hardware.ServiceCostCalculation sc
        INNER JOIN Matrix m ON sc.MatrixId = m.Id
        INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
        LEFT JOIN Atom.TaxAndDutiesView tax on tax.Wg = m.WgId and tax.Country = m.CountryId
        LEFT JOIN Atom.MaterialCostWarranty mcw on mcw.Wg = m.WgId and mcw.ClusterRegion = c.ClusterRegionId

END
GO

CREATE PROCEDURE [Hardware].[UpdateLogisticCost]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET Logistic = Hardware.CalcLogisticCost(
                               lc.StandardHandling,
                               lc.HighAvailabilityHandling,
                               lc.StandardDelivery,
                               lc.ExpressDelivery,
                               lc.TaxiCourierDelivery,
                               lc.ReturnDeliveryFactory,
                               afr.TotalAFR)
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Atom.AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId
    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId 
                                      and lc.Wg = m.WgId
                                      and lc.ReactionTime = m.ReactionTimeId
                                      and lc.ReactionType = m.ReactionTypeId

END
GO

CREATE PROCEDURE [Hardware].[UpdateOtherDirectCost]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET OtherDirect = Hardware.CalcOtherDirectCost(
                                    sc.FieldServiceCost, 
                                    sc.ServiceSupport, 
                                    1, 
                                    sc.Logistic, 
                                    sc.Reinsurance, 
                                    moc.MarkupFactor, 
                                    moc.Markup
                                )
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Atom.MarkupOtherCostsView moc on moc.Wg = m.WgId 
                                               and moc.Country = m.CountryId
                                               and moc.ReactionTimeId = m.ReactionTimeId
                                               and moc.ReactionTypeId = m.ReactionTypeId
                                               and moc.AvailabilityId = m.AvailabilityId
END
GO

CREATE PROCEDURE [Hardware].[UpdateLocalServiceStandardWarranty]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET LocalServiceStandardWarranty = Hardware.CalcLocSrvStandardWarranty(
                                                    fsc.LabourCost,
                                                    fsc.TravelCost,
                                                    sc.ServiceSupport,
                                                    sc.Logistic,
                                                    sc.TaxAndDutiesW,
                                                    afr.TotalAFR,
                                                    sc.AvailabilityFee,
                                                    msw.MarkupFactorStandardWarranty, 
                                                    msw.MarkupStandardWarranty)
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Atom.AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId
    LEFT JOIN Atom.MarkupStandardWarantyView msw on msw.Wg = m.WgId 
                                                    and msw.Country = m.CountryId
                                                    and msw.ReactionTimeId = m.ReactionTimeId
                                                    and msw.ReactionTypeId = m.ReactionTypeId
                                                    and msw.AvailabilityId = m.AvailabilityId
    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId 
                                            and fsc.Country = m.CountryId 
                                            and fsc.ServiceLocation = m.ServiceLocationId
                                            and fsc.ReactionTypeId = m.ReactionTypeId
                                            and fsc.ReactionTimeId = m.ReactionTimeId

END
GO

CREATE PROCEDURE Hardware.UpdateCredits
AS
BEGIN

	SET NOCOUNT ON;

    UPDATE Hardware.ServiceCostCalculation
           SET Credits = MaterialW + LocalServiceStandardWarranty;

END
GO

CREATE PROCEDURE Hardware.UpdateAvailabilityFee
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET AvailabilityFee = iif(afEx.id is null, af.Fee, 0)
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Hardware.AvailabilityFeeCalcView af on af.Country = m.CountryId and af.Wg = m.WgId
    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId
                                            and afEx.ReactionTimeId = m.ReactionTimeId
                                            and afEx.ReactionTypeId = m.ReactionTypeId
                                            and afEx.ServiceLocationId = m.ServiceLocationId

END
GO

CREATE PROCEDURE [Hardware].[UpdateServiceTC]
AS
BEGIN

	SET NOCOUNT ON;

    UPDATE Hardware.ServiceCostCalculation 
            SET ServiceTC = Hardware.CalcServiceTC(
                                FieldServiceCost,
                                ServiceSupport,
                                MaterialW,
                                Logistic,
                                TaxAndDutiesW,
                                Reinsurance,
                                AvailabilityFee,
                                Credits
                           );

END
GO

CREATE PROCEDURE [Hardware].[UpdateServiceTP]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE sc SET ServiceTP = Hardware.CalcServiceTP(sc.ServiceTC, moc.MarkupFactor, moc.Markup)
    FROM Hardware.ServiceCostCalculation sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN Atom.MarkupOtherCosts moc on moc.Wg = m.WgId and moc.Country = m.CountryId
END
GO