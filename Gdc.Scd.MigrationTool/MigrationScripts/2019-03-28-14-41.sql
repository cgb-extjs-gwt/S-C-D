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
            , std.Country
            , std.CurrencyId
            , std.Currency
            , std.ExchangeRate
            , m.WgId
            , std.Wg
            , std.SogId
            , std.Sog
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

            , std.StdWarranty
            , std.StdWarrantyLocation

            --Cost values

            , std.AFR1  
            , std.AFR2  
            , std.AFR3  
            , std.AFR4  
            , std.AFR5  
            , std.AFRP1 

            , std.MatCost1
            , std.MatCost2
            , std.MatCost3
            , std.MatCost4
            , std.MatCost5
            , std.MatCost1P
            , std.MaterialW
            , std.MaterialOow

            , std.TaxAndDuties1
            , std.TaxAndDuties2
            , std.TaxAndDuties3
            , std.TaxAndDuties4
            , std.TaxAndDuties5
            , std.TaxAndDuties1P
            , std.TaxAndDutiesW
            , std.TaxAndDutiesOow

            , r.Cost as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end / std.ExchangeRate as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end / std.ExchangeRate as TravelCost             
            , case when @approved = 0 then fst.PerformanceRate                else fst.PerformanceRate_Approved            end / std.ExchangeRate as PerformanceRate        
            , case when @approved = 0 then fst.TimeAndMaterialShare_norm      else fst.TimeAndMaterialShare_norm_Approved  end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             

            , std.OnsiteHourlyRates      
                       
            --##### SERVICE SUPPORT COST #########                                                                                               
            , std.ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end / std.ExchangeRate as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end / std.ExchangeRate as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end / std.ExchangeRate as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end / std.ExchangeRate as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end / std.ExchangeRate as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end / std.ExchangeRate as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee

            , case when @approved = 0 then moc.Markup                              else moc.Markup_Approved                            end / std.ExchangeRate as MarkupOtherCost                      
            , case when @approved = 0 then moc.MarkupFactor_norm                   else moc.MarkupFactor_norm_Approved                 end as MarkupFactorOtherCost                

            --####### PROACTIVE COST ###################
            , std.LocalRemoteAccessSetup
            , std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                 as LocalRegularUpdate
            , std.LocalPreparation * proSla.LocalPreparationShcRepetition                       as LocalPreparation
            , std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition as LocalRemoteCustomerBriefing
            , std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition as LocalOnsiteCustomerBriefing
            , std.Travel * proSla.TravellingTimeRepetition                                      as Travel
            , std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition           as CentralExecutionReport

            , std.LocalServiceStandardWarranty
            , std.Credit1
            , std.Credit2
            , std.Credit3
            , std.Credit4
            , std.Credit5
            , std.Credits

            --########## MANUAL COSTS ################
            , man.ListPrice          / std.ExchangeRate as ListPrice                   
            , man.DealerDiscount                        as DealerDiscount              
            , man.DealerPrice        / std.ExchangeRate as DealerPrice                 
            , man.ServiceTC          / std.ExchangeRate as ServiceTCManual                   
            , man.ServiceTP          / std.ExchangeRate as ServiceTPManual                   
            , man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  
            , u.Name                                    as ChangeUserName
            , u.Email                                   as ChangeUserEmail

    FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    INNER JOIN Hardware.CalcStdw(@approved, @cnt, @wg) std on std.CountryId = m.CountryId and std.WgId = m.WgId

    LEFT JOIN Hardware.GetReinsurance(@approved) r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.DeactivatedDateTime is null

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.DeactivatedDateTime is null

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId
)
GO

ALTER FUNCTION [Hardware].[GetCostsSlaSog](
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

        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, null, null) m
        join InputAtoms.Wg wg on wg.id = m.WgId and wg.DeactivatedDateTime is null
        left join Hardware.InstallBase ib on ib.Country = m.CountryId and ib.Wg = m.WgId and ib.DeactivatedDateTime is null
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