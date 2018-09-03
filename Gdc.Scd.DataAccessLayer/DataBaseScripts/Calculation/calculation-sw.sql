IF OBJECT_ID('SoftwareSolution.GetCalcResult') IS NOT NULL
  DROP FUNCTION SoftwareSolution.GetCalcResult;
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

CREATE FUNCTION [SoftwareSolution].[GetCalcResult](
    @cnt bigint,
    @sog bigint,
    @av bigint,
    @year bigint
)
RETURNS TABLE
AS
RETURN 
    with cte as
    (
        select  ib.Country,
                m.Sog,
                m.Year,
                m.Availability,

                m.MaintenanceListPrice,
                m.MaintenanceListPrice_Approved,

                m.MarkupForProductMargin,
                m.MarkupForProductMargin_Approved,

                m.Reinsurance,
                m.Reinsurance_Approved,

                m.DiscountDealerPrice,
                m.DiscountDealerPrice_Approved,

                SoftwareSolution.CalcSrvSupportCost(
                    ssc.[1stLevelSupportCosts],
                    m.[2ndLevelSupportCosts],
                    ib.ibCnt,
                    m.InstalledBaseSog
                ) as ServiceSupport,
                SoftwareSolution.CalcSrvSupportCost(
                    ssc.[1stLevelSupportCosts_Approved],
                    m.[2ndLevelSupportCosts_Approved],
                    ib.ibCnt_Approved,
                    m.InstalledBaseSog_Approved
                ) as ServiceSupport_Approved

        FROM SoftwareSolution.SwSpMaintenanceView m,
             Hardware.ServiceSupportCostView ssc,
             Atom.InstallBaseByCountryView ib

        WHERE       (@sog is null or m.Sog = @sog)
                AND (@cnt is null or ib.Country = @cnt)
                AND (@av is null or m.Availability = @av)
                AND (@year is null or m.Year = @year)
    )
    , cte2 as
    (
        select calc.*,

               SoftwareSolution.CalcTransferPrice(Reinsurance, ServiceSupport) as TransferPrice,
               SoftwareSolution.CalcTransferPrice(Reinsurance_Approved, ServiceSupport_Approved) as TransferPrice_Approved
        from cte as calc
    )
    , cte3 as 
    (
        select calc.Country,
               calc.Sog,
               calc.Year,
               calc.Availability,

               calc.DiscountDealerPrice,
               calc.DiscountDealerPrice_Approved,

               calc.Reinsurance,
               calc.Reinsurance_Approved,

               calc.ServiceSupport,
               calc.ServiceSupport_Approved,

               calc.TransferPrice,
               calc.TransferPrice_Approved,

               (case 
                    when calc.MaintenanceListPrice is null then SoftwareSolution.CalcMaintenanceListPrice(calc.TransferPrice, calc.MarkupForProductMargin)
                    else calc.MaintenanceListPrice
                end) as MaintenanceListPrice,
               (case 
                    when calc.MaintenanceListPrice_Approved is null then SoftwareSolution.CalcMaintenanceListPrice(calc.TransferPrice_Approved, calc.MarkupForProductMargin_Approved)
                    else calc.MaintenanceListPrice_Approved
                end) as MaintenanceListPrice_Approved

        from cte2 as calc
    )
    select calc.Country,
           calc.Sog,
           calc.Year,
           calc.Availability,

           calc.Reinsurance,
           calc.Reinsurance_Approved,

           calc.ServiceSupport,
           calc.ServiceSupport_Approved,

           calc.TransferPrice,
           calc.TransferPrice_Approved,

           calc.MaintenanceListPrice,
           calc.MaintenanceListPrice_Approved,

           SoftwareSolution.CalcDealerPrice(calc.MaintenanceListPrice, calc.DiscountDealerPrice) as DealerPrice,
           SoftwareSolution.CalcDealerPrice(calc.MaintenanceListPrice_Approved, calc.DiscountDealerPrice_Approved) as DealerPrice_Approved

    from cte3 as calc
GO


