ALTER FUNCTION [Hardware].[AddMarkup](
    @value float,
    @markupFactor float,
    @markup float
)
RETURNS float
AS
BEGIN

    if @markupFactor is not null
        begin
            set @value = @value * @markupFactor;
        end
    else if @markup is not null
        begin
            set @value = @value + @markup;
        end

    RETURN @value;

END
go

ALTER FUNCTION [Hardware].[MarkupOrFixValue] (
    @value float,
    @markupFactor float,
    @fixed float
)
RETURNS float 
AS
BEGIN

    if @markupFactor is not null
        begin
            return @value * @markupFactor;
        end
    else if @fixed is not null
        begin
            return @fixed;
        end

    RETURN 0;

END
go

alter FUNCTION [Hardware].[GetCosts](
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

                , coalesce(m.AvailabilityFee, 0) as AvailabilityFeeOrZero

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive
       
        from Hardware.GetCalcMember(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
    )
    , CostCte2 as (
        select    m.*

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

                , coalesce(case when m.DurationId = 1 then m.Reinsurance else 0 end, 0) as Reinsurance1
                , coalesce(case when m.DurationId = 2 then m.Reinsurance else 0 end, 0) as Reinsurance2
                , coalesce(case when m.DurationId = 3 then m.Reinsurance else 0 end, 0) as Reinsurance3
                , coalesce(case when m.DurationId = 4 then m.Reinsurance else 0 end, 0) as Reinsurance4
                , coalesce(case when m.DurationId = 5 then m.Reinsurance else 0 end, 0) as Reinsurance5
                , coalesce(case when m.DurationId = 6 then m.Reinsurance else 0 end, 0) as Reinsurance1P

        from CostCte m
    )
    , CostCte3 as (
        select    m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.Reinsurance1 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.Reinsurance2 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.Reinsurance3 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.Reinsurance4 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.Reinsurance5 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.Reinsurance1P + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

        from CostCte2 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance1 + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance2 + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance3 + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance4 + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance5 + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance1P + m.OtherDirect1P + m.AvailabilityFeeOrZero             as ServiceTP1P

        from CostCte3 m
    )
    , CostCte6 as (
        select m.*

                , m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
                , m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
                , m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
                , m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
                , m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
                , m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P

        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.Currency
         , m.ExchangeRate
         , m.SogId
         , m.Sog
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
         , m.TaxAndDutiesW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.TaxOow1, m.TaxOow2, m.TaxOow3, m.TaxOow4, m.TaxOow5, m.TaxOow1P) as TaxAndDutiesOow


         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.MaterialW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.MatOow1, m.MatOow2, m.MatOow3, m.MatOow4, m.MatOow5, m.MatOow1P) as MaterialOow

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
       
         , m.LocalServiceStandardWarranty
         , m.LocalServiceStandardWarrantyManual
       
         , m.Credits

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

         , m.ReleaseDate
         , m.ChangeDate
         , m.ChangeUserName
         , m.ChangeUserEmail

       from CostCte6 m
)
go

alter FUNCTION Hardware.GetReleaseCosts (
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
    with ReinsuranceCte as (
        select r.Wg
             , ta.ReactionTimeId
             , ta.AvailabilityId
             , r.ReinsuranceFlatfee_norm_Approved / er.Value as Cost
        from Hardware.Reinsurance r

        JOIN Dependencies.ReactionTime_Avalability ta on ta.Id = r.ReactionTimeAvailability and ta.IsDisabled = 0

        JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance_Approved

        where r.Duration = (select id from Dependencies.Duration where IsProlongation = 1 and Value = 1)
              and r.DeactivatedDateTime is null
    )
    , CostCte as (
        select    m.*

                , coalesce(m.AvailabilityFee, 0) as AvailabilityFeeOrZero

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive

                , coalesce(r.Cost, 0) as ReinsuranceProlCost
       
        from Hardware.GetCalcMember(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
        left join ReinsuranceCte r on r.Wg = m.WgId and r.ReactionTimeId = m.ReactionTimeId and r.AvailabilityId = m.AvailabilityId
    )
    , CostCte2 as (
        select    m.*

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte3 as (
        select    m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

        from CostCte2 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceProlCost + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceProlCost + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceProlCost + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceProlCost + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceProlCost + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceProlCost + m.OtherDirect1P + m.AvailabilityFeeOrZero             as ServiceTP1P

        from CostCte3 m
    )
    , CostCte6 as (
        select m.*

                , m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
                , m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
                , m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
                , m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
                , m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
                , m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P

        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.CurrencyId
         , m.Currency
         , m.ExchangeRate
         , m.SogId
         , m.Sog
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
         , m.TaxAndDutiesW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.TaxOow1, m.TaxOow2, m.TaxOow3, m.TaxOow4, m.TaxOow5, m.TaxOow1P) as TaxAndDutiesOow


         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.MaterialW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.MatOow1, m.MatOow2, m.MatOow3, m.MatOow4, m.MatOow5, m.MatOow1P) as MaterialOow

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
       
         , m.LocalServiceStandardWarranty
       
         , m.Credits

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

         , m.ReleaseDate
         , m.ChangeUserName
         , m.ChangeUserEmail

       from CostCte6 m
)
go
