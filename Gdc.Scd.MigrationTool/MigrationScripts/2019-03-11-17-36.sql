ALTER VIEW [Fsp].[HwStandardWarrantyView] AS
    SELECT std.Wg
         , std.Country
         , std.Duration
         , dur.Name
         , dur.IsProlongation
         , dur.Value as DurationValue
         , std.AvailabilityId
         , std.ReactionTimeId
         , std.ReactionTypeId
         , std.ServiceLocationId
         , loc.Name as ServiceLocation
         , std.ProActiveSlaId
         , std.ReactionTime_Avalability
         , std.ReactionTime_ReactionType
         , std.ReactionTime_ReactionType_Avalability
    FROM fsp.HwStandardWarranty std
    INNER JOIN Dependencies.Duration dur on dur.Id = std.Duration
    INNER JOIN Dependencies.ServiceLocation loc on loc.id = std.ServiceLocationId
GO

ALTER FUNCTION [Hardware].[GetCalcMemberSla] (
    @approved       bit,
    @sla            Portfolio.Sla readonly
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT    m.Id

            --SLA

            , m.CountryId          
            , c.Name               as Country
            , c.CurrencyId
            , er.Value             as ExchangeRate
            , m.WgId
            , wg.Name              as Wg
            , m.DurationId
            , dur.Name             as Duration
            , dur.Value            as Year
            , dur.IsProlongation   as IsProlongation
            , m.AvailabilityId
            , av.Name              as Availability
            , m.ReactionTimeId
            , rtime.Name           as ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Fsp
            , m.FspDescription

            , m.Sla
            , m.SlaHash

            , stdw.DurationValue   as StdWarranty
            , stdw.ServiceLocation as StdWarrantyLocation

            --Cost values

            , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                           end as AFR1 
            , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                           end as AFR2 
            , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
            , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
            , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
            , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1
                                                                              
            , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved       end as MaterialCostWarranty
            , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved            end as MaterialCostOow     
                                                                                                                      
            , case when @approved = 0 then tax.TaxAndDuties_norm              else tax.TaxAndDuties_norm_Approved          end as TaxAndDuties
                                                                                                                      
            , case when @approved = 0 then r.Cost                             else r.Cost_Approved                         end as Reinsurance

            --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then fscStd.LabourCost                  else fscStd.LabourCost_Approved              end / er.Value as StdLabourCost             
            , case when @approved = 0 then fscStd.TravelCost                  else fscStd.TravelCost_Approved              end / er.Value as StdTravelCost             
            , case when @approved = 0 then fstStd.PerformanceRate             else fstStd.PerformanceRate_Approved         end / er.Value as StdPerformanceRate        

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end / er.Value as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end / er.Value as TravelCost             
            , case when @approved = 0 then fst.PerformanceRate                else fst.PerformanceRate_Approved            end / er.Value as PerformanceRate        
            , case when @approved = 0 then fst.TimeAndMaterialShare_norm      else fst.TimeAndMaterialShare_norm_Approved  end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             
            , case when @approved = 0 then hr.OnsiteHourlyRates               else hr.OnsiteHourlyRates_Approved           end as OnsiteHourlyRates      
                       
            --##### SERVICE SUPPORT COST #########                                                                                               
            , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]  else ssc.[1stLevelSupportCostsCountry_Approved] end / er.Value as [1stLevelSupportCosts] 
            , case when @approved = 0 
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion] end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                end as [2ndLevelSupportCosts] 
            , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
            , case when @approved = 0
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                end as TotalIbPla

            --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then lcStd.ExpressDelivery              else lcStd.ExpressDelivery_Approved          end / er.Value as StdExpressDelivery         
            , case when @approved = 0 then lcStd.HighAvailabilityHandling     else lcStd.HighAvailabilityHandling_Approved end / er.Value as StdHighAvailabilityHandling
            , case when @approved = 0 then lcStd.StandardDelivery             else lcStd.StandardDelivery_Approved         end / er.Value as StdStandardDelivery        
            , case when @approved = 0 then lcStd.StandardHandling             else lcStd.StandardHandling_Approved         end / er.Value as StdStandardHandling        
            , case when @approved = 0 then lcStd.ReturnDeliveryFactory        else lcStd.ReturnDeliveryFactory_Approved    end / er.Value as StdReturnDeliveryFactory   
            , case when @approved = 0 then lcStd.TaxiCourierDelivery          else lcStd.TaxiCourierDelivery_Approved      end / er.Value as StdTaxiCourierDelivery     

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end / er.Value as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end / er.Value as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end / er.Value as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end / er.Value as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end / er.Value as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end / er.Value as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                    else 0
               end as AvailabilityFee

            , case when @approved = 0 then moc.Markup                              else moc.Markup_Approved                            end / er.Value as MarkupOtherCost                      
            , case when @approved = 0 then moc.MarkupFactor_norm                   else moc.MarkupFactor_norm_Approved                 end as MarkupFactorOtherCost                
                                                                                                                                     
            , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / er.Value as MarkupStandardWarranty      
            , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end as MarkupFactorStandardWarranty

            , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate
                else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved
               end as LocalRemoteAccessSetup

            --####### PROACTIVE COST ###################
            , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * prosla.LocalRegularUpdateReadyRepetition 
                else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRegularUpdateReadyRepetition 
               end as LocalRegularUpdate

            , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * prosla.LocalPreparationShcRepetition 
                else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalPreparationShcRepetition 
               end as LocalPreparation

            , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalRemoteShcCustomerBriefingRepetition 
                else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRemoteShcCustomerBriefingRepetition 
               end as LocalRemoteCustomerBriefing

            , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalOnsiteShcCustomerBriefingRepetition 
                else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalOnsiteShcCustomerBriefingRepetition 
               end as LocalOnsiteCustomerBriefing

            , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate * prosla.TravellingTimeRepetition 
                else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved * prosla.TravellingTimeRepetition 
               end as Travel

            , case when @approved = 0 then pro.CentralExecutionShcReportCost * prosla.CentralExecutionShcReportRepetition 
                else pro.CentralExecutionShcReportCost_Approved * prosla.CentralExecutionShcReportRepetition 
               end as CentralExecutionReport

            --########## MANUAL COSTS ################
            , man.ListPrice          / er.Value as ListPrice                   
            , man.DealerDiscount                as DealerDiscount              
            , man.DealerPrice        / er.Value as DealerPrice                 
            , man.ServiceTC          / er.Value as ServiceTCManual                   
            , man.ServiceTP          / er.Value as ServiceTPManual                   
            , man.ServiceTP_Released / er.Value as ServiceTP_Released                  
            , u.Name                            as ChangeUserName
            , u.Email                           as ChangeUserEmail

    FROM @sla m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId and hr.Country = m.CountryId

    LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.ServiceSupportCost ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.FieldServiceCalc fscStd ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lcStd on lcStd.Country = stdw.Country AND lcStd.Wg = stdw.Wg AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability

    LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Wg = m.WgId AND msw.Country = m.CountryId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId
)

go

ALTER FUNCTION [Hardware].[GetCostsSla](
    @approved       bit,
    @sla            Portfolio.Sla readonly
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , case when m.TaxAndDuties is null then 0 else m.TaxAndDuties end as TaxAndDutiesOrZero

                , case when m.Reinsurance is null then 0 else m.Reinsurance end as ReinsuranceOrZero

                , case when m.AvailabilityFee is null then 0 else m.AvailabilityFee end as AvailabilityFeeOrZero

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

                , m.StdLabourCost + m.StdTravelCost + coalesce(m.StdPerformanceRate, 0) as FieldServicePerYearStdw

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StdStandardHandling + m.StdHighAvailabilityHandling + m.StdStandardDelivery + m.StdExpressDelivery + m.StdTaxiCourierDelivery + m.StdReturnDeliveryFactory as LogisticPerYearStdw

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive
       
        from Hardware.GetCalcMemberSla(@approved, @sla) m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdWarranty >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdWarranty >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdWarranty >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdWarranty >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5
                , 0  as mat1P

                , case when m.StdWarranty >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdWarranty >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdWarranty >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdWarranty >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdWarranty >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1 as matO1P

                , m.FieldServicePerYearStdw * m.AFR1  as FieldServiceCostStdw1
                , m.FieldServicePerYearStdw * m.AFR2  as FieldServiceCostStdw2
                , m.FieldServicePerYearStdw * m.AFR3  as FieldServiceCostStdw3
                , m.FieldServicePerYearStdw * m.AFR4  as FieldServiceCostStdw4
                , m.FieldServicePerYearStdw * m.AFR5  as FieldServiceCostStdw5

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYearStdw * m.AFR1  as LogisticStdw1
                , m.LogisticPerYearStdw * m.AFR2  as LogisticStdw2
                , m.LogisticPerYearStdw * m.AFR3  as LogisticStdw3
                , m.LogisticPerYearStdw * m.AFR4  as LogisticStdw4
                , m.LogisticPerYearStdw * m.AFR5  as LogisticStdw5

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdWarranty >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdWarranty >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdWarranty >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdWarranty >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5
                , 0  as tax1P

                , case when m.StdWarranty >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdWarranty >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdWarranty >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdWarranty >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdWarranty >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5
                , m.TaxAndDutiesOrZero * m.matO1P as taxO1P

                , m.mat1  + m.matO1                     as matCost1
                , m.mat2  + m.matO2                     as matCost2
                , m.mat3  + m.matO3                     as matCost3
                , m.mat4  + m.matO4                     as matCost4
                , m.mat5  + m.matO5                     as matCost5
                , m.mat1P + m.matO1P                    as matCost1P

                , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
                , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
                , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
                , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
                , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
                , m.TaxAndDutiesOrZero * (m.mat1P + m.matO1P) as TaxAndDuties1P

        from CostCte2 m
    )
    , CostCte3 as (
        select    
                  m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw1, m.ServiceSupportPerYear, m.LogisticStdw1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw2, m.ServiceSupportPerYear, m.LogisticStdw2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw3, m.ServiceSupportPerYear, m.LogisticStdw3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw4, m.ServiceSupportPerYear, m.LogisticStdw4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw5, m.ServiceSupportPerYear, m.LogisticStdw5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5

        from CostCte2_2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceOrZero + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1  as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceOrZero + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2  as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceOrZero + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3  as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceOrZero + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4  as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceOrZero + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5  as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceOrZero + m.OtherDirect1P + m.AvailabilityFeeOrZero              as ServiceTP1P

        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
                , case when m.ServiceTP1  < m.OtherDirect1  then 0 else m.ServiceTP1  - m.OtherDirect1  end as ServiceTC1
                , case when m.ServiceTP2  < m.OtherDirect2  then 0 else m.ServiceTP2  - m.OtherDirect2  end as ServiceTC2
                , case when m.ServiceTP3  < m.OtherDirect3  then 0 else m.ServiceTP3  - m.OtherDirect3  end as ServiceTC3
                , case when m.ServiceTP4  < m.OtherDirect4  then 0 else m.ServiceTP4  - m.OtherDirect4  end as ServiceTC4
                , case when m.ServiceTP5  < m.OtherDirect5  then 0 else m.ServiceTP5  - m.OtherDirect5  end as ServiceTC5
                , case when m.ServiceTP1P < m.OtherDirect1P then 0 else m.ServiceTP1P - m.OtherDirect1P end as ServiceTC1P
        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.ExchangeRate
         , m.WgId
         , m.Wg
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

         , m.Fsp
         , m.FspDescription

         , m.Sla
         , m.SlaHash

         , m.StdWarranty
         , m.StdWarrantyLocation

         --Cost

         , m.AvailabilityFee * m.Year as AvailabilityFee
         , m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
         , m.taxO1 + m.taxO2 + m.taxO3 + m.taxO4 + m.taxO5 as TaxAndDutiesOow
         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
         , m.matO1 + m.matO2 + m.matO3 + m.matO4 + m.matO5 as MaterialOow

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
       
         , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
       
         , m.Credit1 + m.Credit2 + m.Credit3 + m.Credit4 + m.Credit5 as Credits

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ServiceTC
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP

         , m.ServiceTC1
         , m.ServiceTC2
         , m.ServiceTC3
         , m.ServiceTC4
         , m.ServiceTC5
         , m.ServiceTC1P

         , m.ServiceTP1
         , m.ServiceTP2
         , m.ServiceTP3
         , m.ServiceTP4
         , m.ServiceTP5
         , m.ServiceTP1P

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ServiceTPManual
         , m.ServiceTP_Released

         , m.ChangeUserName
         , m.ChangeUserEmail

       from CostCte6 m
)

go

ALTER PROCEDURE [Hardware].[SpGetCosts]
    @approved     bit,
    @local        bit,
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @lastid       bigint,
    @limit        int,
    @total        int output
AS
BEGIN

    SET NOCOUNT ON;

    select @total = COUNT(Id) from Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    declare @sla Portfolio.Sla;
    insert into @sla select * from Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    declare @cur nvarchar(max);
    declare @exchange float;

    if @local = 1
    begin
    
        --convert values from EUR to local

        select costs.Id

             , Country
             , cur.Name as Currency
             , costs.ExchangeRate

             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty
             , StdWarrantyLocation

             --Cost

             , AvailabilityFee               * costs.ExchangeRate  as AvailabilityFee 
             , TaxAndDutiesW                 * costs.ExchangeRate  as TaxAndDutiesW
             , TaxAndDutiesOow               * costs.ExchangeRate  as TaxAndDutiesOow
             , Reinsurance                   * costs.ExchangeRate  as Reinsurance
             , ProActive                     * costs.ExchangeRate  as ProActive
             , ServiceSupportCost            * costs.ExchangeRate  as ServiceSupportCost

             , MaterialW                     * costs.ExchangeRate  as MaterialW
             , MaterialOow                   * costs.ExchangeRate  as MaterialOow
             , FieldServiceCost              * costs.ExchangeRate  as FieldServiceCost
             , Logistic                      * costs.ExchangeRate  as Logistic
             , OtherDirect                   * costs.ExchangeRate  as OtherDirect
             , LocalServiceStandardWarranty  * costs.ExchangeRate  as LocalServiceStandardWarranty
             , Credits                       * costs.ExchangeRate  as Credits
             , ServiceTC                     * costs.ExchangeRate  as ServiceTC
             , ServiceTP                     * costs.ExchangeRate  as ServiceTP

             , ServiceTCManual               * costs.ExchangeRate  as ServiceTCManual
             , ServiceTPManual               * costs.ExchangeRate  as ServiceTPManual

             , ServiceTP_Released            * costs.ExchangeRate  as ServiceTP_Released

             , ListPrice                     * costs.ExchangeRate  as ListPrice
             , DealerPrice                   * costs.ExchangeRate  as DealerPrice
             , DealerDiscount                                      as DealerDiscount
                                                             
             , ChangeUserName                                      as ChangeUserName
             , ChangeUserEmail                                     as ChangeUserEmail

        from Hardware.GetCostsSla(@approved, @sla) costs
        join [References].Currency cur on cur.Id = costs.CurrencyId
        order by Id
        
    end
    else
    begin

        select costs.Id

             , Country
             , 'EUR' as Currency
             , costs.ExchangeRate

             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty
             , StdWarrantyLocation

             --Cost

             , AvailabilityFee               
             , TaxAndDutiesW                 
             , TaxAndDutiesOow               
             , Reinsurance                   
             , ProActive                     
             , ServiceSupportCost            

             , MaterialW                     
             , MaterialOow                   
             , FieldServiceCost              
             , Logistic                      
             , OtherDirect                   
             , LocalServiceStandardWarranty  
             , Credits                       
             , ServiceTC                     
             , ServiceTP                     

             , ServiceTCManual               
             , ServiceTPManual               

             , ServiceTP_Released            

             , ListPrice                     
             , DealerPrice                   
             , DealerDiscount                
                                             
             , ChangeUserName                
             , ChangeUserEmail               

        from Hardware.GetCostsSla(@approved, @sla) costs
        order by Id
    end
END

GO
ALTER FUNCTION [Hardware].[GetCalcMember] (
    @approved       bit,
    @cnt            dbo.ListID readonly,
    @wg             dbo.ListID readonly,
    @av             dbo.ListID readonly,
    @dur            dbo.ListID readonly,
    @reactiontime   dbo.ListID readonly,
    @reactiontype   dbo.ListID readonly,
    @loc            dbo.ListID readonly,
    @pro            dbo.ListID readonly,
    @lastid         bigint,
    @limit          int
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT    m.Id

            --SLA

            , m.CountryId          
            , c.Name               as Country
            , c.CurrencyId
            , er.Value             as ExchangeRate
            , m.WgId
            , wg.Name              as Wg
            , m.DurationId
            , dur.Name             as Duration
            , dur.Value            as Year
            , dur.IsProlongation   as IsProlongation
            , m.AvailabilityId
            , av.Name              as Availability
            , m.ReactionTimeId
            , rtime.Name           as ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

            , stdw.DurationValue   as StdWarranty
            , stdw.ServiceLocation as StdWarrantyLocation

            --Cost values

            , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                           end as AFR1 
            , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                           end as AFR2 
            , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
            , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
            , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
            , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1
                                                                              
            , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved       end as MaterialCostWarranty
            , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved            end as MaterialCostOow     
                                                                                                                      
            , case when @approved = 0 then tax.TaxAndDuties_norm              else tax.TaxAndDuties_norm_Approved          end as TaxAndDuties
                                                                                                                      
            , case when @approved = 0 then r.Cost                             else r.Cost_Approved                         end as Reinsurance

            --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then fscStd.LabourCost                  else fscStd.LabourCost_Approved              end / er.Value as StdLabourCost             
            , case when @approved = 0 then fscStd.TravelCost                  else fscStd.TravelCost_Approved              end / er.Value as StdTravelCost             
            , case when @approved = 0 then fstStd.PerformanceRate             else fstStd.PerformanceRate_Approved         end / er.Value as StdPerformanceRate        

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end / er.Value as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end / er.Value as TravelCost             
            , case when @approved = 0 then fst.PerformanceRate                else fst.PerformanceRate_Approved            end / er.Value as PerformanceRate        
            , case when @approved = 0 then fst.TimeAndMaterialShare_norm      else fst.TimeAndMaterialShare_norm_Approved  end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             
            , case when @approved = 0 then hr.OnsiteHourlyRates               else hr.OnsiteHourlyRates_Approved           end as OnsiteHourlyRates      
                       
            --##### SERVICE SUPPORT COST #########                                                                                               
            , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]  else ssc.[1stLevelSupportCostsCountry_Approved] end / er.Value as [1stLevelSupportCosts] 
            , case when @approved = 0 
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion] end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / er.Value else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                end as [2ndLevelSupportCosts] 
            , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
            , case when @approved = 0
                    then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                    else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                end as TotalIbPla

            --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
            , case when @approved = 0 then lcStd.ExpressDelivery              else lcStd.ExpressDelivery_Approved          end / er.Value as StdExpressDelivery         
            , case when @approved = 0 then lcStd.HighAvailabilityHandling     else lcStd.HighAvailabilityHandling_Approved end / er.Value as StdHighAvailabilityHandling
            , case when @approved = 0 then lcStd.StandardDelivery             else lcStd.StandardDelivery_Approved         end / er.Value as StdStandardDelivery        
            , case when @approved = 0 then lcStd.StandardHandling             else lcStd.StandardHandling_Approved         end / er.Value as StdStandardHandling        
            , case when @approved = 0 then lcStd.ReturnDeliveryFactory        else lcStd.ReturnDeliveryFactory_Approved    end / er.Value as StdReturnDeliveryFactory   
            , case when @approved = 0 then lcStd.TaxiCourierDelivery          else lcStd.TaxiCourierDelivery_Approved      end / er.Value as StdTaxiCourierDelivery     

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end / er.Value as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end / er.Value as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end / er.Value as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end / er.Value as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end / er.Value as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end / er.Value as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                    else 0
               end as AvailabilityFee

            , case when @approved = 0 then moc.Markup                              else moc.Markup_Approved                            end / er.Value as MarkupOtherCost                      
            , case when @approved = 0 then moc.MarkupFactor_norm                   else moc.MarkupFactor_norm_Approved                 end as MarkupFactorOtherCost                
                                                                                                                                     
            , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / er.Value as MarkupStandardWarranty      
            , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end as MarkupFactorStandardWarranty

            , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate
                else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved
               end as LocalRemoteAccessSetup

            --####### PROACTIVE COST ###################
            , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * prosla.LocalRegularUpdateReadyRepetition 
                else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRegularUpdateReadyRepetition 
               end as LocalRegularUpdate

            , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * prosla.LocalPreparationShcRepetition 
                else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalPreparationShcRepetition 
               end as LocalPreparation

            , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalRemoteShcCustomerBriefingRepetition 
                else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRemoteShcCustomerBriefingRepetition 
               end as LocalRemoteCustomerBriefing

            , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalOnsiteShcCustomerBriefingRepetition 
                else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalOnsiteShcCustomerBriefingRepetition 
               end as LocalOnsiteCustomerBriefing

            , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate * prosla.TravellingTimeRepetition 
                else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved * prosla.TravellingTimeRepetition 
               end as Travel

            , case when @approved = 0 then pro.CentralExecutionShcReportCost * prosla.CentralExecutionShcReportRepetition 
                else pro.CentralExecutionShcReportCost_Approved * prosla.CentralExecutionShcReportRepetition 
               end as CentralExecutionReport

            --########## MANUAL COSTS ################
            , man.ListPrice          / er.Value as ListPrice                   
            , man.DealerDiscount                as DealerDiscount              
            , man.DealerPrice        / er.Value as DealerPrice                 
            , man.ServiceTC          / er.Value as ServiceTCManual                   
            , man.ServiceTP          / er.Value as ServiceTPManual                   
            , man.ServiceTP_Released / er.Value as ServiceTP_Released                  
            , u.Name                            as ChangeUserName
            , u.Email                           as ChangeUserEmail

    FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId and hr.Country = m.CountryId

    LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.ServiceSupportCost ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.FieldServiceCalc fscStd ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lcStd on lcStd.Country = stdw.Country AND lcStd.Wg = stdw.Wg AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability

    LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Wg = m.WgId AND msw.Country = m.CountryId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId
)

GO
ALTER FUNCTION [Hardware].[GetCosts](
    @approved bit,
    @cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , case when m.TaxAndDuties is null then 0 else m.TaxAndDuties end as TaxAndDutiesOrZero

                , case when m.Reinsurance is null then 0 else m.Reinsurance end as ReinsuranceOrZero

                , case when m.AvailabilityFee is null then 0 else m.AvailabilityFee end as AvailabilityFeeOrZero

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

                , m.StdLabourCost + m.StdTravelCost + coalesce(m.StdPerformanceRate, 0) as FieldServicePerYearStdw

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StdStandardHandling + m.StdHighAvailabilityHandling + m.StdStandardDelivery + m.StdExpressDelivery + m.StdTaxiCourierDelivery + m.StdReturnDeliveryFactory as LogisticPerYearStdw

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive
       
        from Hardware.GetCalcMember(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdWarranty >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdWarranty >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdWarranty >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdWarranty >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5
                , 0  as mat1P

                , case when m.StdWarranty >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdWarranty >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdWarranty >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdWarranty >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdWarranty >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1 as matO1P

                , m.FieldServicePerYearStdw * m.AFR1  as FieldServiceCostStdw1
                , m.FieldServicePerYearStdw * m.AFR2  as FieldServiceCostStdw2
                , m.FieldServicePerYearStdw * m.AFR3  as FieldServiceCostStdw3
                , m.FieldServicePerYearStdw * m.AFR4  as FieldServiceCostStdw4
                , m.FieldServicePerYearStdw * m.AFR5  as FieldServiceCostStdw5

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYearStdw * m.AFR1  as LogisticStdw1
                , m.LogisticPerYearStdw * m.AFR2  as LogisticStdw2
                , m.LogisticPerYearStdw * m.AFR3  as LogisticStdw3
                , m.LogisticPerYearStdw * m.AFR4  as LogisticStdw4
                , m.LogisticPerYearStdw * m.AFR5  as LogisticStdw5

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdWarranty >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdWarranty >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdWarranty >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdWarranty >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5
                , 0  as tax1P

                , case when m.StdWarranty >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdWarranty >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdWarranty >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdWarranty >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdWarranty >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5
                , m.TaxAndDutiesOrZero * m.matO1P as taxO1P

                , m.mat1  + m.matO1                     as matCost1
                , m.mat2  + m.matO2                     as matCost2
                , m.mat3  + m.matO3                     as matCost3
                , m.mat4  + m.matO4                     as matCost4
                , m.mat5  + m.matO5                     as matCost5
                , m.mat1P + m.matO1P                    as matCost1P

                , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
                , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
                , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
                , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
                , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
                , m.TaxAndDutiesOrZero * (m.mat1P + m.matO1P) as TaxAndDuties1P

        from CostCte2 m
    )
    , CostCte3 as (
        select    
                  m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw1, m.ServiceSupportPerYear, m.LogisticStdw1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw2, m.ServiceSupportPerYear, m.LogisticStdw2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw3, m.ServiceSupportPerYear, m.LogisticStdw3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw4, m.ServiceSupportPerYear, m.LogisticStdw4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw5, m.ServiceSupportPerYear, m.LogisticStdw5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5

        from CostCte2_2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceOrZero + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1  as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceOrZero + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2  as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceOrZero + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3  as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceOrZero + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4  as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceOrZero + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5  as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceOrZero + m.OtherDirect1P + m.AvailabilityFeeOrZero              as ServiceTP1P

        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
                , case when m.ServiceTP1  < m.OtherDirect1  then 0 else m.ServiceTP1  - m.OtherDirect1  end as ServiceTC1
                , case when m.ServiceTP2  < m.OtherDirect2  then 0 else m.ServiceTP2  - m.OtherDirect2  end as ServiceTC2
                , case when m.ServiceTP3  < m.OtherDirect3  then 0 else m.ServiceTP3  - m.OtherDirect3  end as ServiceTC3
                , case when m.ServiceTP4  < m.OtherDirect4  then 0 else m.ServiceTP4  - m.OtherDirect4  end as ServiceTC4
                , case when m.ServiceTP5  < m.OtherDirect5  then 0 else m.ServiceTP5  - m.OtherDirect5  end as ServiceTC5
                , case when m.ServiceTP1P < m.OtherDirect1P then 0 else m.ServiceTP1P - m.OtherDirect1P end as ServiceTC1P
        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.ExchangeRate
         , m.WgId
         , m.Wg
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

         , m.AvailabilityFee * m.Year as AvailabilityFee
         , m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
         , m.taxO1 + m.taxO2 + m.taxO3 + m.taxO4 + m.taxO5 as TaxAndDutiesOow
         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
         , m.matO1 + m.matO2 + m.matO3 + m.matO4 + m.matO5 as MaterialOow

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
       
         , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
       
         , m.Credit1 + m.Credit2 + m.Credit3 + m.Credit4 + m.Credit5 as Credits

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ServiceTC
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP

         , m.ServiceTC1
         , m.ServiceTC2
         , m.ServiceTC3
         , m.ServiceTC4
         , m.ServiceTC5
         , m.ServiceTC1P

         , m.ServiceTP1
         , m.ServiceTP2
         , m.ServiceTP3
         , m.ServiceTP4
         , m.ServiceTP5
         , m.ServiceTP1P

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ServiceTPManual
         , m.ServiceTP_Released

         , m.ChangeUserName
         , m.ChangeUserEmail

       from CostCte6 m
)
GO

IF OBJECT_ID('Report.HwCalcResult') IS NOT NULL
  DROP FUNCTION Report.HwCalcResult;
go 

CREATE FUNCTION Report.HwCalcResult
(
    @approved bit,
    @local bit,
    @country         dbo.ListID readonly,
    @wg              dbo.ListID readonly,
    @availability    dbo.ListID readonly,
    @duration        dbo.ListID readonly,
    @reactiontime    dbo.ListID readonly,
    @reactiontype    dbo.ListID readonly,
    @servicelocation dbo.ListID readonly,
    @proactive       dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select    Country
            , case when @local = 1 then c.Name else 'EUR' end as Currency

            , sog.Name as SOG
            , Wg
            , Availability
            , Duration
            , ReactionTime
            , ReactionType
            , ServiceLocation
            , ProActiveSla

            , StdWarranty
            , StdWarrantyLocation

            --Cost

            , case when @local = 1 then AvailabilityFee * costs.ExchangeRate else AvailabilityFee end as AvailabilityFee 
            , case when @local = 1 then TaxAndDutiesW * costs.ExchangeRate else TaxAndDutiesW end as TaxAndDutiesW
            , case when @local = 1 then TaxAndDutiesOow * costs.ExchangeRate else TaxAndDutiesOow end as TaxAndDutiesOow
            , case when @local = 1 then Reinsurance * costs.ExchangeRate else Reinsurance end as Reinsurance
            , case when @local = 1 then ProActive * costs.ExchangeRate else ProActive end as ProActive
            , case when @local = 1 then ServiceSupportCost * costs.ExchangeRate else ServiceSupportCost end as ServiceSupportCost
                                                          
            , case when @local = 1 then MaterialW * costs.ExchangeRate else MaterialW end as MaterialW
            , case when @local = 1 then MaterialOow * costs.ExchangeRate else MaterialOow end as MaterialOow
            , case when @local = 1 then FieldServiceCost * costs.ExchangeRate else FieldServiceCost end as FieldServiceCost
            , case when @local = 1 then Logistic * costs.ExchangeRate else Logistic end as Logistic
            , case when @local = 1 then OtherDirect * costs.ExchangeRate else OtherDirect end as OtherDirect
            , case when @local = 1 then LocalServiceStandardWarranty * costs.ExchangeRate else LocalServiceStandardWarranty end as LocalServiceStandardWarranty
            , case when @local = 1 then Credits * costs.ExchangeRate else Credits end as Credits
            , case when @local = 1 then ServiceTC * costs.ExchangeRate else ServiceTC end as ServiceTC
            , case when @local = 1 then ServiceTP * costs.ExchangeRate else ServiceTP end as ServiceTP
                                                          
            , case when @local = 1 then ServiceTCManual * costs.ExchangeRate else ServiceTCManual end as ServiceTCManual
            , case when @local = 1 then ServiceTPManual * costs.ExchangeRate else ServiceTPManual end as ServiceTPManual
                                                          
            , case when @local = 1 then ServiceTP_Released * costs.ExchangeRate else ServiceTP_Released end as ServiceTP_Released
                                                          
            , case when @local = 1 then ListPrice * costs.ExchangeRate else ListPrice end as ListPrice
            , case when @local = 1 then DealerPrice * costs.ExchangeRate else DealerPrice end as DealerPrice
            , DealerDiscount                               as DealerDiscount
                                                           
            , ChangeUserName + '[' + ChangeUserEmail + ']' as ChangeUser

    from Hardware.GetCosts(@approved, @country, @wg, @availability, @duration, @reactiontime, @reactiontype, @servicelocation, @proactive, -1, -1) costs
    join [References].Currency c on c.Id = costs.CurrencyId
    join InputAtoms.Wg wg on wg.id = costs.WgId
    LEFT JOIN InputAtoms.Sog sog on sog.Id = wg.SogId
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HW-CALC-RESULT');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

declare @money bigint;
select @money = id from Report.ReportColumnType where upper(name) = 'MONEY';

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SOG', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG(Asset)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StdWarranty', 'Standard warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StdWarrantyLocation', 'Standard Warranty Service Location', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'FieldServiceCost', 'Field service cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceSupportCost', 'Service support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'Logistic', 'Logistic cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'AvailabilityFee', 'Availability fee', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'TaxAndDutiesW', 'Tax & Duties iW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'TaxAndDutiesOow', 'Tax & Duties OOW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'MaterialW', 'Material cost iW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'MaterialOow', 'Material cost OOW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ProActive', 'ProActive', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTC', 'Service TC(calc)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTCManual', 'Service TC(manual)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTP', 'Service TP(calc)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTPManual', 'Service TP(manual)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTP_Released', 'Service TP(released)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ListPrice', 'List price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'DealerDiscount', 'Dealer discount in %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'DealerPrice', 'Dealer price', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ChangeUser', 'Change user', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'OtherDirect', 'Other direct cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'LocalServiceStandardWarranty', 'Local service standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'Credits', 'Credits', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
declare @filterTypeId bigint = 0

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'boolean'), 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'boolean'), 'local', 'Local currency');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'country' and MultiSelect=1), 'country', 'Country');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'wg' and MultiSelect=1), 'wg', 'Asset(WG)');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'availability' and MultiSelect=1), 'availability', 'Availability');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'duration' and MultiSelect=1), 'duration', 'Duration');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'reactiontime' and MultiSelect=1), 'reactiontime', 'Reaction time');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'reactiontype' and MultiSelect=1), 'reactiontype', 'Reaction type');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'servicelocation' and MultiSelect=1), 'servicelocation', 'Service location');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'proactive' and MultiSelect=1), 'proactive', 'ProActive');


go


