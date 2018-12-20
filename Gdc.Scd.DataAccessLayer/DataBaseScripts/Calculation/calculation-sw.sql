IF OBJECT_ID('SoftwareSolution.SwSpMaintenanceCostView', 'U') IS NOT NULL
  DROP TABLE SoftwareSolution.SwSpMaintenanceCostView;
go

IF OBJECT_ID('SoftwareSolution.SwSpMaintenanceCostView', 'V') IS NOT NULL
  DROP VIEW SoftwareSolution.SwSpMaintenanceCostView;
go

IF OBJECT_ID('SoftwareSolution.ProActiveView', 'U') IS NOT NULL
  DROP TABLE SoftwareSolution.ProActiveView;
go

IF OBJECT_ID('SoftwareSolution.ProActiveView', 'V') IS NOT NULL
  DROP VIEW SoftwareSolution.ProActiveView;
go

IF OBJECT_ID('SoftwareSolution.SwSpMaintenanceView', 'V') IS NOT NULL
  DROP VIEW SoftwareSolution.SwSpMaintenanceView;
go

IF OBJECT_ID('InputAtoms.WgSogView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgSogView;
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

CREATE FUNCTION [SoftwareSolution].[CalcDealerPrice] (@maintenance float, @discount float)
returns float
as
BEGIN
    if @discount >= 1
    begin
        return null;
    end
    return @maintenance * (1 - @discount);
END
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

CREATE VIEW InputAtoms.WgSogView as 
    select wg.*
         , sog.Name as Sog
         , sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
GO

CREATE VIEW [SoftwareSolution].[SwSpMaintenanceView] as
    SELECT ssm.Pla,
           ssm.Sog,
           ssm.Sfab,
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

CREATE VIEW SoftwareSolution.SwSpMaintenanceCostView as
    with GermanyServiceCte as (
        select  wg.Id
              , ssc.[1stLevelSupportCosts]
              , ssc.[1stLevelSupportCosts_Approved]
              , ib.InstalledBaseCountry
              , ib.InstalledBaseCountry_Approved

        from Hardware.ServiceSupportCostView ssc
        join InputAtoms.WgView wg on wg.ClusterPla = ssc.ClusterPla
        join Hardware.InstallBase ib on ib.Country = ssc.Country and ib.Wg = wg.Id
        join InputAtoms.Country c on c.id = ssc.Country

        where c.ISO3CountryCode = 'DEU' --install base by Germany!
    )
    , SwSpMaintenanceCte as (
        select m.*
             , cte.*
             , wg.SogId
        from SoftwareSolution.SwSpMaintenanceView m 
        left join InputAtoms.WgSogView wg on wg.SogId = m.Sog
        left join GermanyServiceCte cte on m.Sog = wg.Id
    )
    , SwSpMaintenanceCte2 as (
            select m.*
                 , SoftwareSolution.CalcSrvSupportCost(m.[1stLevelSupportCosts], m.[2ndLevelSupportCosts], m.InstalledBaseCountry, m.InstalledBaseSog) as ServiceSupport
                 , SoftwareSolution.CalcSrvSupportCost(m.[1stLevelSupportCosts_Approved], m.[2ndLevelSupportCosts_Approved], m.InstalledBaseCountry_Approved, m.InstalledBaseSog_Approved) as ServiceSupport_Approved
        from SwSpMaintenanceCte m
    )
    , SwSpMaintenanceCte3 as (
        select m.*
             , SoftwareSolution.CalcTransferPrice(m.Reinsurance, m.ServiceSupport) as TransferPrice
             , SoftwareSolution.CalcTransferPrice(m.Reinsurance_Approved, m.ServiceSupport_Approved) as TransferPrice_Approved
         from SwSpMaintenanceCte2 m
    )
    , SwSpMaintenanceCte4 as (
        select m.Sog
             , m.Pla
             , m.Sfab
             , m.Availability
             , m.Year
             , m.[1stLevelSupportCosts]
             , m.[1stLevelSupportCosts_Approved]
             , m.[2ndLevelSupportCosts]
             , m.[2ndLevelSupportCosts_Approved]
             , m.InstalledBaseCountry
             , m.InstalledBaseCountry_Approved
             , m.InstalledBaseSog
             , m.InstalledBaseSog_Approved
             , m.Reinsurance
             , m.Reinsurance_Approved
             , m.DiscountDealerPrice
             , m.DiscountDealerPrice_Approved
             , m.ServiceSupport
             , m.ServiceSupport_Approved
             , m.TransferPrice
             , m.TransferPrice_Approved
        
            , (case 
                    when m.MaintenanceListPrice is null then SoftwareSolution.CalcMaintenanceListPrice(m.TransferPrice, m.MarkupForProductMargin)
                    else m.MaintenanceListPrice
                end) as MaintenanceListPrice
        
            ,(case 
                    when m.MaintenanceListPrice_Approved is null then SoftwareSolution.CalcMaintenanceListPrice(m.TransferPrice_Approved, m.MarkupForProductMargin_Approved)
                    else m.MaintenanceListPrice_Approved
               end) as MaintenanceListPrice_Approved 

        from SwSpMaintenanceCte3 m
    )
    select m.*
         , DealerPrice = SoftwareSolution.CalcDealerPrice(m.MaintenanceListPrice, m.DiscountDealerPrice)
         , DealerPrice_Approved = SoftwareSolution.CalcDealerPrice(m.MaintenanceListPrice_Approved, m.DiscountDealerPrice_Approved)
    from SwSpMaintenanceCte4 m
GO

CREATE VIEW SoftwareSolution.ProActiveView AS
with ProActiveCte as (
    select pro.Country,
           pro.Sog,
           dur.Value as Year,

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

    from SoftwareSolution.ProActiveSw pro
    left join Fsp.SwFspCodeTranslation fsp on fsp.SogId = pro.Sog
    left join Dependencies.ProActiveSla sla on sla.id = fsp.ProactiveSlaId
    left join Dependencies.Duration dur on dur.Id = fsp.DurationId
)
, ProActiveCte2 as (
     select pro.Country,
            pro.Sog,
            pro.Year,

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
)
select pro.*,
       Hardware.CalcProActive(pro.Setup, pro.Service, pro.Year) as ProActive,
       Hardware.CalcProActive(pro.Setup_Approved, pro.Service_Approved, pro.Year) as ProActive_Approved
from ProActiveCte2 pro
GO


