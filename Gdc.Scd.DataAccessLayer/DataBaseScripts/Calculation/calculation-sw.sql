IF OBJECT_ID('SoftwareSolution.UpdateMaintenanceListPrice') IS NOT NULL
    DROP PROCEDURE SoftwareSolution.UpdateMaintenanceListPrice;
go

IF OBJECT_ID('SoftwareSolution.UpdateDealerPrice') IS NOT NULL
    DROP PROCEDURE SoftwareSolution.UpdateDealerPrice;
go

IF OBJECT_ID('SoftwareSolution.UpdateReinsurance') IS NOT NULL
    DROP PROCEDURE SoftwareSolution.UpdateReinsurance;
go

IF OBJECT_ID('SoftwareSolution.UpdateSrvSupport') IS NOT NULL
    DROP PROCEDURE SoftwareSolution.UpdateSrvSupport;
go

IF OBJECT_ID('SoftwareSolution.UpdateTransferPrice') IS NOT NULL
    DROP PROCEDURE SoftwareSolution.UpdateTransferPrice;
go

IF OBJECT_ID('SoftwareSolution.CalcDealerPrice') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcDealerPrice;
go 

IF OBJECT_ID('SoftwareSolution.CalcMaintenanceListPrice') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcMaintenanceListPrice;
go 

IF OBJECT_ID('SoftwareSolution.CalcSrvSupportCost') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcSrvSupportCost;
go 

IF OBJECT_ID('SoftwareSolution.CalcTransferPrice') IS NOT NULL
  DROP FUNCTION SoftwareSolution.CalcTransferPrice;
go 

IF OBJECT_ID('SoftwareSolution.SwSpMaintenanceView', 'V') IS NOT NULL
  DROP VIEW SoftwareSolution.SwSpMaintenanceView;
go

CREATE FUNCTION [SoftwareSolution].[CalcDealerPrice] (@maintenance float, @discount float)
returns float
as
BEGIN
    if @discount >= 1
    begin
        return null;
    end
    return @maintenance * (1 - @discount);END
GO

CREATE FUNCTION [SoftwareSolution].[CalcMaintenanceListPrice] (@transfer float, @markup float)
returns float
as
BEGIN
    return @transfer * (1 + @markup);
END
GO

CREATE FUNCTION [SoftwareSolution].[CalcSrvSupportCost] (
    @firstLevelSupport float,
    @secondLevelSupport float,
    @ibCountry float,
    @ibSOG float
)
returns float
as
BEGIN
    if @ibCountry = 0 or @ibSOG = 0
    begin
        return null;
    end
    return @firstLevelSupport / @ibCountry + @secondLevelSupport / @ibSOG;
END
GO

CREATE FUNCTION [SoftwareSolution].[CalcTransferPrice] (@reinsurance float, @srvSupport float)
returns float
as
BEGIN
    return @reinsurance + @srvSupport;
END
GO

CREATE VIEW [SoftwareSolution].[SwSpMaintenanceView] as
    SELECT ssm.Pla,
           ssm.Sog,
           ssm.Sfab,
           ssm.SwDigit,
           ssm.SwLicense,
           ya.YearId as Year,
           ya.AvailabilityId as Availability,
           
           ssm.[2ndLevelSupportCosts],
           ssm.[2ndLevelSupportCosts_Approved],

           ssm.InstalledBaseSog,
           ssm.InstalledBaseSog_Approved,
           
           (case ssm.ReinsuranceFlatfee 
               when null then ssm.ShareSwSpMaintenanceListPrice / 100 * ssm.RecommendedSwSpMaintenanceListPrice 
               else ssm.ReinsuranceFlatfee * er.Value
            end) as Reinsurance,
           
           (case ssm.ReinsuranceFlatfee_Approved 
               when null then ssm.ShareSwSpMaintenanceListPrice_Approved / 100 * ssm.RecommendedSwSpMaintenanceListPrice_Approved 
               else ssm.ReinsuranceFlatfee_Approved * er2.Value
            end) as Reinsurance_Approved,
          
           (ssm.ShareSwSpMaintenanceListPrice / 100) as ShareSwSpMaintenance,
           (ssm.ShareSwSpMaintenanceListPrice_Approved / 100) as ShareSwSpMaintenance_Approved,

           ssm.RecommendedSwSpMaintenanceListPrice as MaintenanceListPrice,
           ssm.RecommendedSwSpMaintenanceListPrice_Approved as MaintenanceListPrice_Approved,

           (ssm.MarkupForProductMarginSwLicenseListPrice / 100)  as MarkupForProductMargin,
           (ssm.MarkupForProductMarginSwLicenseListPrice_Approved / 100)  as MarkupForProductMargin_Approved,

           (ssm.DiscountDealerPrice / 100) as DiscountDealerPrice,
           (ssm.DiscountDealerPrice_Approved / 100) as DiscountDealerPrice_Approved

    FROM SoftwareSolution.SwSpMaintenance ssm
    JOIN Dependencies.Year_Availability ya on ya.Id = ssm.YearAvailability
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = ssm.CurrencyReinsurance
    LEFT JOIN [References].ExchangeRate er2 on er2.CurrencyId = ssm.CurrencyReinsurance_Approved
GO

CREATE PROCEDURE [SoftwareSolution].[UpdateMaintenanceListPrice]
AS
BEGIN

	SET NOCOUNT ON;

    update calc
           set MaintenanceListPrice = iif(m.MaintenanceListPrice is null, 
                                          SoftwareSolution.CalcMaintenanceListPrice(calc.TransferPrice, m.MarkupForProductMargin),
                                          m.MaintenanceListPrice)
    from SoftwareSolution.SwSpCostCalculation calc
    LEFT JOIN SoftwareSolution.SwSpMaintenanceView m on m.Sog = calc.SogId
                                                        and m.Availability = calc.AvailabilityId
                                                        and m.Year = calc.YearId;
END
GO

CREATE PROCEDURE [SoftwareSolution].[UpdateDealerPrice] 
AS
BEGIN

	SET NOCOUNT ON;

    update calc
           set DealerPrice = SoftwareSolution.CalcDealerPrice(calc.MaintenanceListPrice, m.DiscountDealerPrice)
    from SoftwareSolution.SwSpCostCalculation calc
    LEFT JOIN SoftwareSolution.SwSpMaintenanceView m on m.Sog = calc.SogId
                                                        and m.Availability = calc.AvailabilityId
                                                        and m.Year = calc.YearId;
END
GO

CREATE PROCEDURE [SoftwareSolution].[UpdateReinsurance]
AS
BEGIN

	SET NOCOUNT ON;

    update calc set Reinsurance = m.Reinsurance
    from SoftwareSolution.SwSpCostCalculation calc
    LEFT JOIN SoftwareSolution.SwSpMaintenanceView m on m.Sog = calc.SogId
                                                        and m.Availability = calc.AvailabilityId
                                                        and m.Year = calc.YearId;
END
GO

CREATE PROCEDURE [SoftwareSolution].[UpdateSrvSupport]
AS
BEGIN

    SET NOCOUNT ON;

    UPDATE calc 
           set ServiceSupport = SoftwareSolution.CalcSrvSupportCost(
                                   ssc.[1stLevelSupportCostsCountry],
                                   m.[2ndLevelSupportCosts],
                                   ib.ibCnt,
                                   m.InstalledBaseSog
                                )
    FROM SoftwareSolution.SwSpCostCalculation calc
    LEFT JOIN Hardware.ServiceSupportCost ssc on ssc.Country = calc.CountryId
    LEFT JOIN Atom.InstallBaseByCountryView ib on ib.Country = calc.CountryId
    LEFT JOIN SoftwareSolution.SwSpMaintenanceView m on m.Sog = calc.SogId
                                                        and m.Availability = calc.AvailabilityId
                                                        and m.Year = calc.YearId
END
GO

CREATE PROCEDURE [SoftwareSolution].[UpdateTransferPrice]
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE SoftwareSolution.SwSpCostCalculation
           set TransferPrice = SoftwareSolution.CalcTransferPrice(Reinsurance, ServiceSupport)

END
GO