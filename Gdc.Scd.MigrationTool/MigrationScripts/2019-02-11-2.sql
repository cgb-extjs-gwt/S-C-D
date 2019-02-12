USE [SCD_2_3]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION [Portfolio].[GetBySlaPaging](
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
RETURNS @tbl TABLE 
            (   
                [rownum] [int] NOT NULL,
                [Id] [bigint] NOT NULL,
                [CountryId] [bigint] NOT NULL,
                [WgId] [bigint] NOT NULL,
                [AvailabilityId] [bigint] NOT NULL,
                [DurationId] [bigint] NOT NULL,
                [ReactionTimeId] [bigint] NOT NULL,
                [ReactionTypeId] [bigint] NOT NULL,
                [ServiceLocationId] [bigint] NOT NULL,
                [ProActiveSlaId] [bigint] NOT NULL,
                [SlaHash] [int] NOT NULL
            )
AS
BEGIN
	declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
    declare @isEmptyWG    bit = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv    bit = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur   bit = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@reactiontype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@reactiontime);
    declare @isEmptyLoc   bit = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro   bit = Portfolio.IsListEmpty(@pro);

    if @limit > 0
        begin
            with SlaCte as (
                select ROW_NUMBER() over(
                            order by m.CountryId
                                   , m.WgId
                                   , m.AvailabilityId
                                   , m.DurationId
                                   , m.ReactionTimeId
                                   , m.ReactionTypeId
                                   , m.ServiceLocationId
                                   , m.ProActiveSlaId
                        ) as rownum
                     , m.*
                    from Portfolio.LocalPortfolio m
					where   (@isEmptyCnt = 1 or CountryId in (select id from @cnt))
							AND (@isEmptyWG = 1 or WgId in (select id from @wg))
							AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
							AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
							AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @reactiontime))
							AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @reactiontype))
							AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
							AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))
            )
            insert @tbl
            select top(@limit)
                    rownum, Id, CountryId, WgId, AvailabilityId, DurationId, ReactionTimeId, ReactionTypeId, ServiceLocationId, ProActiveSlaId, SlaHash
            from SlaCte where rownum > @lastid
        end
    else
        begin
            insert @tbl
            select -1 as rownum, Id, CountryId, WgId, AvailabilityId, DurationId, ReactionTimeId, ReactionTypeId, ServiceLocationId, ProActiveSlaId, SlaHash
            from Portfolio.LocalPortfolio m
			where   (@isEmptyCnt = 1 or CountryId in (select id from @cnt))
							AND (@isEmptyWG = 1 or WgId in (select id from @wg))
							AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
							AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
							AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @reactiontime))
							AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @reactiontype))
							AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
							AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))

             order by m.CountryId
                    , m.WgId
                    , m.AvailabilityId
                    , m.DurationId
                    , m.ReactionTimeId
                    , m.ReactionTypeId
                    , m.ServiceLocationId
                    , m.ProActiveSlaId;

        end

    RETURN;
END;
GO

ALTER FUNCTION [Hardware].[GetCalcMember] (
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
    with Cte as (
        SELECT 
                   m.*

                 , case when stdw.DurationValue is not null then stdw.DurationValue 
                        when stdw2.DurationValue is not null then stdw2.DurationValue 
                    end as StdWarranty
                 , case when stdw.DurationValue is not null then stdw.ReactionTimeId 
                        when stdw2.DurationValue is not null then stdw2.ReactionTimeId  
                    end as StdwReactionTimeId 
                 , case when stdw.DurationValue is not null then stdw.ReactionTypeId 
                        when stdw2.DurationValue is not null then stdw2.ReactionTypeId 
                    end as StdwReactionTypeId

        FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId --find local standard warranty portfolio
        LEFT JOIN Fsp.HwStandardWarrantyView stdw2 on stdw2.Wg = m.WgId and stdw2.Country is null    --find principle standard warranty portfolio, if local does not exist

    )
    SELECT m.Id

        --SLA

         , m.CountryId          
         , c.Name               as Country
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

         , m.StdWarranty

         , case when @approved = 0 then afr.AFR1  else AFR1_Approved       end as AFR1 
         , case when @approved = 0 then afr.AFR2  else AFR2_Approved       end as AFR2 
         , case when @approved = 0 then afr.AFR3  else afr.AFR3_Approved   end as AFR3 
         , case when @approved = 0 then afr.AFR4  else afr.AFR4_Approved   end as AFR4 
         , case when @approved = 0 then afr.AFR5  else afr.AFR5_Approved   end as AFR5 
         , case when @approved = 0 then afr.AFRP1 else afr.AFRP1_Approved  end as AFRP1
       
         , case when @approved = 0 then hdd.HddRet                         else hdd.HddRet_Approved                  end as HddRet              
         
         , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved    end as MaterialCostWarranty
         , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved         end as MaterialCostOow     

         , case when @approved = 0 then tax.TaxAndDuties                   else tax.TaxAndDuties_Approved            end as TaxAndDuties

         , case when @approved = 0 then r.Cost                             else r.Cost_Approved                      end as Reinsurance
         , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved              end as LabourCost             
         , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved              end as TravelCost             
         , case when @approved = 0 then fsc.TimeAndMaterialShare           else fsc.TimeAndMaterialShare_Approved    end as TimeAndMaterialShare   
         , case when @approved = 0 then fsc.PerformanceRate                else fsc.PerformanceRate_Approved         end as PerformanceRate        
         , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved              end as TravelTime             
         , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved              end as RepairTime             
         , case when @approved = 0 then fsc.OnsiteHourlyRates              else fsc.OnsiteHourlyRates_Approved       end as OnsiteHourlyRates      
                  
         , case when @approved = 0 then ssc.[1stLevelSupportCosts]         else ssc.[1stLevelSupportCosts_Approved]  end as [1stLevelSupportCosts] 
         , case when @approved = 0 then ssc.[2ndLevelSupportCosts]         else ssc.[2ndLevelSupportCosts_Approved]  end as [2ndLevelSupportCosts] 

         , case when @approved = 0 then ssc.ServiceSupport                 else ssc.ServiceSupport_Approved          end as ServiceSupport
         
         , case when @approved = 0 then lcs.ExpressDelivery                 else lcs.ExpressDelivery_Approved          end as StdExpressDelivery         
         , case when @approved = 0 then lcs.HighAvailabilityHandling        else lcs.HighAvailabilityHandling_Approved end as StdHighAvailabilityHandling
         , case when @approved = 0 then lcs.StandardDelivery                else lcs.StandardDelivery_Approved         end as StdStandardDelivery        
         , case when @approved = 0 then lcs.StandardHandling                else lcs.StandardHandling_Approved         end as StdStandardHandling        
         , case when @approved = 0 then lcs.ReturnDeliveryFactory           else lcs.ReturnDeliveryFactory_Approved    end as StdReturnDeliveryFactory   
         , case when @approved = 0 then lcs.TaxiCourierDelivery             else lcs.TaxiCourierDelivery_Approved      end as StdTaxiCourierDelivery     

         , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved          end as ExpressDelivery         
         , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling
         , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved         end as StandardDelivery        
         , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved         end as StandardHandling        
         , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved    end as ReturnDeliveryFactory   
         , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved      end as TaxiCourierDelivery     

         , case when afEx.id is null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                 else 0
           end as AvailabilityFee

         , case when @approved = 0 then moc.Markup                         else moc.Markup_Approved                       end as MarkupOtherCost                      
         , case when @approved = 0 then moc.MarkupFactor                   else moc.MarkupFactor_Approved                 end as MarkupFactorOtherCost                

         , case when @approved = 0 then msw.MarkupFactorStandardWarranty   else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty
         , case when @approved = 0 then msw.MarkupStandardWarranty         else msw.MarkupStandardWarranty_Approved       end as MarkupStandardWarranty      

         , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate
                else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved
            end as LocalRemoteAccessSetup

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

         , man.ListPrice       as ListPrice                   
         , man.DealerDiscount  as DealerDiscount              
         , man.DealerPrice     as DealerPrice                 
         , man.ServiceTC       as ServiceTCManual                   
         , man.ServiceTP       as ServiceTPManual                   
         , man.ServiceTP_Released as ServiceTP_Released                  
         , man.ChangeUserName  as ChangeUserName
         , man.ChangeUserEmail as ChangeUserEmail

         , m.SlaHash

    FROM Cte m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.HddRetention hdd on hdd.Wg = m.WgId AND hdd.Year = m.DurationId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Hardware.LogisticsCostView lcs on lcs.Country = m.CountryId AND lcs.Wg = m.WgId AND lcs.ReactionTime = m.StdwReactionTimeId AND lcs.ReactionType = m.StdwReactionTypeId

    LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeId = m.ReactionTimeId AND moc.ReactionTypeId = m.ReactionTypeId AND moc.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCostView man on man.PortfolioId = m.Id
)
GO

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
                , m.matO1P * m.AFRP1 as taxO1P

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
         , m.HddRet
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
    select Id

         , Country
         , Wg
         , Availability
         , Duration
         , ReactionTime
         , ReactionType
         , ServiceLocation
         , ProActiveSla

         , StdWarranty

         , AvailabilityFee
         , HddRet
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

         , ListPrice
         , DealerDiscount
         , DealerPrice
         , ServiceTCManual
         , ServiceTPManual
         , ChangeUserName
         , ChangeUserEmail

         ,ServiceTP_Released

    from Hardware.GetCostsFull(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit)
)
GO

ALTER PROCEDURE [Hardware].[SpGetCosts]
     @approved bit,
    @local bit,
 	@cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

	declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
    declare @isEmptyWG    bit = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv    bit = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur   bit = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@reactiontype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@reactiontime);
    declare @isEmptyLoc   bit = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro   bit = Portfolio.IsListEmpty(@pro);

    select @total = COUNT(id)
    from Portfolio.LocalPortfolio m
   where   (@isEmptyCnt = 1 or CountryId in (select id from @cnt))
		AND (@isEmptyWG = 1 or WgId in (select id from @wg))
		AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
		AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
		AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @reactiontime))
		AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @reactiontype))
		AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
		AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))


    declare @cur nvarchar(max);
    declare @exchange float;

    if @local = 1
    begin
    
        --convert values from EUR to local

        select costs.Id

             , Country
             , cur.Name as Currency
             , er.Value as ExchangeRate

             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty

             --Cost

             , AvailabilityFee               * er.Value  as AvailabilityFee 
             , HddRet                        * er.Value  as HddRet
             , TaxAndDutiesW                 * er.Value  as TaxAndDutiesW
             , TaxAndDutiesOow               * er.Value  as TaxAndDutiesOow
             , Reinsurance                   * er.Value  as Reinsurance
             , ProActive                     * er.Value  as ProActive
             , ServiceSupportCost            * er.Value  as ServiceSupportCost

             , MaterialW                     * er.Value  as MaterialW
             , MaterialOow                   * er.Value  as MaterialOow
             , FieldServiceCost              * er.Value  as FieldServiceCost
             , Logistic                      * er.Value  as Logistic
             , OtherDirect                   * er.Value  as OtherDirect
             , LocalServiceStandardWarranty  * er.Value  as LocalServiceStandardWarranty
             , Credits                       * er.Value  as Credits
             , ServiceTC                     * er.Value  as ServiceTC
             , ServiceTP                     * er.Value  as ServiceTP

             , ServiceTCManual               * er.Value  as ServiceTCManual
             , ServiceTPManual               * er.Value  as ServiceTPManual

             , ServiceTP_Released            * er.Value as ServiceTP_Released

             , ListPrice                     * er.Value  as ListPrice
             , DealerPrice                   * er.Value  as DealerPrice
             , DealerDiscount                             as DealerDiscount

             , ChangeUserName                             as ChangeUserName
             , ChangeUserEmail                            as ChangeUserEmail

        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) costs
		join [InputAtoms].Country c on c.Name = costs.Country
		join [References].Currency cur on cur.Id = c.CurrencyId
		join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        order by Id 
        
    end
    else
    begin

        select  cur.Name as Currency
             , er.Value as ExchangeRate, 
			 costs.*
        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) costs
		join [InputAtoms].Country c on c.Name = costs.Country
		join [References].Currency cur on cur.Id = c.CurrencyId
		join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        order by Id
    end
END
GO

ALTER FUNCTION [SoftwareSolution].[GetSwSpMaintenancePaging] (
    @approved bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS @tbl TABLE 
        (   
            [rownum] [int] NOT NULL,
            [Id] [bigint] NOT NULL,
            [Pla] [bigint] NOT NULL,
            [Sfab] [bigint] NOT NULL,
            [Sog] [bigint] NOT NULL,
            [SwDigit] [bigint] NOT NULL,
            [Availability] [bigint] NOT NULL,
            [Year] [bigint] NOT NULL,
            [2ndLevelSupportCosts] [float] NULL,
            [InstalledBaseSog] [float] NULL,
            [ReinsuranceFlatfee] [float] NULL,
            [CurrencyReinsurance] [bigint] NULL,
            [RecommendedSwSpMaintenanceListPrice] [float] NULL,
            [MarkupForProductMarginSwLicenseListPrice] [float] NULL,
            [ShareSwSpMaintenanceListPrice] [float] NULL,
            [DiscountDealerPrice] [float] NULL
        )
AS
BEGIN
		declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
		declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
		declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

        if @limit > 0
        begin
            with cte as (
                select ROW_NUMBER() over(
                            order by ssm.SwDigit
                                   , ya.AvailabilityId
                                   , ya.YearId
                        ) as rownum
                      , ssm.*
                      , ya.AvailabilityId
                      , ya.YearId
                FROM SoftwareSolution.SwSpMaintenance ssm
                JOIN Dependencies.Duration_Availability ya on ya.Id = ssm.DurationAvailability
                WHERE (@isEmptyDigit = 1 or ssm.SwDigit in (select id from @digit))
					AND (@isEmptyAV = 1 or ya.AvailabilityId in (select id from @av))
					AND (@isEmptyYear = 1 or ya.YearId in (select id from @year))
            )
            insert @tbl
            select top(@limit)
                    rownum
                  , ssm.Id
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Sog
                  , ssm.SwDigit
                  , ssm.AvailabilityId
                  , ssm.YearId
              
                  , case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
                  , case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
                  , case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
                  , case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end
                  , case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
                  , case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

            from cte ssm where rownum > @lastid
        end
    else
        begin
            insert @tbl
            select -1 as rownum
                  , ssm.Id
                  , ssm.Pla
                  , ssm.Sfab
                  , ssm.Sog
                  , ssm.SwDigit
                  , ya.AvailabilityId
                  , ya.YearId

                  , case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
                  , case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
                  , case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
                  , case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end
                  , case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
                  , case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end
                  , case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

            FROM SoftwareSolution.SwSpMaintenance ssm
            JOIN Dependencies.Duration_Availability ya on ya.Id = ssm.DurationAvailability

            WHERE (@isEmptyDigit = 1 or ssm.SwDigit in (select id from @digit))
					AND (@isEmptyAV = 1 or ya.AvailabilityId in (select id from @av))
					AND (@isEmptyYear = 1 or ya.YearId in (select id from @year))

        end

    RETURN;
END
GO

ALTER FUNCTION [SoftwareSolution].[GetCosts] (
    @approved bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
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

ALTER PROCEDURE [SoftwareSolution].[SpGetCosts]
    @approved bit,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

	declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
	declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
	declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

    SELECT @total = COUNT(m.id)

        FROM SoftwareSolution.SwSpMaintenance m 
        JOIN Dependencies.Duration_Availability yav on yav.Id = m.DurationAvailability

		WHERE (@isEmptyDigit = 1 or m.SwDigit in (select id from @digit))
			AND (@isEmptyAV = 1 or yav.AvailabilityId in (select id from @av))
			AND (@isEmptyYear = 1 or yav.YearId in (select id from @year))

    select  m.rownum
          , m.Id
          , d.Name as SwDigit
          , sog.Name as Sog
          , av.Name as Availability 
          , y.Name as Year
          , m.[1stLevelSupportCosts]
          , m.[2ndLevelSupportCosts]
          , m.InstalledBaseCountry
          , m.InstalledBaseSog
          , m.Reinsurance
          , m.ServiceSupport
          , m.TransferPrice
          , m.MaintenanceListPrice
          , m.DealerPrice
          , m.DiscountDealerPrice
    from SoftwareSolution.GetCosts(@approved, @digit, @av, @year, @lastid, @limit) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Year y on y.Id = m.Year

    order by m.SwDigit, m.Availability, m.Year

END
GO

ALTER FUNCTION [SoftwareSolution].[GetProActivePaging] (
     @approved bit,
     @cnt dbo.ListID readonly,
     @digit dbo.ListID readonly,
     @av dbo.ListID readonly,
     @year dbo.ListID readonly,
     @lastid bigint,
     @limit int
)
RETURNS @tbl TABLE 
        (   
            rownum                                  int NOT NULL,
            Id                                      bigint,
            Country                                 bigint,
            Pla                                     bigint,
            Sog                                     bigint,
                                                    
            SwDigit                                 bigint,
                                                    
            FspId                                   bigint,
            Fsp                                     nvarchar(30),
            FspServiceDescription                   nvarchar(100),
            AvailabilityId                          bigint,
            DurationId                              bigint,
            ReactionTimeId                          bigint,
            ReactionTypeId                          bigint,
            ServiceLocationId                       bigint,
            ProactiveSlaId                          bigint,

            LocalRemoteAccessSetupPreparationEffort float,
            LocalRegularUpdateReadyEffort           float,
            LocalPreparationShcEffort               float,
            CentralExecutionShcReportCost           float,
            LocalRemoteShcCustomerBriefingEffort    float,
            LocalOnSiteShcCustomerBriefingEffort    float,
            TravellingTime                          float,
            OnSiteHourlyRate                        float
        )
AS
BEGIN
		declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
		declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
		declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
		declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

        if @limit > 0
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
				where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
					AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
					AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
            )
            , cte as (
                select ROW_NUMBER() over(
                            order by
                               pro.SwDigit
                             , fsp.AvailabilityId
                             , fsp.DurationId
                             , fsp.ReactionTimeId
                             , fsp.ReactionTypeId
                             , fsp.ServiceLocationId
                             , fsp.ProactiveSlaId
                         ) as rownum
                     , pro.Id
                     , pro.Country
                     , pro.Pla
                     , pro.Sog

                     , pro.SwDigit

                     , fsp.id as FspId
                     , fsp.Name as Fsp
                     , fsp.ServiceDescription as FspServiceDescription
                     , fsp.AvailabilityId
                     , fsp.DurationId
                     , fsp.ReactionTimeId
                     , fsp.ReactionTypeId
                     , fsp.ServiceLocationId
                     , fsp.ProactiveSlaId

                     , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                     , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                     , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                     , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                     , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                     , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                     , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                    FROM SoftwareSolution.ProActiveSw pro
                    LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

				    WHERE (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
				    AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
					AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))

            )
            INSERT @tbl
            SELECT *
            from cte pro where pro.rownum > @lastid
        end
    else
        begin
            with FspCte as (
                select fsp.*
                from fsp.SwFspCodeTranslation fsp
                join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
				where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
				AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
				AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
            )
            INSERT @tbl
            SELECT -1 as rownum
                 , pro.Id
                 , pro.Country
                 , pro.Pla
                 , pro.Sog

                 , pro.SwDigit

                 , fsp.id as FspId
                 , fsp.Name as Fsp
                 , fsp.ServiceDescription as FspServiceDescription
                 , fsp.AvailabilityId
                 , fsp.DurationId
                 , fsp.ReactionTimeId
                 , fsp.ReactionTypeId
                 , fsp.ServiceLocationId
                 , fsp.ProactiveSlaId

                 , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort  else pro.LocalRemoteAccessSetupPreparationEffort       end as LocalRemoteAccessSetupPreparationEffort
                 , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort            else pro.LocalRegularUpdateReadyEffort_Approved        end as LocalRegularUpdateReadyEffort           
                 , case when @approved = 0 then pro.LocalPreparationShcEffort                else pro.LocalPreparationShcEffort_Approved            end as LocalPreparationShcEffort
                 , case when @approved = 0 then pro.CentralExecutionShcReportCost            else pro.CentralExecutionShcReportCost_Approved        end as CentralExecutionShcReportCost
                 , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort     else pro.LocalRemoteShcCustomerBriefingEffort_Approved end as LocalRemoteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.LocalOnSiteShcCustomerBriefingEffort     else pro.LocalOnSiteShcCustomerBriefingEffort_Approved end as LocalOnSiteShcCustomerBriefingEffort
                 , case when @approved = 0 then pro.TravellingTime                           else pro.TravellingTime_Approved                       end as TravellingTime
                 , case when @approved = 0 then pro.OnSiteHourlyRate                         else pro.OnSiteHourlyRate_Approved                     end as OnSiteHourlyRate

                FROM SoftwareSolution.ProActiveSw pro
                LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

				WHERE (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
				AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
				AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))

        end

    RETURN;
END
GO

ALTER FUNCTION [SoftwareSolution].[GetProActiveCosts] (
     @approved bit,
	 @cnt dbo.ListID readonly,
     @digit dbo.ListID readonly,
     @av dbo.ListID readonly,
     @year dbo.ListID readonly,
     @lastid bigint,
     @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with ProActiveCte as (
        select    pro.rownum                
                , pro.Id                    
                , pro.Country               
                , pro.Pla                   
                , pro.Sog                   
                , pro.SwDigit               
                , pro.FspId                 
                , pro.Fsp                   
                , pro.FspServiceDescription 
                , pro.AvailabilityId        
                , pro.DurationId            
                , pro.ReactionTimeId        
                , pro.ReactionTypeId        
                , pro.ServiceLocationId     
                , pro.ProactiveSlaId        

                , pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * sla.LocalPreparationShcRepetition as LocalPreparation

                , pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * sla.LocalRegularUpdateReadyRepetition as LocalRegularUpdate

                , pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalRemoteShcCustomerBriefingRepetition as LocalRemoteCustomerBriefing
                
                , pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalOnsiteShcCustomerBriefingRepetition as LocalOnsiteCustomerBriefing
                
                , pro.TravellingTime * pro.OnSiteHourlyRate * sla.TravellingTimeRepetition as Travel

                , pro.CentralExecutionShcReportCost * sla.CentralExecutionShcReportRepetition as CentralExecutionReport

                , pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate as Setup

        FROM SoftwareSolution.GetProActivePaging(@approved, @cnt, @digit, @av, @year, @lastid, @limit) pro
        LEFT JOIN Dependencies.ProActiveSla sla on sla.id = pro.ProactiveSlaId
    )
    , ProActiveCte2 as (
         select pro.*

               , pro.LocalPreparation + 
                 pro.LocalRegularUpdate + 
                 pro.LocalRemoteCustomerBriefing +
                 pro.LocalOnsiteCustomerBriefing +
                 pro.Travel +
                 pro.CentralExecutionReport as Service

        from ProActiveCte pro
    )
    select pro.*
         , Hardware.CalcProActive(pro.Setup, pro.Service, dur.Value) as ProActive
    from ProActiveCte2 pro
    LEFT JOIN Dependencies.Duration dur on dur.Id = pro.DurationId
);
GO

ALTER PROCEDURE [SoftwareSolution].[SpGetProActiveCosts]
    @approved bit,
  	@cnt dbo.ListID readonly,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

	declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
	declare @isEmptyDigit    bit = Portfolio.IsListEmpty(@digit);
	declare @isEmptyAV    bit = Portfolio.IsListEmpty(@av);
	declare @isEmptyYear    bit = Portfolio.IsListEmpty(@year);

    WITH FspCte AS (
        select fsp.SwDigitId
        from fsp.SwFspCodeTranslation fsp
        join Dependencies.ProActiveSla pro on pro.id = fsp.ProactiveSlaId and pro.Name <> '0'
		where (@isEmptyDigit = 1 or fsp.SwDigitId in (select id from @digit))
				AND (@isEmptyAV = 1 or fsp.AvailabilityId in (select id from @av))
				AND (@isEmptyYear = 1 or fsp.DurationId in (select id from @year))
    )
    SELECT @total = COUNT(pro.id)

    FROM SoftwareSolution.ProActiveSw pro
    LEFT JOIN FspCte fsp ON fsp.SwDigitId = pro.SwDigit

	WHERE (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))
		AND (@isEmptyDigit = 1 or pro.SwDigit in (select id from @digit))
		AND (@isEmptyCnt = 1 or pro.Country in (select id from @cnt))

    -----------------------------------------------------------------------------------------------------

    select    m.rownum
            , c.Name as Country               
            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts(@approved, @cnt, @digit, @av, @year, @lastid, @limit) m
    JOIN InputAtoms.Country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId

    order by m.SwDigit, m.AvailabilityId, m.DurationId, m.ProactiveSlaId;


END
GO

IF OBJECT_ID('Portfolio.IntToListID') IS NOT NULL
  DROP FUNCTION Portfolio.IntToListID;
go 

CREATE FUNCTION Portfolio.IntToListID(@var bigint)
RETURNS @tbl TABLE( id bigint NULL)
AS
BEGIN
	insert @tbl(id) values (@var)
RETURN
END
GO

IF OBJECT_ID('Report.GetCostsFull') IS NOT NULL
  DROP FUNCTION Report.GetCostsFull;
go 

CREATE FUNCTION [Report].[GetCostsFull](
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS @tbl TABLE (
	      Fsp nvarchar(max) NULL
	     ,FspDescription nvarchar(max) NULL

		 ,Id bigint NOT NULL

         ,CountryId bigint NOT NULL
         ,Country nvarchar(max) NULL
         ,WgId bigint NOT NULL
         ,Wg nvarchar(max) NULL
         ,AvailabilityId bigint NOT NULL
         ,Availability nvarchar(max) NULL
         ,DurationId bigint NOT NULL
         ,Duration nvarchar(max) NULL
         ,Year int NOT NULL
         ,IsProlongation bit NOT NULL
         ,ReactionTimeId bigint NOT NULL
         ,ReactionTime nvarchar(max) NULL
         ,ReactionTypeId bigint NOT NULL
         ,ReactionType nvarchar(max) NULL
         ,ServiceLocationId bigint NOT NULL
         ,ServiceLocation nvarchar(max) NULL
         ,ProActiveSlaId bigint NOT NULL
         ,ProActiveSla nvarchar(max) NULL

         ,StdWarranty int NULL

         --Cost

         ,AvailabilityFee float NULL
         ,HddRet float NULL
         ,TaxAndDutiesW float NULL
         ,TaxAndDutiesOow float NULL
         ,Reinsurance float NULL
         ,ProActive float NULL
         ,ServiceSupportCost float NULL

         , MaterialW float NULL
         , MaterialOow float NULL
         , FieldServiceCost float NULL
         , Logistic float NULL
         , OtherDirect float NULL
         
         , LocalServiceStandardWarranty float NULL
         
         , Credits float NULL


         , ServiceTC float NULL
         , ServiceTP float NULL

         ,ServiceTC1 float NULL
         ,ServiceTC2 float NULL
         ,ServiceTC3 float NULL
         ,ServiceTC4 float NULL
         ,ServiceTC5 float NULL
         ,ServiceTC1P float NULL

         ,ServiceTP1 float NULL
         ,ServiceTP2 float NULL
         ,ServiceTP3 float NULL
         ,ServiceTP4 float NULL
         ,ServiceTP5 float NULL
         ,ServiceTP1P float NULL

         ,ListPrice float NULL
         ,DealerDiscount float NULL
         ,DealerPrice float NULL
         ,ServiceTCManual float NULL
         ,ServiceTPManual float NULL
         ,ChangeUserName nvarchar(max) NULL
         ,ChangeUserEmail nvarchar(max) NULL

         ,ServiceTP_Released float NULL

         ,SlaHash int NOT NULL
) 
AS
begin
	declare @cntTable dbo.ListId;
	if @cnt is not null insert into @cntTable(id) SELECT id FROM Portfolio.IntToListID(@cnt);

	declare @wgTable dbo.ListId;
	if @wg is not null insert into @wgTable(id) SELECT id FROM Portfolio.IntToListID(@wg);

	declare @avTable dbo.ListId;
	if @av is not null insert into @avTable(id) SELECT id FROM Portfolio.IntToListID(@av);

	declare @durTable dbo.ListId;
	if @dur is not null insert into @durTable(id) SELECT id FROM Portfolio.IntToListID(@dur);

	declare @rtimeTable dbo.ListId;
	if @reactiontime is not null insert into @rtimeTable(id) SELECT id FROM Portfolio.IntToListID(@reactiontime);
	
	declare @rtypeTable dbo.ListId;
	if @reactiontype is not null insert into @rtypeTable(id) SELECT id FROM Portfolio.IntToListID(@reactiontype);
	
	declare @locTable dbo.ListId;
	if @loc is not null insert into @locTable(id) SELECT id FROM Portfolio.IntToListID(@loc);
	
	declare @proTable dbo.ListId;
	if @pro is not null insert into @proTable(id) SELECT id FROM Portfolio.IntToListID(@pro);

	insert into @tbl
    select 
           fsp.Name as Fsp
         , fsp.ServiceDescription as FspDescription

         ,m.*

    FROM Hardware.GetCostsFull(1, @cntTable, @wgTable, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, 0, -1) m
    LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                           and fsp.CountryId = m.CountryId
                                           and fsp.WgId = m.WgId
                                           and fsp.AvailabilityId = m.AvailabilityId
                                           and fsp.DurationId= m.DurationId
                                           and fsp.ReactionTimeId = m.ReactionTimeId
                                           and fsp.ReactionTypeId = m.ReactionTypeId
                                           and fsp.ServiceLocationId = m.ServiceLocationId
                                           and fsp.ProactiveSlaId = m.ProActiveSlaId

return

end
GO

ALTER FUNCTION [Report].[Contract]
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
         , wg.Name as Wg
         , wg.Description as WgDescription
         , null as SLA
         , m.ServiceLocation
         , m.ReactionTime
         , m.ReactionType
         , m.Availability
         , m.ProActiveSla
		 , case when m.DurationId >= 1 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP1
         , case when m.DurationId >= 2 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP2
         , case when m.DurationId >= 3 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP3
         , case when m.DurationId >= 4 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP4
         , case when m.DurationId >= 5 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP5

		 , case when m.DurationId >= 1 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly1
		 , case when m.DurationId >= 2 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly2
		 , case when m.DurationId >= 3 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly3
		 , case when m.DurationId >= 4 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly4
		 , case when m.DurationId >= 5 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly5

         , m.StdWarranty as WarrantyLevel
         , null as PortfolioType
         , wg.Sog as Sog	

    from Report.GetCostsFull(@cnt, @wg, @av, (select top(1) id from Dependencies.Duration where IsProlongation = 0 and Value = 5), @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
	join Dependencies.Duration dur on dur.id = m.DurationId
)
GO

IF OBJECT_ID('Report.CalcOutputNewVsOld') IS NOT NULL
  DROP FUNCTION Report.CalcOutputNewVsOld;
go 

CREATE FUNCTION [Report].[CalcOutputNewVsOld]
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
RETURNS @tbl TABLE (
	Id bigint NOT NULL
	,Country nvarchar(max) NULL
  ,SogDescription nvarchar(max) NULL
  ,Name nvarchar(max) NULL
  ,WgDescription nvarchar(max) NULL
  ,ServiceLocation nvarchar(max) NULL
  ,ReactionTime nvarchar(max) 
  ,Wg nvarchar(max) NULL
  ,ServiceProduct nvarchar(max) NULL
  ,LocalServiceStandardWarranty nvarchar(max) NULL
  ,StandardWarrantyOld nvarchar(max) NULL
  ,Sog nvarchar(max) NULL
  ,Bw float NULL
)
AS
begin
	declare @cntTable dbo.ListId;
	if @cnt is not null insert into @cntTable(id) SELECT id FROM Portfolio.IntToListID(@cnt);

	declare @wgTable dbo.ListId;
	if @wg is not null insert into @wgTable(id) SELECT id FROM Portfolio.IntToListID(@wg);

	declare @avTable dbo.ListId;
	if @av is not null insert into @avTable(id) SELECT id FROM Portfolio.IntToListID(@av);

	declare @durTable dbo.ListId;
	if @dur is not null insert into @durTable(id) SELECT id FROM Portfolio.IntToListID(@dur);

	declare @rtimeTable dbo.ListId;
	if @reactiontime is not null insert into @rtimeTable(id) SELECT id FROM Portfolio.IntToListID(@reactiontime);
	
	declare @rtypeTable dbo.ListId;
	if @reactiontype is not null insert into @rtypeTable(id) SELECT id FROM Portfolio.IntToListID(@reactiontype);
	
	declare @locTable dbo.ListId;
	if @loc is not null insert into @locTable(id) SELECT id FROM Portfolio.IntToListID(@loc);
	
	declare @proTable dbo.ListId;
	if @pro is not null insert into @proTable(id) SELECT id FROM Portfolio.IntToListID(@pro);

	insert into @tbl
    select    m.Id
            , m.Country 
            , wg.SogDescription
            , fsp.Name
            , wg.Description as WgDescription
            , m.ServiceLocation
            , m.ReactionTime
            , m.Wg
         
            , (m.Duration + ' ' + m.ServiceLocation) as ServiceProduct
            
            , m.LocalServiceStandardWarranty
            , null as StandardWarrantyOld

            , wg.Sog

            , (100 * (m.LocalServiceStandardWarranty - null) / m.LocalServiceStandardWarranty) as Bw

    FROM Hardware.GetCostsFull(0, @cntTable, @wgTable, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, 0, -1) m --not approved

    INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

    LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                           and fsp.CountryId = m.CountryId
                                           and fsp.WgId = m.WgId
                                           and fsp.AvailabilityId = m.AvailabilityId
                                           and fsp.DurationId= m.DurationId
                                           and fsp.ReactionTimeId = m.ReactionTimeId
                                           and fsp.ReactionTypeId = m.ReactionTypeId
                                           and fsp.ServiceLocationId = m.ServiceLocationId
                                           and fsp.ProactiveSlaId = m.ProActiveSlaId
return;
end
GO

ALTER FUNCTION [Report].[LogisticCostCalcCentral]
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
    select    m.Id
            , c.Region
            , c.Name as Country
            , m.Wg

            , m.ServiceLocation as ServiceLevel
            , m.ReactionTime
            , m.ReactionType
            , m.Duration
            , m.Availability
            , m.ProActiveSla

            , coalesce(m.ServiceTCManual, m.ServiceTC) as ServiceTC
            , lc.StandardHandling_Approved as Handling
            , m.TaxAndDutiesW
            , m.TaxAndDutiesOow

            , m.Logistic as LogisticW
            , null as LogisticOow

            , m.AvailabilityFee as Fee

    from Report.GetCostsFull(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.CountryView c on c.Id = m.CountryId
    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId
)
GO

UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='wg' 
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='sog'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='countrygroup'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='country'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='availability'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='duration'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='reactiontime'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='reactiontype'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='servicelocation'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='year'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='proactive'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='usercountry'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='swdigit'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='wgall'
UPDATE [Report].[ReportFilterType] SET [MultiSelect] = 0 WHERE [Name]='wgstandard'

INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('wg', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('country', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('availability', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('duration', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('reactiontime', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('reactiontype', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('servicelocation', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('year', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('proactive', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('usercountry', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('swdigit', 1)
INSERT INTO [Report].[ReportFilterType] (Name, MultiSelect) VALUES('wgstandard', 1)

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HW-CALC-RESULT')
declare @index int = 0;
delete from Report.ReportFilter where ReportId = @reportId;
declare @filterTypeId bigint = 0

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'local', 'Local currency');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'country' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'country', 'Country');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'wg' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'wg', 'Asset(WG)');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'availability' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'availability', 'Availability');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'duration' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'duration', 'Duration');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'reactiontime' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'reactiontime', 'Reaction time');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'reactiontype' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'reactiontype', 'Reaction type');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'servicelocation' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'servicelocation', 'Service location');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'proactive' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'proactive', 'ProActive');


set @reportId = (select Id from Report.Report where upper(Name) = 'SW-CALC-RESULT')
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;


set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'swdigit' and MultiSelect=1), 'digit', 'SW digit');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'availability' and MultiSelect=1), 'availability', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'year' and MultiSelect=1), 'year', 'Year');

set @reportId = (select Id from Report.Report where upper(Name) = 'SW-PROACTIVE-CALC-RESULT')
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'country' and MultiSelect=1), 'Country', 'Country');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'swdigit' and MultiSelect=1), 'digit', 'SW digit');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'availability' and MultiSelect=1), 'availability', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where Name = 'year' and MultiSelect=1), 'year', 'Service period');

GO
ALTER FUNCTION [Report].[HwCalcResult]
(
    @approved bit,
    @local bit,
    @country dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @availability dbo.ListID readonly,
    @duration dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @servicelocation dbo.ListID readonly,
    @proactive dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    with CurrencyCte as (
        select   case when @local = 1 then cur.Name else 'EUR' end as Currency
               , case when @local = 1 then er.Value else 1     end as Exchange
        from [References].Currency cur
        join [References].ExchangeRate er on er.CurrencyId = cur.Id
        where cur.Id = (select CurrencyId from InputAtoms.Country where id  in (select id from @country))
    )
    select    Country
            , case when @local = 1 then cur.Name else 'EUR' end as Currency

            , Wg
            , Availability
            , Duration
            , ReactionTime
            , ReactionType
            , ServiceLocation
            , ProActiveSla

            , StdWarranty

            --Cost

            , case when @local = 1 then AvailabilityFee * er.Value else AvailabilityFee end as AvailabilityFee 
            , case when @local = 1 then HddRet * er.Value else HddRet end as HddRet
            , case when @local = 1 then TaxAndDutiesW * er.Value else TaxAndDutiesW end as TaxAndDutiesW
            , case when @local = 1 then TaxAndDutiesOow * er.Value else TaxAndDutiesOow end as TaxAndDutiesOow
            , case when @local = 1 then Reinsurance * er.Value else Reinsurance end as Reinsurance
            , case when @local = 1 then ProActive * er.Value else ProActive end as ProActive
            , case when @local = 1 then ServiceSupportCost * er.Value else ServiceSupportCost end as ServiceSupportCost
                                                          
            , case when @local = 1 then MaterialW * er.Value else MaterialW end as MaterialW
            , case when @local = 1 then MaterialOow * er.Value else MaterialOow end as MaterialOow
            , case when @local = 1 then FieldServiceCost * er.Value else FieldServiceCost end as FieldServiceCost
            , case when @local = 1 then Logistic * er.Value else Logistic end as Logistic
            , case when @local = 1 then OtherDirect * er.Value else OtherDirect end as OtherDirect
            , case when @local = 1 then LocalServiceStandardWarranty * er.Value else LocalServiceStandardWarranty end as LocalServiceStandardWarranty
            , case when @local = 1 then Credits * er.Value else Credits end as Credits
            , case when @local = 1 then ServiceTC * er.Value else ServiceTC end as ServiceTC
            , case when @local = 1 then ServiceTP * er.Value else ServiceTP end as ServiceTP
                                                          
            , case when @local = 1 then ServiceTCManual * er.Value else ServiceTCManual end as ServiceTCManual
            , case when @local = 1 then ServiceTPManual * er.Value else ServiceTPManual end as ServiceTPManual
                                                          
            , case when @local = 1 then ServiceTP_Released * er.Value else ServiceTP_Released end as ServiceTP_Released
                                                          
            , case when @local = 1 then ListPrice * er.Value else ListPrice end as ListPrice
            , case when @local = 1 then DealerPrice * er.Value else DealerPrice end as DealerPrice
            , DealerDiscount                               as DealerDiscount
                                                           
            , ChangeUserName + '[' + ChangeUserEmail + ']' as ChangeUser

    from Hardware.GetCosts(@approved, @country, @wg, @availability, @duration, @reactiontime, @reactiontype, @servicelocation, @proactive, -1, -1)
	join [References].Currency cur on cur.Id in (select CurrencyId from InputAtoms.Country where id in (select id from @country))
	join [References].ExchangeRate er on er.CurrencyId = cur.Id
)
GO

ALTER PROCEDURE [Portfolio].[DenyLocalPortfolioById]
    @ids dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;

    DELETE FROM Portfolio.LocalPortfolio
    WHERE (Id in (select Id from @ids));

END
GO

IF OBJECT_ID('Report.SolutionPackPriceList') IS NOT NULL
  DROP FUNCTION Report.SolutionPackPriceList;
go 

CREATE FUNCTION [Report].[SolutionPackPriceList]
(
    @digit bigint
)
RETURNS @tbl TABLE (
	Digit nvarchar(max) NULL
	,SogDescription nvarchar(max) NULL
	,Sog nvarchar(max) NULL
	
	,Fsp nvarchar(max) NULL
	,SpDescription nvarchar(max) NULL
	,Sp nvarchar(max) NULL

	,TP float NULL
	,DealerPrice float NULL
	,ListPrice float NULL
)
as
begin
	declare @digitList dbo.ListId; 
	if @digit is not null insert into @digitList(id) select id from Portfolio.IntToListID(@digit);

	declare @emptyAv dbo.ListId;
	declare @emptyYear dbo.ListId;

	insert into @tbl
    select    dig.Name as Digit
            , sog.Description as SogDescription
            , sog.Name as Sog

            , fsp.Name as Fsp
            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @emptyAv, @emptyYear, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.SwDigitId = sw.SwDigit
                                          and fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
return
end
go

IF OBJECT_ID('Report.SolutionPackPriceListDetail') IS NOT NULL
  DROP FUNCTION Report.SolutionPackPriceListDetail;
go 

CREATE FUNCTION [Report].SolutionPackPriceListDetail
(
	@digit bigint
)
RETURNS @tbl TABLE (
	SogDescription nvarchar(max) NULL
	,Digit nvarchar(max) NULL
	,Fsp nvarchar(max) NULL
	,Sog nvarchar(max) NULL
	
	,SpDescription nvarchar(max) NULL
	,Sp nvarchar(max) NULL

	,SupportCost float NULL

	,Reinsurance float NULL

	,TP float NULL
	,DealerPrice float NULL
	,ListPrice float NULL
)
as
begin
	declare @digitList dbo.ListId; 
	if @digit is not null insert into @digitList(id) select id from Portfolio.IntToListID(@digit);

	declare @emptyAv dbo.ListId;
	declare @emptyYear dbo.ListId;
	
	insert into @tbl
     select    sog.Description as SogDescription
            , dig.Name as Digit
            , fsp.Name as Fsp
            , sog.Name as Sog

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.[2ndLevelSupportCosts] as SupportCost
            
            , sw.Reinsurance as Reinsurance

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @emptyAv, @emptyYear, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.SwDigitId = sw.SwDigit
                                          and fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
return
end
GO

IF OBJECT_ID('Report.SolutionPackProActiveCosting') IS NOT NULL
  DROP FUNCTION Report.SolutionPackProActiveCosting;
go 

CREATE FUNCTION Report.SolutionPackProActiveCosting
(
    @cnt bigint,
    @digit bigint,
    @year bigint
)
RETURNS @tbl TABLE (
	CountryGroup nvarchar(max) NULL
	,Country nvarchar(max) NULL
	,InfSolution nvarchar(max) NULL
	,Wg nvarchar(max) NULL
	
	,Sog nvarchar(max) NULL
	,Fsp nvarchar(max) NULL

	,ServiceDescription nvarchar(max) NULL

	,Sp nvarchar(max) NULL
	,Duration nvarchar(max) NULL
	,Availability nvarchar(max) NULL

	,ReActive float NULL
	,ProActive float NULL
	,ServiceTP float NULL
)
as
begin
	declare @cntList dbo.ListId; 
	if @cnt is not null insert into @cntList(id) select id from Portfolio.IntToListID(@cnt);

	declare @digitList dbo.ListId; 
	if @digit is not null insert into @digitList(id) select id from Portfolio.IntToListID(@digit);

	declare @yearList dbo.ListId; 
	if @year is not null insert into @yearList(id) select id from Portfolio.IntToListID(@year);

	declare @emptyAv dbo.ListId;

   insert into @tbl
   select    c.CountryGroup
            , c.Name as Country

            , dig.Name as InfSolution
            , sog.Name as Wg
            , sog.Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription
            , sog.Description as Sp

            , case 
                when y.IsProlongation = 1 then 'Prolongation'
                else CAST(y.Value as varchar(15))
             end as Duration

             , av.Name as Availability

             , sc.TransferPrice - pro.ProActive as ReActive
             , pro.ProActive as ProActive
             , sc.TransferPrice as ServiceTP

    from SoftwareSolution.GetProActiveCosts(1, @cntList, @digitList, @emptyAv, @yearList, -1, -1) pro
    join Dependencies.Year y on y.id = pro.DurationId
    join Dependencies.Availability av on av.id = pro.AvailabilityId
    join InputAtoms.CountryView c on c.id = pro.Country
    join InputAtoms.SwDigit dig on dig.Id = pro.SwDigit
    join InputAtoms.WgSogView sog on sog.id = pro.Sog
    left join SoftwareSolution.GetCosts(1, @digitList, @emptyAv, @yearList, -1, -1) sc on sc.Year = pro.DurationId and sc.Availability = pro.AvailabilityId and sc.SwDigit = pro.SwDigit
    left join Fsp.SwFspCodeTranslation fsp on fsp.Id = pro.FspId
return
end
go

IF OBJECT_ID('Report.GetServiceCostsBySla') IS NOT NULL
  DROP FUNCTION Report.GetServiceCostsBySla;
go 

CREATE FUNCTION [Report].[GetServiceCostsBySla]
(
    @cnt nvarchar(200),
    @loc nvarchar(200),
    @av nvarchar(200),
    @reactiontime nvarchar(200),
    @reactiontype nvarchar(200),
    @wg nvarchar(200),   
    @dur nvarchar(200)
)
RETURNS @tbl TABLE (
    Country nvarchar(200),
    ServiceTC float, 
    ServiceTP float, 
    ServiceTP1 float,
    ServiceTP2 float,
    ServiceTP3 float,
    ServiceTP4 float,
    ServiceTP5 float
)
AS
BEGIN

    declare @cntId dbo.ListId;
    declare @locId dbo.ListId;
    declare @avId dbo.ListId;
    declare @reactiontimeId dbo.ListId;
    declare @reactiontypeId dbo.ListId;
    declare @wgId dbo.ListId;
    declare @durId dbo.ListId;
	declare @proId dbo.ListId;

    insert into @cntId select id from InputAtoms.Country where UPPER(Name)= UPPER(@cnt);
    insert into @locId select  id from Dependencies.ServiceLocation where UPPER(Name) = UPPER(@loc);
    insert into @avId select   id from Dependencies.Availability where ExternalName like '%' + @av + '%';
    insert into @reactiontimeId select   id from Dependencies.ReactionTime where UPPER(Name)=UPPER(@reactiontime);
    insert into @reactiontypeId select   id from Dependencies.ReactionType where UPPER(Name)=UPPER(@reactiontype);
    insert into @wgId select id from InputAtoms.Wg where UPPER(Name)=UPPER(@wg);
    insert into @durId select id from Dependencies.Duration where UPPER(Name)=UPPER(@dur);

    INSERT @tbl
    select costs.Country,
           coalesce(costs.ServiceTCManual, costs.ServiceTC) as ServiceTC, 
		   costs.ServiceTP_Released as ServiceTP, 
           costs.ServiceTP1,
           costs.ServiceTP2,
           costs.ServiceTP3,
           costs.ServiceTP4,
           costs.ServiceTP5
     from Hardware.GetCostsFull(0, @cntId, @wgId, @avId, @durId, @reactiontimeId, @reactiontypeId, @locId, @proId, 0, -1) costs

    return;
END
GO

IF OBJECT_ID('Report.SwCalcResult') IS NOT NULL
  DROP FUNCTION Report.SwCalcResult;
go 

CREATE FUNCTION Report.SwCalcResult
(
	@approved bit,
    @digit dbo.ListID readonly,
    @availability dbo.ListID readonly,
    @year dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select    d.Name as SwDigit
            , sog.Name as Sog
            , av.Name as Availability 
            , y.Name as Year
            , m.ServiceSupport
            , m.Reinsurance
            , m.TransferPrice
            , m.MaintenanceListPrice
            , m.DealerPrice
    from SoftwareSolution.GetCosts(@approved, @digit, @availability, @year, -1, -1) m
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join Dependencies.Availability av on av.Id = m.Availability
    join Dependencies.Year y on y.Id = m.Year
)
GO

IF OBJECT_ID('Report.SwProactiveCalcResult') IS NOT NULL
  DROP FUNCTION Report.SwProactiveCalcResult;
go 

CREATE FUNCTION Report.SwProactiveCalcResult
(
    @approved bit,
    @country dbo.ListID readonly,
    @digit dbo.ListID readonly,
    @availability dbo.ListID readonly,
    @year dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select    c.Name as Country               
            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts(@approved, @country, @digit, @availability, @year, -1, -1) m
    JOIN InputAtoms.Country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId
)
GO

IF OBJECT_ID('Report.SwServicePriceListDetail') IS NOT NULL
  DROP FUNCTION Report.SwServicePriceListDetail;
go 

CREATE FUNCTION Report.SwServicePriceListDetail
(
    @digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
	Digit nvarchar(max) NULL
	,SogDescription nvarchar(max) NULL
	,Sog nvarchar(max) NULL
	,Fsp2 nvarchar(max) NULL
	,Fsp nvarchar(max) NULL
	
	,SpDescription nvarchar(max) NULL
	,Sp nvarchar(max) NULL

	,ServiceSupport float NULL
	,Reinsurance float NULL

	,TP float NULL
	,DealerPrice float NULL
	,ListPrice float NULL
)
as
begin
	declare @digitList dbo.ListId; 
	if @digit is not null insert into @digitList(id) select id from Portfolio.IntToListID(@digit);

	declare @avList dbo.ListId; 
	if @av is not null insert into @avList(id) select id from Portfolio.IntToListID(@av);

	declare @yearList dbo.ListId; 
	if @year is not null insert into @yearList(id) select id from Portfolio.IntToListID(@year);

	insert into @tbl
    select    dig.Name as Digit
            , sog.Description as SogDescription
            , sog.Name as Sog
            , null as Fsp2
            , fsp.Name as Fsp

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.ServiceSupport as ServiceSupport
            , sw.Reinsurance as Reinsurance

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @avList, @yearList, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
                                          and fsp.SogId = sw.Sog
return
end
GO

IF OBJECT_ID('Report.SwServicePriceList') IS NOT NULL
  DROP FUNCTION Report.SwServicePriceList;
go 

CREATE FUNCTION [Report].[SwServicePriceList]
(
	@digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
	Digit nvarchar(max) NULL
	,SogDescription nvarchar(max) NULL
	,Sog nvarchar(max) NULL
	,Fsp nvarchar(max) NULL
	
	,SpDescription nvarchar(max) NULL
	,Sp nvarchar(max) NULL

	,TP float NULL
	,DealerPrice float NULL
	,ListPrice float NULL
)
as
begin
	declare @digitList dbo.ListId; 
	if @digit is not null insert into @digitList(id) select id from Portfolio.IntToListID(@digit);

	declare @avList dbo.ListId; 
	if @av is not null insert into @avList(id) select id from Portfolio.IntToListID(@av);

	declare @yearList dbo.ListId; 
	if @year is not null insert into @yearList(id) select id from Portfolio.IntToListID(@year);

	insert into @tbl
    select 
              dig.Name as Digit
            , sog.Description as SogDescription
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @avList, @yearList, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
                                          and fsp.SogId = sw.Sog
return
end
GO