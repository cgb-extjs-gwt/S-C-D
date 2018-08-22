IF OBJECT_ID('dbo.GetAfr') IS NOT NULL
  DROP FUNCTION dbo.GetAfr;
go 

IF OBJECT_ID('dbo.CalcFieldServiceCost') IS NOT NULL
  DROP FUNCTION dbo.CalcFieldServiceCost;
go 

IF OBJECT_ID('dbo.CalcHddRetention') IS NOT NULL
  DROP FUNCTION dbo.CalcHddRetention;
go 

IF OBJECT_ID('dbo.CalcMaterialCostWar') IS NOT NULL
  DROP FUNCTION dbo.CalcMaterialCostWar;
go 

IF OBJECT_ID('dbo.CalcSrvSupportCost') IS NOT NULL
  DROP FUNCTION dbo.CalcSrvSupportCost;
go 

IF OBJECT_ID('dbo.CalcTaxAndDutiesWar') IS NOT NULL
  DROP FUNCTION dbo.CalcTaxAndDutiesWar;
go 

IF TYPE_ID('dbo.execError') IS NOT NULL
  DROP Type dbo.execError;
go

IF OBJECT_ID('dbo.UpdateFieldServiceCost') IS NOT NULL
    DROP PROCEDURE dbo.UpdateFieldServiceCost;
go

IF OBJECT_ID('dbo.UpdateHddRetention') IS NOT NULL
    DROP PROCEDURE dbo.UpdateHddRetention;
go

IF OBJECT_ID('dbo.UpdateMaterialOow') IS NOT NULL
    DROP PROCEDURE dbo.UpdateMaterialOow;
go

IF OBJECT_ID('dbo.UpdateMaterialW') IS NOT NULL
    DROP PROCEDURE dbo.UpdateMaterialW;
go

IF OBJECT_ID('dbo.UpdateSrvSupportCost') IS NOT NULL
    DROP PROCEDURE dbo.UpdateSrvSupportCost;
go

IF OBJECT_ID('dbo.UpdateTaxAndDutiesOow') IS NOT NULL
    DROP PROCEDURE dbo.UpdateTaxAndDutiesOow;
go

IF OBJECT_ID('dbo.UpdateTaxAndDutiesW') IS NOT NULL
    DROP PROCEDURE dbo.UpdateTaxAndDutiesW;
go

IF OBJECT_ID('dbo.UpdateReinsurance') IS NOT NULL
    DROP PROCEDURE dbo.UpdateReinsurance;
go

IF OBJECT_ID('dbo.UpdateLogisticCost') IS NOT NULL
    DROP PROCEDURE dbo.UpdateLogisticCost;
go

IF OBJECT_ID('dbo.AfrByDurationView', 'V') IS NOT NULL
  DROP VIEW dbo.AfrByDurationView;
go

IF OBJECT_ID('dbo.HddFrByDurationView', 'V') IS NOT NULL
  DROP VIEW dbo.HddFrByDurationView;
go

IF OBJECT_ID('dbo.HddRetByDurationView', 'V') IS NOT NULL
  DROP VIEW dbo.HddRetByDurationView;
go

IF OBJECT_ID('dbo.InstallBaseByCountryView', 'V') IS NOT NULL
  DROP VIEW dbo.InstallBaseByCountryView;
go

IF OBJECT_ID('dbo.DurationToYearView', 'V') IS NOT NULL
  DROP VIEW dbo.DurationToYearView;
go

IF OBJECT_ID('dbo.LogisticsCostView', 'V') IS NOT NULL
  DROP VIEW dbo.LogisticsCostView;
go

IF OBJECT_ID('dbo.CountryClusterRegionView', 'V') IS NOT NULL
  DROP VIEW dbo.CountryClusterRegionView;
go

IF OBJECT_ID('dbo.ReinsuranceByDuration', 'V') IS NOT NULL
  DROP VIEW dbo.ReinsuranceByDuration;
go

IF OBJECT_ID('dbo.CalcReinsuranceCost') IS NOT NULL
  DROP FUNCTION dbo.CalcReinsuranceCost;
go 

IF OBJECT_ID('dbo.CalcLogisticCost') IS NOT NULL
  DROP FUNCTION dbo.CalcLogisticCost;
go 

CREATE VIEW [dbo].[DurationToYearView] as 
    select dur.Id as DurID,
           dur.Name as DurName,
           y.Id as YearID,
           y.Name as YearName,
           dur.Value,
           dur.IsProlongation
    from Dependencies.Duration dur
    join Dependencies.Year y on dur.Value = y.Value and dur.IsProlongation = y.IsProlongation
GO

CREATE FUNCTION [dbo].[CalcLogisticCost](
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

CREATE function [dbo].[CalcFieldServiceCost] (
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
                   @timeAndMaterialShare * ((@travelTime + @repairTime) * @onsiteHourlyRate) + 
                   @performanceRate
                  );
END
GO

CREATE FUNCTION [dbo].[CalcHddRetention](@cost float, @fr float)
RETURNS float
AS
BEGIN
    RETURN @cost * @fr;
END
GO

CREATE FUNCTION [dbo].[CalcMaterialCostWar](@cost float, @afr float)
RETURNS float
AS
BEGIN
    RETURN @cost * @afr;
END
GO

CREATE function [dbo].[CalcSrvSupportCost] (
    @firstLevelSupport float,
    @secondLevelSupport float,
    @ibCountry float,
    @ibPla float
)
returns float
as
BEGIN
    return @firstLevelSupport / @ibCountry + @secondLevelSupport / @ibPla;
END
GO

CREATE FUNCTION [dbo].[CalcTaxAndDutiesWar](@cost float, @tax float)
RETURNS float
AS
BEGIN
    RETURN @cost * @tax;
END
GO

create view [dbo].[AfrByDurationView] as 
    select wg.Id as WgID,
           d.Id as DurID, 
           (select sum(a.AFR) 
            from Atom.AFR a
            JOIN Dependencies.Year y on y.Id = a.Year
            where a.Wg = wg.Id
                  and y.IsProlongation = d.IsProlongation
                  and y.Value <= d.Value) as TotalAFR
    from Dependencies.Duration d,
         InputAtoms.Wg wg
GO

CREATE view [dbo].[HddFrByDurationView] as 
     select wg.Id as WgID,
            d.Id as DurID, 
            (select sum(h.HddFr) 
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as TotalFr
        from Dependencies.Duration d,
             InputAtoms.Wg wg
GO

CREATE view [dbo].[HddRetByDurationView] as 
     select wg.Id as WgID,
            d.Id as DurID, 
            (select sum(dbo.CalcHddRetention(h.HddMaterialCost, h.HddFr))
                from Hardware.HddRetention h
                JOIN Dependencies.Year y on y.Id = h.Year
                where h.Wg = wg.Id
                       and y.IsProlongation = d.IsProlongation
                       and y.Value <= d.Value) as HddRet
        from Dependencies.Duration d,
             InputAtoms.Wg wg
go

create view [dbo].[InstallBaseByCountryView] as

    with InstallBasePlaCte (Country, Pla, totalIB)
    as
    (
        select Country, Pla, sum(InstalledBaseCountry) as totalIB
        from Atom.InstallBase 
        where InstalledBaseCountry is not null
        group by Country, Pla
    )
    select ib.Wg,
            ib.Country,
            ib.InstalledBaseCountry as ibCnt,
            ibp.totalIB as ib_Cnt_PLA
    from Atom.InstallBase ib
    LEFT JOIN InstallBasePlaCte ibp on ibp.Pla = ib.Pla and ibp.Country = ib.Country
GO

CREATE VIEW [dbo].[LogisticsCostView] AS
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

CREATE VIEW [dbo].[CountryClusterRegionView] as
    WITH cte (id, IsImeia, IsJapan, IsApac) as (
      SELECT cr.Id, 
             (case UPPER(cr.Name)
                when 'EMEIA' then 1
                else 0
              end),
         
             (case UPPER(cr.Name)
                when 'JAPAN' then 1
                else 0
              end),
         
             (case UPPER(cr.Name)
                when 'APAC' then 1
                else 0
              end)
        FROM InputAtoms.ClusterRegion cr
    )
    SELECT c.Id, 
           c.Name,
           cr.IsImeia,
           cr.IsJapan,
           cr.IsApac
    FROM InputAtoms.Country c
    JOIN cte cr on cr.Id = c.ClusterRegionId
GO

CREATE FUNCTION [dbo].[GetAfr](@wg bigint, @dur bigint)
RETURNS float
AS
BEGIN

    DECLARE @result float;

    SELECT @result = TotalAFR from AfrByDurationView where WgID = @wg and DurID = @dur

    RETURN @result;

END
GO

CREATE FUNCTION [dbo].[CalcReinsuranceCost](@fee float, @upliftFactor float, @exchangeRate float)
RETURNS float
AS
BEGIN
    RETURN @fee * @upliftFactor * @exchangeRate
END
GO

CREATE VIEW [dbo].[ReinsuranceByDuration] as
    with ExchangeRateCte(Currency, ExchangeRate) as 
    (
       select er.Currency2Id, er.Value
       from [References].ExchangeRate er,
            [References].Currency c
       where er.Currency1Id = c.Id
             AND upper(c.Name) = 'EUR'
    )
    SELECT r.Wg, 
           dur.DurID as  Duration,
           rta.AvailabilityId, 
           rta.ReactionTimeId,
           dbo.CalcReinsuranceCost(r.ReinsuranceFlatfee, r.[ReinsuranceUplift factor], er.ExchangeRate) as Cost
    FROM Hardware.Reinsurance r
    JOIN Dependencies.ReactionTime_Avalability rta on rta.Id = r.ReactionTimeAvailability
    JOIN Dependencies.Year y on y.Id = r.Year
    JOIN DurationToYearView dur on dur.YearID = y.Id
    LEFT JOIN ExchangeRateCte er on er.Currency = r.CurrencyReinsurance
GO

CREATE PROCEDURE [dbo].[UpdateReinsurance]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] 
           SET Reinsurance = rd.Cost
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN ReinsuranceByDuration rd on rd.Wg = m.WgId 
              AND rd.Duration = m.DurationId 
              AND rd.AvailabilityId = m.AvailabilityId 
              AND rd.ReactionTimeId = m.ReactionTimeId

END
GO

CREATE PROCEDURE [dbo].[UpdateFieldServiceCost]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] 
           SET FieldServiceCost = dbo.CalcFieldServiceCost(fsc.TimeAndMaterialShare, fsc.TravelCost, fsc.LabourCost, 1, fsc.TravelTime, fsc.RepairTime, 1, afr.TotalAFR)
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId
    LEFT JOIN Hardware.FieldServiceCost fsc ON fsc.Wg = m.WgId and fsc.Country = m.CountryId and fsc.ServiceLocation = m.ServiceLocationId

END
GO

CREATE PROCEDURE [dbo].[UpdateHddRetention]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] SET HddRetention = hr.HddRet
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN HddRetByDurationView hr on hr.WgID = m.WgId and hr.DurID = m.DurationId

END
GO

CREATE PROCEDURE [dbo].[UpdateMaterialOow]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] 
           SET MaterialOow = dbo.CalcMaterialCostWar(mco.MaterialCostOow, afr.TotalAFR)
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
    LEFT JOIN Atom.MaterialCostOow mco on mco.Wg = m.WgId and mco.ClusterRegion = c.ClusterRegionId
    LEFT JOIN AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId

END
GO

CREATE PROCEDURE [dbo].[UpdateMaterialW]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] 
           SET MaterialW = dbo.CalcMaterialCostWar(mcw.MaterialCostWarranty, afr.TotalAFR)
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
    LEFT JOIN Atom.MaterialCostWarranty mcw on mcw.Wg = m.WgId and mcw.ClusterRegion = c.ClusterRegionId
    LEFT JOIN AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId

END
GO

CREATE PROCEDURE [dbo].[UpdateSrvSupportCost] 
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] 
        SET ServiceSupport = (case c.IsImeia
                                   when 1 then dbo.CalcSrvSupportCost(ssc.[1stLevelSupportCostsCountry], ssc.[2ndLevelSupportCostsClusterRegion], ib.ibCnt, ib.ib_Cnt_PLA) * dur.Value
                                   else dbo.CalcSrvSupportCost(ssc.[1stLevelSupportCostsCountry], ssc.[2ndLevelSupportCostsLocal], ib.ibCnt, ib.ib_Cnt_PLA) * dur.Value
                              end
                             )
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m on sc.MatrixId = m.Id
    INNER JOIN Dependencies.Duration dur on dur.Id = m.DurationId
    INNER JOIN CountryClusterRegionView c on c.Id = m.CountryId
    LEFT JOIN InstallBaseByCountryView ib on ib.Wg = m.WgId and ib.Country = m.CountryId
    LEFT JOIN Hardware.ServiceSupportCost ssc on ib.Country = m.CountryId

END
GO

CREATE PROCEDURE [dbo].[UpdateTaxAndDutiesOow]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] 
           SET TaxAndDutiesOow = dbo.CalcTaxAndDutiesWar(mco.MaterialCostOow, tax.TaxAndDuties)
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
    LEFT JOIN Atom.TaxAndDuties tax on tax.Wg = m.WgId and tax.Country = m.CountryId
    LEFT JOIN Atom.MaterialCostOow mco on mco.Wg = m.WgId and mco.ClusterRegion = c.ClusterRegionId

END
GO

CREATE PROCEDURE [dbo].[UpdateTaxAndDutiesW]
AS
BEGIN

    SET NOCOUNT ON;

        UPDATE [Hardware].[ServiceCostCalculation] 
               SET TaxAndDutiesW = dbo.CalcTaxAndDutiesWar(mcw.MaterialCostWarranty, tax.TaxAndDuties)
        FROM [Hardware].[ServiceCostCalculation] sc
        INNER JOIN Matrix m ON sc.MatrixId = m.Id
        INNER JOIN InputAtoms.Country c on m.CountryId = c.Id
        LEFT JOIN Atom.TaxAndDuties tax on tax.Wg = m.WgId and tax.Country = m.CountryId
        LEFT JOIN Atom.MaterialCostWarranty mcw on mcw.Wg = m.WgId and mcw.ClusterRegion = c.ClusterRegionId

END
GO

CREATE PROCEDURE [dbo].[UpdateLogisticCost]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE [Hardware].[ServiceCostCalculation] 
            SET Logistic = dbo.CalcLogisticCost(
                               lc.StandardHandling,
                               lc.HighAvailabilityHandling,
                               lc.StandardDelivery,
                               lc.ExpressDelivery,
                               lc.TaxiCourierDelivery,
                               lc.ReturnDeliveryFactory,
                               afr.TotalAFR)
    FROM [Hardware].[ServiceCostCalculation] sc
    INNER JOIN Matrix m ON sc.MatrixId = m.Id
    LEFT JOIN AfrByDurationView afr on afr.WgID = m.WgId and afr.DurID = m.DurationId
    LEFT JOIN LogisticsCostView lc on lc.Country = m.CountryId 
                                      and lc.Wg = m.WgId
                                      and lc.ReactionTime = m.ReactionTimeId
                                      and lc.ReactionType = m.ReactionTypeId

END
GO
