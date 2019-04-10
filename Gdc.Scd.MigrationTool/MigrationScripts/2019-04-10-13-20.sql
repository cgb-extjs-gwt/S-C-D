if OBJECT_ID('[Report].[GetParameterHw]') is not null
    drop function [Report].[GetParameterHw];
GO

create function [Report].[GetParameterHw]
(
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    with CountryCte as (
        select c.*
             , cur.Id as Currency
             , er.Value as ExchangeRate
             , case when @approved = 0 then tax.TaxAndDuties_norm else tax.TaxAndDuties_norm_Approved end as TaxAndDuties
        from InputAtoms.Country c 
        INNER JOIN [References].Currency cur on cur.Id = c.CurrencyId
        INNER JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = c.Id
        where c.Id = @cnt
    )
    , WgCte as (
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

             , case when @approved = 0 then r.ReinsuranceFlatfee1              else r.ReinsuranceFlatfee1_Approved              end as ReinsuranceFlatfee1
             , case when @approved = 0 then r.ReinsuranceFlatfee2              else r.ReinsuranceFlatfee2_Approved              end as ReinsuranceFlatfee2
             , case when @approved = 0 then r.ReinsuranceFlatfee3              else r.ReinsuranceFlatfee3_Approved              end as ReinsuranceFlatfee3
             , case when @approved = 0 then r.ReinsuranceFlatfee4              else r.ReinsuranceFlatfee4_Approved              end as ReinsuranceFlatfee4
             , case when @approved = 0 then r.ReinsuranceFlatfee5              else r.ReinsuranceFlatfee5_Approved              end as ReinsuranceFlatfee5
             , case when @approved = 0 then r.ReinsuranceFlatfeeP1             else r.ReinsuranceFlatfeeP1_Approved             end as ReinsuranceFlatfeeP1
             , case when @approved = 0 then r.ReinsuranceUpliftFactor_4h_24x7  else r.ReinsuranceUpliftFactor_4h_24x7_Approved  end as ReinsuranceUpliftFactor_4h_24x7
             , case when @approved = 0 then r.ReinsuranceUpliftFactor_4h_9x5   else r.ReinsuranceUpliftFactor_4h_9x5_Approved   end as ReinsuranceUpliftFactor_4h_9x5
             , case when @approved = 0 then r.ReinsuranceUpliftFactor_NBD_9x5  else r.ReinsuranceUpliftFactor_NBD_9x5_Approved  end as ReinsuranceUpliftFactor_NBD_9x5

        from InputAtoms.Wg wg 
        INNER JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
        LEFT JOIN InputAtoms.Sog sog on sog.id = wg.SogId
        LEFT JOIN Hardware.AfrYear afr on afr.Wg = wg.Id
        LEFT JOIN Hardware.ReinsuranceYear r on r.Wg = wg.Id
        where wg.DeactivatedDateTime is null and (@wg is null or wg.Id = @wg)
    )
    , CostCte as (
            select 
                m.Id
                , m.CountryId
                , c.Name as Country
                , wg.Description as WgDescription
                , wg.Name as Wg
                , wg.SogDescription
                , wg.SCD_ServiceType
                , pro.ExternalName as Sla
                , loc.Name as ServiceLocation
                , rtime.Name as ReactionTime
                , rtype.Name as ReactionType
                , av.Name as Availability
                , c.Currency
                , c.ExchangeRate

                --FSP
                , fsp.Name Fsp
                , fsp.ServiceDescription as FspDescription

                --cost blocks

                , case when @approved = 0 then fsc.LabourCost else fsc.LabourCost_Approved end as LabourCost
                , case when @approved = 0 then fsc.TravelCost else fsc.TravelCost_Approved end as TravelCost
                , case when @approved = 0 then fst.PerformanceRate else fst.PerformanceRate_Approved end as PerformanceRate
                , case when @approved = 0 then fsc.TravelTime else fsc.TravelTime_Approved end as TravelTime
                , case when @approved = 0 then fsc.RepairTime else fsc.RepairTime_Approved end as RepairTime
                , case when @approved = 0 then hr.OnsiteHourlyRates else hr.OnsiteHourlyRates_Approved end as OnsiteHourlyRate


                , case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end         as StandardHandling
                , case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling 
                , case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end         as StandardDelivery
                , case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end          as ExpressDelivery
                , case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end      as TaxiCourierDelivery
                , case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end    as ReturnDeliveryFactory 
                , case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end + case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end as LogisticHandlingPerYear
                , case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end + case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end + case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end + case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end as LogisticTransportPerYear

                , case when afEx.id is not null then case when @approved = 0 then af.Fee else af.Fee_Approved end else 0 end as AvailabilityFee
      
                , c.TaxAndDuties as TaxAndDutiesW

                , case when @approved = 0 then moc.Markup else moc.Markup_Approved end       as MarkupOtherCost
                , case when @approved = 0 then moc.MarkupFactor else moc.MarkupFactor_Approved end as MarkupFactorOtherCost

                , case when @approved = 0 then msw.MarkupFactorStandardWarranty else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty
                , case when @approved = 0 then msw.MarkupStandardWarranty else msw.MarkupStandardWarranty_Approved end       as MarkupStandardWarranty
      
                , wg.AFR1
                , wg.AFR2
                , wg.AFR3
                , wg.AFR4
                , wg.AFR5
                , wg.AFRP1

                , Hardware.CalcFieldServiceCost(
                            case when @approved = 0 then fst.TimeAndMaterialShare_norm else fst.TimeAndMaterialShare_norm_Approved end, 
                            case when @approved = 0 then fsc.TravelCost                else fsc.TravelCost_Approved end, 
                            case when @approved = 0 then fsc.LabourCost                else fsc.LabourCost_Approved end, 
                            case when @approved = 0 then fst.PerformanceRate           else fst.PerformanceRate_Approved end, 
                            case when @approved = 0 then fsc.TravelTime                else fsc.TravelTime_Approved end, 
                            case when @approved = 0 then fsc.RepairTime                else fsc.RepairTime_Approved end, 
                            case when @approved = 0 then hr.OnsiteHourlyRates          else hr.OnsiteHourlyRates_Approved end, 
                            1
                        ) as FieldServicePerYear

                , case when @approved = 0 then ssc.[1stLevelSupportCosts] else ssc.[1stLevelSupportCosts_Approved] end as [1stLevelSupportCosts]
                , case when @approved = 0 then ssc.[2ndLevelSupportCosts] else ssc.[2ndLevelSupportCosts_Approved] end as [2ndLevelSupportCosts]
           
                , wg.ReinsuranceFlatfee1
                , wg.ReinsuranceFlatfee2
                , wg.ReinsuranceFlatfee3
                , wg.ReinsuranceFlatfee4
                , wg.ReinsuranceFlatfee5
                , wg.ReinsuranceFlatfeeP1
                , wg.ReinsuranceUpliftFactor_4h_24x7
                , wg.ReinsuranceUpliftFactor_4h_9x5
                , wg.ReinsuranceUpliftFactor_NBD_9x5

                , case when @approved = 0 then mcw.MaterialCostIw else mcw.MaterialCostIw_Approved end as MaterialCostWarranty
                , case when @approved = 0 then mcw.MaterialCostOow else mcw.MaterialCostOow_Approved end as MaterialCostOow

                , dur.Value as Duration
                , dur.IsProlongation

        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN CountryCte c on c.Id = m.CountryId

        INNER JOIN WgCte wg on wg.Id = m.WgId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.Country = m.CountryId and hr.RoleCode = wg.RoleCodeId 

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
        LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

        LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTimeType = m.ReactionTime_ReactionType
                                            AND lc.DeactivatedDateTime is null

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw on mcw.Country = m.CountryId and mcw.Wg = m.WgId 

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPlaId

        LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId 
                                               AND moc.Country = m.CountryId 
                                               AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability 
                                               and moc.DeactivatedDateTime is null

        LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Country = m.CountryId AND msw.Wg = m.WgId and msw.DeactivatedDateTime is null

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

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

              , m.Duration

              , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
              , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
              , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
              , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
              , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
            
              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , Hardware.CalcByDur(
                      m.Duration
                    , m.IsProlongation 
                    , m.LogisticHandlingPerYear * m.AFR1 
                    , m.LogisticHandlingPerYear * m.AFR2 
                    , m.LogisticHandlingPerYear * m.AFR3 
                    , m.LogisticHandlingPerYear * m.AFR4 
                    , m.LogisticHandlingPerYear * m.AFR5 
                    , m.LogisticHandlingPerYear * m.AFRP1
                ) as LogisticsHandling

             , Hardware.CalcByDur(
                       m.Duration
                     , m.IsProlongation 
                     , m.LogisticTransportPerYear * m.AFR1 
                     , m.LogisticTransportPerYear * m.AFR2 
                     , m.LogisticTransportPerYear * m.AFR3 
                     , m.LogisticTransportPerYear * m.AFR4 
                     , m.LogisticTransportPerYear * m.AFR5 
                     , m.LogisticTransportPerYear * m.AFRP1
                 ) as LogisticTransportcost

            , m.Currency
    from CostCte m
)

go

ALTER FUNCTION [Report].[CalcParameterHw]
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
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
            
              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , m.LogisticsHandling

             , m.LogisticTransportcost

            , m.Currency
    from Report.GetParameterHw(1, @cnt, @wg, @av, @reactiontime, @reactiontype, @loc, @pro) m
)
go

ALTER FUNCTION [Report].[CalcParameterHwNotApproved]
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
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
              , m.Currency

             --FSP
              , m.Fsp
              , m.FspDescription

              --cost blocks

              , m.LabourCost
              , m.TravelCost
              , m.PerformanceRate
              , m.TravelTime
              , m.RepairTime
              , m.OnsiteHourlyRate

              , m.AvailabilityFee
      
              , m.TaxAndDutiesW

              , m.MarkupOtherCost
              , m.MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty
      
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

              , m.ReinsuranceUpliftFactor_4h_24x7
              , m.ReinsuranceUpliftFactor_4h_9x5
              , m.ReinsuranceUpliftFactor_NBD_9x5

              , m.MaterialCostWarranty
              , m.MaterialCostOow

              , m.Duration

              , m.FieldServiceCost1
              , m.FieldServiceCost2
              , m.FieldServiceCost3
              , m.FieldServiceCost4
              , m.FieldServiceCost5
         
              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 
         
              , m.LogisticsHandling
         
              , m.LogisticTransportcost

    from Report.GetParameterHw(0, @cnt, @wg, @av, @reactiontime, @reactiontype, @loc, @pro) m
)
go
