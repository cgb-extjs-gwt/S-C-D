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