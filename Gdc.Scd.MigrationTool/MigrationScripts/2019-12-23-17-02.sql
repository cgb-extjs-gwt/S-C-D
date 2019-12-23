USE [SCD_2]
GO

/****** Object:  View [Hardware].[ServiceSupportCostView]    Script Date: 23.12.2019 16:56:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--ALTER VIEW 
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
		 , ssc.TotalIb
		 , ssc.TotalIb_Approved
		 , ssc.Total_IB_Pla
		 , ssc.Total_IB_Pla_Approved

         , case when ssc.TotalIb <> 0 and ssc.Total_IB_Pla <> 0 
                then ssc.[1stLevelSupportCosts] / ssc.TotalIb + ssc.[2ndLevelSupportCosts] / ssc.Total_IB_Pla
            end as ServiceSupport

         , case when ssc.TotalIb_Approved <> 0 and ssc.Total_IB_Pla_Approved <> 0 
                then ssc.[1stLevelSupportCosts_Approved] / ssc.TotalIb_Approved + ssc.[2ndLevelSupportCosts_Approved] / ssc.Total_IB_Pla_Approved
            end as ServiceSupport_Approved

    from cte ssc
GO

--ALTER GET PARAMETER STD
ALTER function [Report].[GetParameterStd]
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
    , MaterialCostWarranty              float
    , MaterialCostOow                   float
    , OnsiteHourlyRate                  float
    , Fee                               float
    , MarkupFactorStandardWarranty      float
    , MarkupStandardWarranty            float
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

             , case when @approved = 0 then mcw.MaterialCostIw else mcw.MaterialCostIw_Approved end as MaterialCostWarranty
             , case when @approved = 0 then mcw.MaterialCostOow else mcw.MaterialCostOow_Approved end as MaterialCostOow

             , case when @approved = 0 then hr.OnsiteHourlyRates else hr.OnsiteHourlyRates_Approved end as OnsiteHourlyRate

             , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee

             , case when @approved = 0 then msw.MarkupFactorStandardWarranty else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty

             , case when @approved = 0 then msw.MarkupStandardWarranty else msw.MarkupStandardWarranty_Approved end       as MarkupStandardWarranty
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
              , MaterialCostWarranty             
              , MaterialCostOow                  
              , OnsiteHourlyRate                 
              , Fee                              
              , MarkupFactorStandardWarranty     
              , MarkupStandardWarranty
			  , IB_per_Country
			  , IB_per_PLA           

    from WgCte wg, CountryCte c;

    return
end
GO

--ALTER GET PARAMETER HARDWARE
ALTER function [Report].[GetParameterHw]
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
                        then Hardware.CalcFieldServiceCost(
                                                  fst.TimeAndMaterialShare_norm 
                                                , fsc.TravelCost                
                                                , fsc.LabourCost                
                                                , fst.PerformanceRate           
                                                , fsc.TravelTime                
                                                , fsc.RepairTime                
                                                , std.OnsiteHourlyRate
                                                , 1
                                            ) 
                        else Hardware.CalcFieldServiceCost(
                                              fst.TimeAndMaterialShare_norm_Approved  
                                            , fsc.TravelCost_Approved  
                                            , fsc.LabourCost_Approved  
                                            , fst.PerformanceRate_Approved  
                                            , fsc.TravelTime_Approved  
                                            , fsc.RepairTime_Approved  
                                            , std.OnsiteHourlyRate 
                                            , 1
                                        )
                    end as FieldServicePerYear

                , std.[1stLevelSupportCosts]
                , std.[2ndLevelSupportCosts]
           
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

              , m.AvailabilityFee * m.ExchangeRate as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
      
              , m.AFR1   * 100 as AFR1
              , m.AFR2   * 100 as AFR2
              , m.AFR3   * 100 as AFR3
              , m.AFR4   * 100 as AFR4
              , m.AFR5   * 100 as AFR5
              , m.AFRP1  * 100 as AFRP1

              , m.[1stLevelSupportCosts] * m.ExchangeRate as [1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts] * m.ExchangeRate as [2ndLevelSupportCosts]
           
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
			  , m.TimeAndMaterialShare
			  , m.IB_per_Country
			  , m.IB_per_PLA

    from CostCte m
)
GO

--ALTER CALC PARAMETER HARDWARE
ALTER FUNCTION [Report].[CalcParameterHw]
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

              , m.AvailabilityFee as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
      
              , m.AFR1
              , m.AFR2
              , m.AFR3
              , m.AFR4
              , m.AFR5
              , m.AFRP1

              , m.[1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts]
           
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
			, m.TimeAndMaterialShare
			, m.IB_per_Country
			, m.IB_per_PLA
    from Report.GetParameterHw(1, @cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m
)
GO

DECLARE @ReportId bigint;
SELECT @ReportId = Id FROM [Report].[Report] WHERE Name = 'Calculation-Parameter-hw'

DECLARE @typeNumber bigint;
SELECT @typeNumber = Id FROM [Report].[ReportColumnType] WHERE Name = 'number'


UPDATE [SCD_2].[Report].[ReportColumn]
SET [Index] = [Index] + 1
WHERE ReportId = @ReportId AND [Index] > 18

UPDATE [SCD_2].[Report].[ReportColumn]
SET [Index] = [Index] + 2
WHERE ReportId = @ReportId AND [Index] > 48

INSERT INTO [Report].[ReportColumn]
           ([AllowNull]
           ,[Flex]
           ,[Index]
           ,[Name]
           ,[ReportId]
           ,[Text]
           ,[TypeId]
           ,[Format])
VALUES
           (1, 1, 19, 'TimeAndMaterialShare'
           ,@ReportId 
           ,'Time And Material Share'
           ,@typeNumber
           ,NULL)

INSERT INTO [Report].[ReportColumn]
           ([AllowNull]
           ,[Flex]
           ,[Index]
           ,[Name]
           ,[ReportId]
           ,[Text]
           ,[TypeId]
           ,[Format])
VALUES
           (1, 1, 49, 'IB_per_PLA'
           ,@ReportId 
           ,'IB per Cluster PLA'
           ,@typeNumber
           ,NULL)


INSERT INTO [Report].[ReportColumn]
           ([AllowNull]
           ,[Flex]
           ,[Index]
           ,[Name]
           ,[ReportId]
           ,[Text]
           ,[TypeId]
           ,[Format])
VALUES
           (1, 1, 50, 'IB_per_Country'
           ,@ReportId 
           ,'IB per Country'
           ,@typeNumber
           ,NULL)
GO