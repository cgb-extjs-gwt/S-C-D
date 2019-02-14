ALTER FUNCTION [Hardware].[GetCostsFull](
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

                , m.Year * m.ServiceSupport as ServiceSupportCost

                , m.TravelCost + m.LabourCost as FieldServicePerYearStdw

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

                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw1, m.ServiceSupport, m.LogisticStdw1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw2, m.ServiceSupport, m.LogisticStdw2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw3, m.ServiceSupport, m.LogisticStdw3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw4, m.ServiceSupport, m.LogisticStdw4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw5, m.ServiceSupport, m.LogisticStdw5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5
                , 0     as LocalServiceStandardWarranty1P

        from CostCte2_2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
             , 0 as Credit1P
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceOrZero + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1  as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceOrZero + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2  as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceOrZero + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3  as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceOrZero + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4  as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceOrZero + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5  as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceOrZero + m.OtherDirect1P + m.AvailabilityFeeOrZero - m.Credit1P as ServiceTP1P

        from CostCte4 m
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

         , m.StdWarranty

         --Cost

         , m.AvailabilityFee * m.Year as AvailabilityFee
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.tax1, m.tax2, m.tax3, m.tax4, m.tax5, m.tax1P) as TaxAndDutiesW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.taxO1, m.taxO2, m.taxO3, m.taxO4, m.taxO5, m.taxO1P) as TaxAndDutiesOow
         , m.Reinsurance
         , m.ProActive
         , m.ServiceSupportCost

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.mat1, m.mat2, m.mat3, m.mat4, m.mat5, m.mat1P) as MaterialW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.matO1, m.matO2, m.matO3, m.matO4, m.matO5, m.matO1P) as MaterialOow
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
         
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.LocalServiceStandardWarranty1, m.LocalServiceStandardWarranty2, m.LocalServiceStandardWarranty3, m.LocalServiceStandardWarranty4, m.LocalServiceStandardWarranty5, m.LocalServiceStandardWarranty1P) as LocalServiceStandardWarranty
         
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Credit1, m.Credit2, m.Credit3, m.Credit4, m.Credit5, m.Credit1P) as Credits


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
         , m.ChangeUserName
         , m.ChangeUserEmail

         , m.ServiceTP_Released

         , m.SlaHash

       from CostCte6 m
)
go