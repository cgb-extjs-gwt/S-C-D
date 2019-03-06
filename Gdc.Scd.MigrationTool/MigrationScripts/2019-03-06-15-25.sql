ALTER FUNCTION [Hardware].[GetCostsSlaSog](@approved bit, @sla Portfolio.Sla readonly)
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
             , m.ExchangeRate

             , m.WgId
             , m.Wg
             , wg.Description as WgDescription
             , wg.SogId
             , sog.Name as Sog

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
             , m.Credits

             , ib.InstalledBaseCountry

             , (sum(m.ServiceTC * ib.InstalledBaseCountry)                               over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tc 
             , (sum(case when m.ServiceTC > 0 then ib.InstalledBaseCountry end)          over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tc
             , (sum(m.ServiceTP_Released * ib.InstalledBaseCountry)                      over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp
             , (sum(case when m.ServiceTP_Released > 0 then ib.InstalledBaseCountry end) over(partition by wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp

             , m.ListPrice
             , m.DealerDiscount
             , m.DealerPrice

        from Hardware.GetCostsSla(@approved, @sla) m
        join InputAtoms.Wg wg on wg.id = m.WgId
        join InputAtoms.Sog sog on sog.id = wg.SogId
        left join Hardware.InstallBase ib on ib.Country = m.CountryId and ib.Wg = m.WgId
    )
    select    
            m.Id

            --SLA

            , m.CountryId
            , m.Country
            , m.CurrencyId
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
            , m.Credits

            , case when m.sum_ib_by_tc > 0 then m.sum_ib_x_tc / m.sum_ib_by_tc end as ServiceTcSog
            , case when m.sum_ib_by_tp > 0 then m.sum_ib_x_tp / m.sum_ib_by_tp end as ServiceTpSog
    
            , m.ListPrice
            , m.DealerDiscount
            , m.DealerPrice  

    from cte m
)
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
go

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
    with CostCte as (
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
              , er.Value as ExchangeRate

             --FSP
              , fsp.Name Fsp
              , fsp.ServiceDescription as FspDescription

              --cost blocks

              , fsc.LabourCost_Approved as LabourCost
              , fsc.TravelCost_Approved as TravelCost
              , fst.PerformanceRate_Approved as PerformanceRate
              , fsc.TravelTime_Approved as TravelTime
              , fsc.RepairTime_Approved as RepairTime
              , hr.OnsiteHourlyRates_Approved as OnsiteHourlyRate

              , lc.StandardHandling_Approved as StandardHandling

              , lc.StandardHandling_Approved + 
                lc.HighAvailabilityHandling_Approved + 
                lc.StandardDelivery_Approved + 
                lc.ExpressDelivery_Approved + 
                lc.TaxiCourierDelivery_Approved + 
                lc.ReturnDeliveryFactory_Approved as LogisticPerYear

              , case when afEx.id is not null then af.Fee_Approved else 0 end as AvailabilityFee
      
              , tax.TaxAndDuties_norm_Approved as TaxAndDutiesW

              , moc.Markup_Approved       as MarkupOtherCost
              , moc.MarkupFactor_Approved as MarkupFactorOtherCost

              , msw.MarkupFactorStandardWarranty_Approved as MarkupFactorStandardWarranty
              , msw.MarkupStandardWarranty_Approved       as MarkupStandardWarranty
      
              , afr.AFR1_Approved  as AFR1
              , afr.AFR2_Approved  as AFR2
              , afr.AFR3_Approved  as AFR3
              , afr.AFR4_Approved  as AFR4
              , afr.AFR5_Approved  as AFR5
              , afr.AFRP1_Approved as AFRP1

              , Hardware.CalcFieldServiceCost(
                            fst.TimeAndMaterialShare_norm_Approved, 
                            fsc.TravelCost_Approved, 
                            fsc.LabourCost_Approved, 
                            fst.PerformanceRate_Approved, 
                            fsc.TravelTime_Approved, 
                            fsc.RepairTime_Approved, 
                            hr.OnsiteHourlyRates_Approved, 
                            1
                        ) as FieldServicePerYear

              , ssc.[1stLevelSupportCosts_Approved]           as [1stLevelSupportCosts]
              , ssc.[2ndLevelSupportCosts_Approved]           as [2ndLevelSupportCosts]
           
              , r.ReinsuranceFlatfee1_Approved                as ReinsuranceFlatfee1
              , r.ReinsuranceFlatfee2_Approved                as ReinsuranceFlatfee2
              , r.ReinsuranceFlatfee3_Approved                as ReinsuranceFlatfee3
              , r.ReinsuranceFlatfee4_Approved                as ReinsuranceFlatfee4
              , r.ReinsuranceFlatfee5_Approved                as ReinsuranceFlatfee5
              , r.ReinsuranceFlatfeeP1_Approved               as ReinsuranceFlatfeeP1
              , r.ReinsuranceUpliftFactor_4h_24x7_Approved    as ReinsuranceUpliftFactor_4h_24x7
              , r.ReinsuranceUpliftFactor_4h_9x5_Approved     as ReinsuranceUpliftFactor_4h_9x5
              , r.ReinsuranceUpliftFactor_NBD_9x5_Approved    as ReinsuranceUpliftFactor_NBD_9x5

              , mcw.MaterialCostWarranty_Approved as MaterialCostWarranty
              , mco.MaterialCostOow_Approved as MaterialCostOow

              , dur.Value as Duration
              , dur.IsProlongation

        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        INNER JOIN [References].Currency cur on cur.Id = c.CurrencyId

        INNER JOIN [References].ExchangeRate er on er.CurrencyId = cur.Id

        INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        INNER JOIN InputAtoms.WgView wg2 on wg2.Id = m.WgId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId and hr.Country = m.CountryId

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId
        LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

        LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTimeType = m.ReactionTime_ReactionType

        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN Hardware.ReinsuranceYear r on r.Wg = m.WgId

        LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability

        LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Wg = m.WgId AND msw.Country = m.CountryId 

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

              , m.StandardHandling as StandardHandling

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
            
              , Hardware.CalcByDur(
                       m.Duration
                     , m.IsProlongation 
                     , m.LogisticPerYear * m.AFR1 
                     , m.LogisticPerYear * m.AFR2 
                     , m.LogisticPerYear * m.AFR3 
                     , m.LogisticPerYear * m.AFR4 
                     , m.LogisticPerYear * m.AFR5 
                     , m.LogisticPerYear * m.AFRP1
                 ) as LogisticTransportcost

            , m.Currency
    from CostCte m
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
    with CostCte as (
            select 
                m.Id
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

             --FSP
              , fsp.Name Fsp
              , fsp.ServiceDescription as FspDescription

              --cost blocks

              , fsc.LabourCost as LabourCost
              , fsc.TravelCost as TravelCost
              , fst.PerformanceRate as PerformanceRate
              , fsc.TravelTime as TravelTime
              , fsc.RepairTime as RepairTime
              , hr.OnsiteHourlyRates as OnsiteHourlyRate

              , lc.StandardHandling as StandardHandling

              , (lc.StandardHandling + 
                lc.HighAvailabilityHandling + 
                lc.StandardDelivery + 
                lc.ExpressDelivery + 
                lc.TaxiCourierDelivery + 
                lc.ReturnDeliveryFactory) as LogisticPerYear

              , case when afEx.id is not null then af.Fee * er.Value else 0 end as AvailabilityFee
      
              , tax.TaxAndDuties_norm * er.Value  as TaxAndDutiesW

              , moc.Markup                   as MarkupOtherCost
              , moc.MarkupFactor             as MarkupFactorOtherCost

              , msw.MarkupFactorStandardWarranty             as MarkupFactorStandardWarranty
              , msw.MarkupStandardWarranty                   as MarkupStandardWarranty
      
              , afr.AFR1  as AFR1
              , afr.AFR2  as AFR2
              , afr.AFR3  as AFR3
              , afr.AFR4  as AFR4
              , afr.AFR5  as AFR5
              , afr.AFRP1 as AFRP1

              , Hardware.CalcFieldServiceCost(
                            fst.TimeAndMaterialShare_norm, 
                            fsc.TravelCost, 
                            fsc.LabourCost, 
                            fst.PerformanceRate, 
                            fsc.TravelTime, 
                            fsc.RepairTime, 
                            hr.OnsiteHourlyRates, 
                            1
                        ) as FieldServicePerYear

              , ssc.[1stLevelSupportCosts] * er.Value            as [1stLevelSupportCosts]
              , ssc.[2ndLevelSupportCosts] * er.Value            as [2ndLevelSupportCosts]
           
              , r.ReinsuranceFlatfee1 * er.Value                 as ReinsuranceFlatfee1
              , r.ReinsuranceFlatfee2 * er.Value                 as ReinsuranceFlatfee2
              , r.ReinsuranceFlatfee3 * er.Value                 as ReinsuranceFlatfee3
              , r.ReinsuranceFlatfee4 * er.Value                 as ReinsuranceFlatfee4
              , r.ReinsuranceFlatfee5 * er.Value                 as ReinsuranceFlatfee5
              , r.ReinsuranceFlatfeeP1 * er.Value                as ReinsuranceFlatfeeP1

              , r.ReinsuranceUpliftFactor_4h_24x7     as ReinsuranceUpliftFactor_4h_24x7
              , r.ReinsuranceUpliftFactor_4h_9x5      as ReinsuranceUpliftFactor_4h_9x5
              , r.ReinsuranceUpliftFactor_NBD_9x5     as ReinsuranceUpliftFactor_NBD_9x5

              , mcw.MaterialCostWarranty * er.Value  as MaterialCostWarranty
              , mco.MaterialCostOow * er.Value       as MaterialCostOow
              , cur.Name as Currency

              , dur.Value as Duration
              , dur.IsProlongation

        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        INNER JOIN [References].Currency cur on cur.Id = c.CurrencyId

        INNER JOIN [References].ExchangeRate er on er.CurrencyId = cur.Id

        INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        INNER JOIN InputAtoms.WgView wg2 on wg2.Id = m.WgId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.RoleCode = wg.RoleCodeId and hr.Country = m.CountryId

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId
        LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

        LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTimeType = m.ReactionTime_ReactionType

        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN Hardware.ReinsuranceYear r on r.Wg = m.WgId

        LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability

        LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Wg = m.WgId AND msw.Country = m.CountryId 

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

              , m.StandardHandling

              , m.AvailabilityFee
      
              , m.TaxAndDutiesW

              , m.MarkupOtherCost
              , m.MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty
      
              , m.AFR1   * 100 as AFR1
              , m.AFR2   * 100 as AFR2
              , m.AFR3   * 100 as AFR3
              , m.AFR4   * 100 as AFR4
              , m.AFR5   * 100 as AFR5
              , m.AFRP1  * 100 as AFRP1

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

             , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
             , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
             , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
             , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
             , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
            
             , Hardware.CalcByDur(
                       m.Duration
                     , m.IsProlongation 
                     , m.LogisticPerYear * m.AFR1 
                     , m.LogisticPerYear * m.AFR2 
                     , m.LogisticPerYear * m.AFR3 
                     , m.LogisticPerYear * m.AFR4 
                     , m.LogisticPerYear * m.AFR5 
                     , m.LogisticPerYear * m.AFRP1
                 ) as LogisticTransportcost

    from CostCte m
)
go

ALTER FUNCTION [Report].[CalcOutputVsFREEZE]
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    with cte as (
        SELECT    m.Id

                --FSP
                , fsp.Name Fsp
                , fsp.ServiceDescription as FspDescription
        
                , wg.SogDescription as SogDescription
                , wg.Description as WgDescription
                , wg.Name as Wg
        
            --SLA
                , er.Value as ExchangeRate
                , c.Name as Country
                , dur.Name as Duration
                , dur.Value as Year
                , dur.IsProlongation
                , av.Name as Availability
                , rtime.Name as ReactionTime
                , loc.Name as ServiceLocation
                , prosla.ExternalName  as ProActiveSla

                , stdw.DurationValue as StdWarranty

                , afr.AFR1 , AFR1_Approved
                , afr.AFR2, AFR2_Approved       
                , afr.AFR3, afr.AFR3_Approved   
                , afr.AFR4, afr.AFR4_Approved   
                , afr.AFR5, afr.AFR5_Approved   
                , afr.AFRP1, afr.AFRP1_Approved

                , mcw.MaterialCostWarranty, mcw.MaterialCostWarranty_Approved

                , coalesce(tax.TaxAndDuties_norm, 0) as TaxAndDuties, coalesce(tax.TaxAndDuties_norm_Approved, 0) as TaxAndDuties_Approved

                , fsc.TravelCost + fsc.LabourCost + coalesce(fst.PerformanceRate, 0) / er.Value as FieldServicePerYearStdw
                , fsc.TravelCost_Approved + fsc.LabourCost_Approved + coalesce(fst.PerformanceRate_Approved, 0) / er.Value  as FieldServicePerYearStdw_Approved

                , ssc.ServiceSupport         , ssc.ServiceSupport_Approved

                , (lc.StandardHandling + lc.HighAvailabilityHandling + lc.StandardDelivery + lc.ExpressDelivery + lc.TaxiCourierDelivery + lc.ReturnDeliveryFactory) / er.Value as LogisticPerYearStdw
                , (lc.StandardHandling_Approved + lc.HighAvailabilityHandling_Approved + lc.StandardDelivery_Approved + lc.ExpressDelivery_Approved + lc.TaxiCourierDelivery_Approved + lc.ReturnDeliveryFactory_Approved) / er.Value as LogisticPerYearStdw_Approved

                , coalesce(case when afEx.Id is not null then af.Fee end, 0)          as AvailabilityFee
                , coalesce(case when afEx.Id is not null then af.Fee_Approved end, 0) as AvailabilityFee_Approved

                , msw.MarkupFactorStandardWarranty_norm AS MarkupFactorStandardWarranty, msw.MarkupFactorStandardWarranty_norm_Approved AS MarkupFactorStandardWarranty_Approved  
                , msw.MarkupStandardWarranty / er.Value  AS MarkupStandardWarranty, msw.MarkupStandardWarranty_Approved / er.Value AS MarkupStandardWarranty_Approved

        FROM Portfolio.GetBySlaSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN InputAtoms.Country c on c.id = m.CountryId

        INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        INNER JOIN InputAtoms.WgView wg2 on wg2.id = m.WgId

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId 

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = stdw.Country AND fsc.Wg = stdw.Wg AND fsc.ServiceLocation = stdw.ServiceLocationId
        LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = stdw.Country AND fst.Wg = stdw.Wg AND fst.ReactionTimeType = stdw.ReactionTime_ReactionType

        LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = stdw.Country AND lc.Wg = stdw.Wg AND lc.ReactionTimeType = stdw.ReactionTime_ReactionType

        LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Wg = m.WgId AND msw.Country = m.CountryId

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    , CostCte as (
        select    m.*

                , m.TaxAndDuties * m.MaterialCostWarranty * m.AFR1 as tax1
                , m.TaxAndDuties * m.MaterialCostWarranty * m.AFR2 as tax2
                , m.TaxAndDuties * m.MaterialCostWarranty * m.AFR3 as tax3
                , m.TaxAndDuties * m.MaterialCostWarranty * m.AFR4 as tax4
                , m.TaxAndDuties * m.MaterialCostWarranty * m.AFR5 as tax5

                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR1_Approved as tax1_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR2_Approved as tax2_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR3_Approved as tax3_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR4_Approved as tax4_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR5_Approved as tax5_Approved

                , m.FieldServicePerYearStdw * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYearStdw * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYearStdw * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYearStdw * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYearStdw * m.AFR5  as FieldServiceCost5

                , m.FieldServicePerYearStdw * m.AFR1  as FieldServiceCost1_Approved
                , m.FieldServicePerYearStdw * m.AFR2  as FieldServiceCost2_Approved
                , m.FieldServicePerYearStdw * m.AFR3  as FieldServiceCost3_Approved
                , m.FieldServicePerYearStdw * m.AFR4  as FieldServiceCost4_Approved
                , m.FieldServicePerYearStdw * m.AFR5  as FieldServiceCost5_Approved

                , m.LogisticPerYearStdw * m.AFR1  as Logistic1
                , m.LogisticPerYearStdw * m.AFR2  as Logistic2
                , m.LogisticPerYearStdw * m.AFR3  as Logistic3
                , m.LogisticPerYearStdw * m.AFR4  as Logistic4
                , m.LogisticPerYearStdw * m.AFR5  as Logistic5

                , m.LogisticPerYearStdw_Approved * m.AFR1_Approved   as Logistic1_Approved
                , m.LogisticPerYearStdw_Approved * m.AFR2_Approved   as Logistic2_Approved
                , m.LogisticPerYearStdw_Approved * m.AFR3_Approved   as Logistic3_Approved
                , m.LogisticPerYearStdw_Approved * m.AFR4_Approved   as Logistic4_Approved
                , m.LogisticPerYearStdw_Approved * m.AFR5_Approved   as Logistic5_Approved

        from cte m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost1, m.ServiceSupport, m.Logistic1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost2, m.ServiceSupport, m.Logistic2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost3, m.ServiceSupport, m.Logistic3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost4, m.ServiceSupport, m.Logistic4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost5, m.ServiceSupport, m.Logistic5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost1_Approved, m.ServiceSupport_Approved, m.Logistic1_Approved, m.tax1_Approved, m.AFR1_Approved, 1 + m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
                        else 0 
                    end as LocalServiceStandardWarranty1_Approved
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost2_Approved, m.ServiceSupport_Approved, m.Logistic2_Approved, m.tax2_Approved, m.AFR2_Approved, 1 + m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
                        else 0 
                    end as LocalServiceStandardWarranty2_Approved
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost3_Approved, m.ServiceSupport_Approved, m.Logistic3_Approved, m.tax3_Approved, m.AFR3_Approved, 1 + m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
                        else 0 
                    end as LocalServiceStandardWarranty3_Approved
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost4_Approved, m.ServiceSupport_Approved, m.Logistic4_Approved, m.tax4_Approved, m.AFR4_Approved, 1 + m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
                        else 0 
                    end as LocalServiceStandardWarranty4_Approved
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCost5_Approved, m.ServiceSupport_Approved, m.Logistic5_Approved, m.tax5_Approved, m.AFR5_Approved, 1 + m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved)
                        else 0 
                    end as LocalServiceStandardWarranty5_Approved

        from CostCte m
    )
    select    m.Id
            , m.Country
            , m.SogDescription as SogDescription
            , m.Fsp
            , m.Wg
            , m.WgDescription
            , m.ServiceLocation
            , m.ReactionTime
            , m.ProActiveSla
         
            , (m.Duration + ' ' + m.ServiceLocation) as ServiceProduct
         
            , (m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5) * m.ExchangeRate as StandardWarranty
            , (m.LocalServiceStandardWarranty1_Approved + m.LocalServiceStandardWarranty2_Approved + m.LocalServiceStandardWarranty3_Approved + m.LocalServiceStandardWarranty4_Approved + m.LocalServiceStandardWarranty5_Approved) * m.ExchangeRate  as StandardWarranty_Approved
            , cur.Name as Currency
    from CostCte2 m
    join InputAtoms.Country cnt on cnt.id = @cnt
    join [References].Currency cur on cur.Id = cnt.CurrencyId
)
go
