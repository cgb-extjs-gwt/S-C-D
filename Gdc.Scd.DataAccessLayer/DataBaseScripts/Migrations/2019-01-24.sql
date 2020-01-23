ALTER FUNCTION [SoftwareSolution].[GetCosts] (
    @approved bit,
    @digit bigint,
    @av bigint,
    @year bigint,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with GermanyServiceCte as (
        SELECT   ssc.ClusterPla
               , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry] else ssc.[1stLevelSupportCostsCountry_Approved] end / er.Value as [1stLevelSupportCosts]
               , case when @approved = 0 then ssc.TotalIb else TotalIb_Approved end as TotalIb

        FROM Hardware.ServiceSupportCost ssc
        JOIN InputAtoms.Country c on c.Id = ssc.Country and c.ISO3CountryCode = 'DEU' --install base by Germany!
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
    )
    , SwSpMaintenanceCte0 as (
            SELECT  ssm.rownum
                  , ssm.Id
                  , ssm.SwDigit
                  , ssm.Sog
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Availability
                  , ssm.Year

                  , ssm.[2ndLevelSupportCosts]
                  , ssm.InstalledBaseSog
           
                  , case when ssm.ReinsuranceFlatfee is null 
                            then ssm.ShareSwSpMaintenanceListPrice / 100 * ssm.RecommendedSwSpMaintenanceListPrice 
                            else ssm.ReinsuranceFlatfee / er.Value
                    end as Reinsurance

                  , ssm.ShareSwSpMaintenanceListPrice / 100                      as ShareSwSpMaintenance

                  , ssm.RecommendedSwSpMaintenanceListPrice                      as MaintenanceListPrice

                  , ssm.MarkupForProductMarginSwLicenseListPrice / 100           as MarkupForProductMargin

                  , ssm.DiscountDealerPrice / 100                                as DiscountDealerPrice

            FROM SoftwareSolution.GetSwSpMaintenancePaging(@approved, @digit, @av, @year, @lastid, @limit) ssm
            LEFT JOIN [References].ExchangeRate er on er.CurrencyId = ssm.CurrencyReinsurance    
    )
    , SwSpMaintenanceCte as (
        select m.*
             , ssc.[1stLevelSupportCosts]
             , ssc.TotalIb

             , SoftwareSolution.CalcSrvSupportCost(ssc.[1stLevelSupportCosts], m.[2ndLevelSupportCosts], ssc.TotalIb, m.InstalledBaseSog) as ServiceSupport

        from SwSpMaintenanceCte0 m 
        join InputAtoms.Pla pla on pla.Id = m.Pla
        left join GermanyServiceCte ssc on ssc.ClusterPla = pla.ClusterPlaId
    )
    , SwSpMaintenanceCte2 as (
        select m.*

             , SoftwareSolution.CalcTransferPrice(m.Reinsurance, m.ServiceSupport) as TransferPrice

         from SwSpMaintenanceCte m
    )
    , SwSpMaintenanceCte3 as (
        select m.rownum
             , m.Id
             , m.SwDigit
             , m.Sog
             , m.Pla
             , m.Sfab
             , m.Availability
             , m.Year
             , m.[1stLevelSupportCosts]
             , m.[2ndLevelSupportCosts]
             , m.TotalIb as InstalledBaseCountry
             , m.InstalledBaseSog
             , m.Reinsurance
             , m.ShareSwSpMaintenance
             , m.DiscountDealerPrice
             , m.ServiceSupport
             , m.TransferPrice

            , case when m.MaintenanceListPrice is null 
                     then SoftwareSolution.CalcMaintenanceListPrice(m.TransferPrice, m.MarkupForProductMargin)
                     else m.MaintenanceListPrice
               end as MaintenanceListPrice

        from SwSpMaintenanceCte2 m
    )
    select m.*
         , SoftwareSolution.CalcDealerPrice(m.MaintenanceListPrice, m.DiscountDealerPrice) as DealerPrice 
    from SwSpMaintenanceCte3 m
)
GO

ALTER VIEW [Hardware].[AvailabilityFeeView] as 
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

ALTER VIEW [Hardware].[FieldServiceCostView] AS
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

ALTER VIEW [Hardware].[LogisticsCostView] AS
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

ALTER VIEW [Hardware].[ServiceSupportCostView] as
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

ALTER VIEW [Hardware].[ReinsuranceView] as
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

ALTER PROCEDURE [Hardware].[SpGetCosts]
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

             , ServiceTC_Released            * @exchange as ServiceTC_Released
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

ALTER VIEW [Hardware].[ManualCostView] as
    select    man.PortfolioId

            , man.ChangeUserId
            , u.Name  as ChangeUserName
            , u.Email as ChangeUserEmail

            , man.ServiceTC   / er.Value as ServiceTC   
            , man.ServiceTP   / er.Value as ServiceTP   

            , man.ServiceTC_Released / er.Value as ServiceTC_Released
            , man.ServiceTP_Released / er.Value as ServiceTP_Released

            , man.ListPrice   / er.Value as ListPrice   
            , man.DealerDiscount
            , man.DealerPrice / er.Value as DealerPrice 

    from Hardware.ManualCost man
    join Portfolio.LocalPortfolio p on p.Id = man.PortfolioId
    join InputAtoms.Country c on c.Id = p.CountryId
    join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
    left join dbo.[User] u on u.Id = man.ChangeUserId
GO

