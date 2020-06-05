IF OBJECT_ID('Hardware.GetCostsSlaSog') IS NOT NULL
  DROP FUNCTION Hardware.GetCostsSlaSog;
go

CREATE FUNCTION [Hardware].[GetCostsSlaSog](
    @approved bit,
    @cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN 
(
    with cte as (
        select    
               m.Id

             --SLA

             , m.CountryId
             , m.Country
             , m.CurrencyId
             , m.Currency
             , m.ExchangeRate

             , m.WgId
             , m.Wg
             , wg.Description as WgDescription
             , m.SogId
             , m.Sog

             , m.AvailabilityId
             , m.Availability
             , m.DurationId
             , m.Duration
             , m.Year
             , m.IsProlongation
             , m.ReactionTimeId
             , m.ReactionTime
             , m.ReactionTypeId
             , m.ReactionType
             , m.ServiceLocationId
             , m.ServiceLocation
             , m.ProActiveSlaId
             , m.ProActiveSla
             , m.Sla
             , m.SlaHash

             , m.StdWarranty
             , m.StdWarrantyLocation

             --Cost

             , m.AvailabilityFee
             , m.TaxAndDutiesW
             , m.TaxAndDutiesOow
             , m.Reinsurance
             , m.ProActive
             , m.ServiceSupportCost
             , m.MaterialW
             , m.MaterialOow
             , m.FieldServiceCost
             , m.Logistic
             , m.OtherDirect
             , coalesce(m.LocalServiceStandardWarrantyManual, m.LocalServiceStandardWarranty) as LocalServiceStandardWarranty
             , m.LocalServiceStandardWarrantyWithRisk
             , m.Credits

             , ib.InstalledBaseCountryNorm

             , (sum(m.ServiceTCResult * ib.InstalledBaseCountryNorm)                          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tc 
             , (sum(case when m.ServiceTCResult <> 0 then ib.InstalledBaseCountryNorm end)    over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tc

             , (sum(m.ServiceTP_Released * ib.InstalledBaseCountryNorm)                       over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp_Released
             , (sum(case when m.ServiceTP_Released <> 0 then ib.InstalledBaseCountryNorm end) over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp_Released

             , (sum(m.ServiceTPResult * ib.InstalledBaseCountryNorm)                          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp
             , (sum(case when m.ServiceTPResult <> 0 then ib.InstalledBaseCountryNorm end)    over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp

             --##################################################

             , (sum(m.ReActiveTPResult * ib.InstalledBaseCountryNorm)                          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp_reactive
             , (sum(case when m.ReActiveTPResult <> 0 then ib.InstalledBaseCountryNorm end)    over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp_reactive
             
             , (sum(m.ProActive * ib.InstalledBaseCountryNorm)                                over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_pro
             , (sum(case when m.ProActive <> 0 then ib.InstalledBaseCountryNorm end)          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_pro
                                                                                            
             , m.ReleaseDate
             , m.ReleaseUserName as ReleaseUser

             , m.ListPrice
             , m.DealerDiscount
             , m.DealerPrice

        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, null, null) m
        join InputAtoms.Wg wg on wg.id = m.WgId and wg.DeactivatedDateTime is null
        left join Hardware.GetInstallBaseOverSog(@approved, @cnt) ib on ib.Country = m.CountryId and ib.Wg = m.WgId
    )
    select    
            m.Id

            --SLA

            , m.CountryId
            , m.Country
            , m.CurrencyId
            , m.Currency
            , m.ExchangeRate

            , m.WgId
            , m.Wg
            , m.WgDescription
            , m.SogId
            , m.Sog

            , m.AvailabilityId
            , m.Availability
            , m.DurationId
            , m.Duration
            , m.Year
            , m.IsProlongation
            , m.ReactionTimeId
            , m.ReactionTime
            , m.ReactionTypeId
            , m.ReactionType
            , m.ServiceLocationId
            , m.ServiceLocation
            , m.ProActiveSlaId
            , m.ProActiveSla
            , m.Sla
            , m.SlaHash

            , m.StdWarranty
            , m.StdWarrantyLocation

            --Cost

            , m.AvailabilityFee
            , m.TaxAndDutiesW
            , m.TaxAndDutiesOow
            , m.Reinsurance
            , m.ProActive
            , m.ServiceSupportCost
            , m.MaterialW
            , m.MaterialOow
            , m.FieldServiceCost
            , m.Logistic
            , m.OtherDirect
            , m.LocalServiceStandardWarranty
            , m.LocalServiceStandardWarrantyWithRisk
            , m.Credits

            , case when m.sum_ib_x_tc <> 0 and m.sum_ib_by_tc <> 0 then m.sum_ib_x_tc / m.sum_ib_by_tc else 0 end as ServiceTcSog
            , case when m.sum_ib_x_tp <> 0 and m.sum_ib_by_tp <> 0 then m.sum_ib_x_tp / m.sum_ib_by_tp else 0 end as ServiceTpSog
            , case when m.sum_ib_x_tp_Released <> 0 and m.sum_ib_by_tp_Released <> 0 then m.sum_ib_x_tp_Released / m.sum_ib_by_tp_Released 
                   when m.ReleaseDate is not null then 0 end as ServiceTpSog_Released

            , case when m.sum_ib_x_tp_reactive <> 0 and m.sum_ib_by_tp_reactive <> 0 then m.sum_ib_x_tp_reactive / m.sum_ib_by_tp_reactive else 0 end as ReactiveTpSog

            , case when m.sum_ib_x_pro <> 0 and m.sum_ib_by_pro <> 0 then m.sum_ib_x_pro / m.sum_ib_by_pro else 0 end as ProActiveSog

            , m.ReleaseDate
            , m.ReleaseUser

            , m.ListPrice
            , m.DealerDiscount
            , m.DealerPrice  

    from cte m
)

go

if OBJECT_ID('[Report].[GetParameterStd]') is not null
    drop function [Report].[GetParameterStd];
go

create function [Report].[GetParameterStd]
(
    @approved bit,
    @cnt bigint,
    @wg bigint
)
RETURNS @result table(
      CountryId                         bigint
    , Country                           nvarchar(255)
    , Currency                          nvarchar(255)
    , ExchangeRate                      float
    , TaxAndDuties                      float
                                        
    , WgId                              bigint
    , Wg                                nvarchar(255)   
    , WgDescription                     nvarchar(255)
    , SCD_ServiceType                   nvarchar(255)
    , SogDescription                    nvarchar(255)
    , RoleCodeId                        bigint
    , AFR1                              float  
    , AFR2                              float
    , AFR3                              float
    , AFR4                              float
    , AFR5                              float
    , AFRP1                             float
    , ReinsuranceFlatfee1               float
    , ReinsuranceFlatfee2               float
    , ReinsuranceFlatfee3               float
    , ReinsuranceFlatfee4               float
    , ReinsuranceFlatfee5               float
    , ReinsuranceFlatfeeP1              float
    , ReinsuranceUpliftFactor_4h_24x7   float
    , ReinsuranceUpliftFactor_4h_9x5    float
    , ReinsuranceUpliftFactor_NBD_9x5   float
    , [1stLevelSupportCosts]            float
    , [2ndLevelSupportCosts]            float
    , Sar                               float
    , MaterialCostWarranty              float
    , MaterialCostOow                   float
    , OnsiteHourlyRate                  float
    , Fee                               float
    , MarkupFactorStandardWarranty      float
    , MarkupStandardWarranty            float
    , RiskFactorStandardWarranty        float
    , RiskStandardWarranty              float
    , IB_per_Country					float
    , IB_per_PLA						float 
    , primary key clustered (CountryId, WgId)

)
as
begin

    with WgCte as (
        select wg.Id
             , wg.Name
             , wg.Description
             , wg.SCD_ServiceType
             , pla.ClusterPlaId
             , sog.Description as SogDescription
             , wg.RoleCodeId
        
             , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                               end as AFR1 
             , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                               end as AFR2 
             , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                           end as AFR3 
             , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                           end as AFR4 
             , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                           end as AFR5 
             , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                          end as AFRP1

             , r.ReinsuranceFlatfee1              
             , r.ReinsuranceFlatfee2              
             , r.ReinsuranceFlatfee3              
             , r.ReinsuranceFlatfee4              
             , r.ReinsuranceFlatfee5              
             , r.ReinsuranceFlatfeeP1             
             , r.ReinsuranceUpliftFactor_4h_24x7  
             , r.ReinsuranceUpliftFactor_4h_9x5   
             , r.ReinsuranceUpliftFactor_NBD_9x5  

             , case when @approved = 0 then ssc.[1stLevelSupportCosts] else ssc.[1stLevelSupportCosts_Approved] end as [1stLevelSupportCosts]
             , case when @approved = 0 then ssc.[2ndLevelSupportCosts] else ssc.[2ndLevelSupportCosts_Approved] end as [2ndLevelSupportCosts]
             , case when @approved = 0 then ssc.Sar	                   else ssc.Sar_Approved end                    as Sar 
             , case when @approved = 0 then mcw.MaterialCostIw else mcw.MaterialCostIw_Approved end as MaterialCostWarranty
             , case when @approved = 0 then mcw.MaterialCostOow else mcw.MaterialCostOow_Approved end as MaterialCostOow

             , case when @approved = 0 then hr.OnsiteHourlyRates else hr.OnsiteHourlyRates_Approved end as OnsiteHourlyRate

             , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee

             , case when @approved = 0 then msw.MarkupFactorStandardWarranty else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty

             , case when @approved = 0 then msw.MarkupStandardWarranty else msw.MarkupStandardWarranty_Approved end       as MarkupStandardWarranty
             , case when @approved = 0 then msw.RiskFactorStandardWarranty else msw.RiskFactorStandardWarranty_Approved end as RiskFactorStandardWarranty
             , case when @approved = 0 then msw.RiskStandardWarranty else msw.RiskStandardWarranty_Approved end as RiskStandardWarranty
             , case when @approved = 0 then ssc.Total_IB_Pla else ssc.Total_IB_Pla_Approved end as IB_per_PLA
             , case when @approved = 0 then ssc.TotalIb else ssc.TotalIb_Approved end as IB_per_Country

        from InputAtoms.Wg wg 

        INNER JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
        
        LEFT JOIN InputAtoms.Sog sog on sog.id = wg.SogId
        
        LEFT JOIN Hardware.AfrYear afr on afr.Wg = wg.Id
        
        LEFT JOIN Report.GetReinsuranceYear(@approved) r on r.Wg = wg.Id

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw on mcw.Country = @cnt and mcw.Wg = wg.Id

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = @cnt and ssc.ClusterPla = pla.ClusterPlaId

        LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.Country = @cnt and hr.RoleCode = wg.RoleCodeId 

        LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Country = @cnt AND msw.Wg = wg.Id and msw.Deactivated = 0

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = @cnt AND af.Wg = wg.Id

        where wg.Deactivated = 0 and (@wg is null or wg.Id = @wg)
    )
    , CountryCte as (
        select c.Id
             , c.Name
             , cur.Name as Currency
             , er.Value as ExchangeRate
             , case when @approved = 0 then tax.TaxAndDuties else tax.TaxAndDuties_Approved end as TaxAndDuties
        from InputAtoms.Country c 
        INNER JOIN [References].Currency cur on cur.Id = c.CurrencyId
        INNER JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDuties tax on tax.Country = c.Id and tax.Deactivated = 0
        where c.Id = @cnt
    )
    insert into @result
    select 
                c.Id
              , c.Name
              , Currency                         
              , ExchangeRate                     
              , TaxAndDuties                     
                                                 
              , wg.Id                             
              , wg.Name                               
              , wg.Description                    
              , SCD_ServiceType                  
              , SogDescription                   
              , RoleCodeId                       
              , AFR1                             
              , AFR2                             
              , AFR3                             
              , AFR4                             
              , AFR5                             
              , AFRP1                            
              , ReinsuranceFlatfee1              
              , ReinsuranceFlatfee2              
              , ReinsuranceFlatfee3              
              , ReinsuranceFlatfee4              
              , ReinsuranceFlatfee5              
              , ReinsuranceFlatfeeP1             
              , ReinsuranceUpliftFactor_4h_24x7  
              , ReinsuranceUpliftFactor_4h_9x5   
              , ReinsuranceUpliftFactor_NBD_9x5  
              , [1stLevelSupportCosts]           
              , [2ndLevelSupportCosts]
              , Sar
              , MaterialCostWarranty             
              , MaterialCostOow                  
              , OnsiteHourlyRate                 
              , Fee                              
              , MarkupFactorStandardWarranty     
              , MarkupStandardWarranty
              , RiskFactorStandardWarranty
              , RiskStandardWarranty
              , IB_per_Country
              , IB_per_PLA           

    from WgCte wg, CountryCte c;

    return
end
GO


if OBJECT_ID('[Report].[GetParameterHw]') is not null
    drop function [Report].[GetParameterHw];
go

create function [Report].[GetParameterHw]
(
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @duration     bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    with CostCte as (
            select 
                m.Id
                , m.CountryId
                , std.Country
                , std.WgDescription
                , std.Wg
                , std.SogDescription
                , std.SCD_ServiceType
                , pro.ExternalName as Sla
                , loc.Name as ServiceLocation
                , rtime.Name as ReactionTime
                , rtype.Name as ReactionType
                , av.Name as Availability
                , std.Currency
                , std.ExchangeRate

                --FSP
                , fsp.Name Fsp
                , fsp.ServiceDescription as FspDescription

                --cost blocks

                , case when @approved = 0 then fsc.LabourCost else fsc.LabourCost_Approved end as LabourCost
                , case when @approved = 0 then fsc.TravelCost else fsc.TravelCost_Approved end as TravelCost
                , case when @approved = 0 then fst.PerformanceRate else fst.PerformanceRate_Approved end as PerformanceRate
                , case when @approved = 0 then fsc.TravelTime else fsc.TravelTime_Approved end as TravelTime
                , case when @approved = 0 then fsc.RepairTime else fsc.RepairTime_Approved end as RepairTime
                , case when @approved = 0 then fst.TimeAndMaterialShare else fst.TimeAndMaterialShare_Approved end as TimeAndMaterialShare
                , case when @approved = 0 then  UpliftFactor.OohUpliftFactor else  UpliftFactor.OohUpliftFactor_Approved end as OohUpliftFactor
                , std.OnsiteHourlyRate


                , case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end         as StandardHandling
                , case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling 
                , case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end         as StandardDelivery
                , case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end          as ExpressDelivery
                , case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end      as TaxiCourierDelivery
                , case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end    as ReturnDeliveryFactory 
                
                , case when @approved = 0 
                        then lc.StandardHandling + lc.HighAvailabilityHandling 
                        else lc.StandardHandling_Approved + lc.HighAvailabilityHandling_Approved 
                    end as LogisticHandlingPerYear

                , case when @approved = 0 
                        then lc.StandardDelivery + lc.ExpressDelivery + lc.TaxiCourierDelivery + lc.ReturnDeliveryFactory 
                        else lc.StandardDelivery_Approved + lc.ExpressDelivery_Approved + lc.TaxiCourierDelivery_Approved + lc.ReturnDeliveryFactory_Approved 
                    end as LogisticTransportPerYear

                , case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee
      
                , std.TaxAndDuties as TaxAndDutiesW

                , case when @approved = 0 then moc.Markup else moc.Markup_Approved end as MarkupOtherCost                      
                , case when @approved = 0 then moc.MarkupFactor_norm else moc.MarkupFactor_norm_Approved end as MarkupFactorOtherCost                

                , std.MarkupFactorStandardWarranty
                , std.MarkupStandardWarranty
                , std.RiskFactorStandardWarranty
                , std.RiskStandardWarranty
      
                , std.AFR1
                , std.AFR2
                , std.AFR3
                , std.AFR4
                , std.AFR5
                , std.AFRP1
                , std.IB_per_Country
                , std.IB_per_PLA

                , case when dur.Value = 1 then std.AFR1 
                       when dur.Value = 2 then std.AFR1 + std.AFR2 
                       when dur.Value = 3 then std.AFR1 + std.AFR2 + std.AFR3 
                       when dur.Value = 4 then std.AFR1 + std.AFR2 + std.AFR3 + std.AFR4 
                       when dur.Value = 5 then std.AFR1 + std.AFR2 + std.AFR3 + std.AFR4 + std.AFR5
                    end AfrSum

                , case when @approved = 0 
                       then 
                            Hardware.CalcByFieldServicePerYear(
                                fst.TimeAndMaterialShare_norm, 
                                fsc.TravelCost, 
                                fsc.LabourCost, 
                                fst.PerformanceRate, 
                                std.ExchangeRate,
                                fsc.TravelTime,
                                fsc.repairTime,
                                1, --calc in local 
                                UpliftFactor.OohUpliftFactor)
                        else
                            Hardware.CalcByFieldServicePerYear(
                                fst.TimeAndMaterialShare_norm_Approved, 
                                fsc.TravelCost_Approved, 
                                fsc.LabourCost_Approved, 
                                fst.PerformanceRate_Approved, 
                                std.ExchangeRate,
                                fsc.TravelTime_Approved,
                                fsc.repairTime_Approved,
                                1, --calc in local 
                                UpliftFactor.OohUpliftFactor_Approved)
         
                   end as FieldServicePerYear

                , std.[1stLevelSupportCosts]
                , std.[2ndLevelSupportCosts]
                , std.Sar
           
                , std.ReinsuranceFlatfee1
                , std.ReinsuranceFlatfee2
                , std.ReinsuranceFlatfee3
                , std.ReinsuranceFlatfee4
                , std.ReinsuranceFlatfee5
                , std.ReinsuranceFlatfeeP1
                , std.ReinsuranceUpliftFactor_4h_24x7
                , std.ReinsuranceUpliftFactor_4h_9x5
                , std.ReinsuranceUpliftFactor_NBD_9x5

                , std.MaterialCostWarranty
                , std.MaterialCostOow

                , dur.Value as Duration
                , dur.IsProlongation

        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId 

        INNER JOIN Report.GetParameterStd(@approved, @cnt, @wg) std on std.CountryId = m.CountryId and std.WgId = m.WgId

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
        LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId and fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType
        LEFT JOIN Hardware.UpliftFactor ON UpliftFactor.Country = m.CountryId AND UpliftFactor.Wg = m.WgId AND UpliftFactor.[Availability] = m.AvailabilityId

        LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTimeType = m.ReactionTime_ReactionType
                                            AND lc.Deactivated = 0

        LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId 
                                               and moc.Wg = m.WgId 
                                               AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability 
                                               and moc.Deactivated = 0

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId 
                                            AND afEx.ReactionTimeId = m.ReactionTimeId 
                                            AND afEx.ReactionTypeId = m.ReactionTypeId 
                                            AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    
                m.Id
              , m.Country
              , m.WgDescription
              , m.Wg
              , m.SogDescription
              , m.SCD_ServiceType
              , m.Sla
              , m.ServiceLocation
              , m.ReactionTime
              , m.ReactionType
              , m.Availability

             , m.Currency

             --FSP
              , m.Fsp
              , m.FspDescription

              --cost blocks

              , m.LabourCost as LabourCost
              , m.TravelCost as TravelCost
              , m.PerformanceRate as PerformanceRate
              , m.TravelTime
              , m.RepairTime
              , m.OnsiteHourlyRate as OnsiteHourlyRate

              , m.TimeAndMaterialShare 
              , m.OohUpliftFactor

              , m.AvailabilityFee * m.ExchangeRate as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
              , m.RiskFactorStandardWarranty as RiskFactorStandardWarranty
              , m.RiskStandardWarranty as RiskStandardWarranty
      
              , m.AFR1   * 100 as AFR1
              , m.AFR2   * 100 as AFR2
              , m.AFR3   * 100 as AFR3
              , m.AFR4   * 100 as AFR4
              , m.AFR5   * 100 as AFR5
              , m.AFRP1  * 100 as AFRP1

              , m.[1stLevelSupportCosts] * m.ExchangeRate as [1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts] * m.ExchangeRate as [2ndLevelSupportCosts]
              , m.Sar * 100                               as Sar

              , m.ReinsuranceFlatfee1 * m.ExchangeRate as ReinsuranceFlatfee1
              , m.ReinsuranceFlatfee2 * m.ExchangeRate as ReinsuranceFlatfee2
              , m.ReinsuranceFlatfee3 * m.ExchangeRate as ReinsuranceFlatfee3
              , m.ReinsuranceFlatfee4 * m.ExchangeRate as ReinsuranceFlatfee4
              , m.ReinsuranceFlatfee5 * m.ExchangeRate as ReinsuranceFlatfee5
              , m.ReinsuranceFlatfeeP1 * m.ExchangeRate as ReinsuranceFlatfeeP1
              , m.ReinsuranceUpliftFactor_4h_24x7 as ReinsuranceUpliftFactor_4h_24x7
              , m.ReinsuranceUpliftFactor_4h_9x5 as ReinsuranceUpliftFactor_4h_9x5
              , m.ReinsuranceUpliftFactor_NBD_9x5 as ReinsuranceUpliftFactor_NBD_9x5

              , m.MaterialCostWarranty * m.ExchangeRate as MaterialCostWarranty
              , m.MaterialCostOow * m.ExchangeRate as MaterialCostOow

              , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Duration as varchar(1)) end as Duration

              , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
              , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
              , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
              , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
              , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
              , m.FieldServicePerYear * m.AFRP1 as FieldServiceCostP1
            
              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , m.LogisticHandlingPerYear * m.AfrSum as LogisticsHandling

              , m.LogisticTransportPerYear * m.AfrSum as LogisticTransportcost
              , m.IB_per_Country
              , m.IB_per_PLA

    from CostCte m
)
go

IF OBJECT_ID('Report.CalcParameterHw') IS NOT NULL
  DROP FUNCTION Report.CalcParameterHw;
go 

CREATE FUNCTION [Report].[CalcParameterHw]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @duration     bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint
)
RETURNS TABLE 
AS
RETURN (
    select    
                m.Id
              , m.Country
              , m.WgDescription
              , m.Wg
              , m.SogDescription
              , m.SCD_ServiceType
              , m.Sla
              , m.ServiceLocation
              , m.ReactionTime
              , m.ReactionType
              , m.Availability

             --FSP
              , m.Fsp
              , m.FspDescription

              --cost blocks

              , m.LabourCost as LabourCost
              , m.TravelCost as TravelCost
              , m.PerformanceRate as PerformanceRate
              , m.TravelTime
              , m.RepairTime
              , m.OnsiteHourlyRate as OnsiteHourlyRate

              , m.TimeAndMaterialShare
              , m.OohUpliftFactor

              , m.AvailabilityFee as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
              , m.RiskFactorStandardWarranty as RiskFactorStandardWarranty
              , m.RiskStandardWarranty as RiskStandardWarranty
      
              , m.AFR1
              , m.AFR2
              , m.AFR3
              , m.AFR4
              , m.AFR5
              , m.AFRP1

              , m.[1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts]
              , m.Sar

              , m.ReinsuranceFlatfee1
              , m.ReinsuranceFlatfee2
              , m.ReinsuranceFlatfee3
              , m.ReinsuranceFlatfee4
              , m.ReinsuranceFlatfee5
              , m.ReinsuranceFlatfeeP1
              , m.ReinsuranceUpliftFactor_4h_24x7 as ReinsuranceUpliftFactor_4h_24x7
              , m.ReinsuranceUpliftFactor_4h_9x5 as ReinsuranceUpliftFactor_4h_9x5
              , m.ReinsuranceUpliftFactor_NBD_9x5 as ReinsuranceUpliftFactor_NBD_9x5

              , m.MaterialCostWarranty
              , m.MaterialCostOow

              , m.Duration

              , m.FieldServiceCost1
              , m.FieldServiceCost2
              , m.FieldServiceCost3
              , m.FieldServiceCost4
              , m.FieldServiceCost5
              , m.FieldServiceCostP1

              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , m.LogisticsHandling

              , m.LogisticTransportcost
        
              , m.Currency
              , m.IB_per_Country
              , m.IB_per_PLA
    from Report.GetParameterHw(1, @cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m
)
go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCULATION-PARAMETER-HW');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Sales Product Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SCD_ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sla', 'SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Currency', 'Local Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'G_MATNR', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'FspDescription', 'G_MAKTX', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LabourCost', 'Labour cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TravelCost', 'Travel cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'PerformanceRate', 'Performance rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'TravelTime', 'Travel time (MTTT)', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'RepairTime', 'Repair time (MTTR)', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TimeAndMaterialShare', 'Time And Material Share', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'OohUpliftFactor', 'OOH uplift factor', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'OnsiteHourlyRate', 'Onsite hourly rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticsHandling', 'Logistics handling cost', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticTransportcost', 'Logistics transport cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'AvailabilityFee', 'Availability Fee', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TaxAndDutiesW', 'Tax & duties', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupFactorOtherCost', 'Markup factor for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MarkupOtherCost', 'Markup for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupFactorStandardWarranty', 'Markup factor for standard warranty local cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MarkupStandardWarranty', 'Markup for standard warranty local cost', 1, 1);
set @index = @index + 1;
INSERT into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) VALUES (@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'RiskFactorStandardWarranty', 'Standard Warranty risk factor', 1, 1);
set @index = @index + 1;
INSERT into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) VALUES (@reportId, @index, Report.GetReportColumnTypeByName('money'), 'RiskStandardWarranty', 'Standard Warranty risk', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR1', 'AFR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR2', 'AFR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR3', 'AFR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR4', 'AFR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR5', 'AFR5', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFRP1', 'AFR 1 year prolongation', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost1', 'Calculated Field Service Cost 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost2', 'Calculated Field Service Cost 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost3', 'Calculated Field Service Cost 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost4', 'Calculated Field Service Cost 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost5', 'Calculated Field Service Cost 5 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCostP1', 'Calculated Field Service Cost 1 year prolongation', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), '2ndLevelSupportCosts', '2nd level support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), '1stLevelSupportCosts', '1st level support cost', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'IB_per_PLA', 'IB per Cluster PLA', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'IB_per_Country', 'IB per Country', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'Sar', 'Service Attached Rate Factor', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee1', 'Reinsurance Flatfee 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee2', 'Reinsurance Flatfee 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee3', 'Reinsurance Flatfee 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee4', 'Reinsurance Flatfee 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee5', 'Reinsurance Flatfee 5 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfeeP1', 'Reinsurance Flatfee 1 year prolongation', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'ReinsuranceUpliftFactor_4h_24x7', 'Reinsurance uplift factor 4h 24x7 (%)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'ReinsuranceUpliftFactor_4h_9x5', 'Reinsurance uplift factor 4h 9x5 (%)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'ReinsuranceUpliftFactor_NBD_9x5', 'Reinsurance uplift factor NBD 9x5 (%)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialCostWarranty', 'Material cost iW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialCostOow', 'Material cost OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Service duration', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wghardware', 0), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability', 0), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('duration', 0), 'duration', 'Duration');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontime', 0), 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');




