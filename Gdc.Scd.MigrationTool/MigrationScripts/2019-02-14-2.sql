IF OBJECT_ID('Fsp.HwStandardWarranty', 'U') IS NOT NULL
  DROP TABLE Fsp.HwStandardWarranty;
go

CREATE TABLE Fsp.HwStandardWarranty(
    [Country] [bigint] NOT NULL foreign key references InputAtoms.Country(Id),
    [Wg] [bigint] NOT NULL foreign key references InputAtoms.Wg(Id),
    [AvailabilityId] [bigint] NOT NULL foreign key references Dependencies.Availability(Id),
    [Duration] [bigint] NOT NULL foreign key references Dependencies.Duration(Id),
    [ReactionTimeId] [bigint] NOT NULL foreign key references Dependencies.ReactionTime(id),
    [ReactionTypeId] [bigint] NOT NULL foreign key references Dependencies.ReactionType(id),
    [ServiceLocationId] [bigint] NOT NULL foreign key references Dependencies.ServiceLocation(id),
    [ProActiveSlaId] [bigint] NOT NULL foreign key references Dependencies.ProActiveSla(id),
    CONSTRAINT PK_HwStandardWarranty PRIMARY KEY NONCLUSTERED (Country, Wg)
)
GO

IF OBJECT_ID('Fsp.HwFspCodeTranslation_Updated', 'TR') IS NOT NULL
  DROP TRIGGER Fsp.HwFspCodeTranslation_Updated;
go

CREATE TRIGGER [Fsp].[HwFspCodeTranslation_Updated]
ON [Fsp].[HwFspCodeTranslation]
After INSERT, UPDATE
AS BEGIN

    truncate table Fsp.HwStandardWarranty;

    -- Disable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty NOCHECK CONSTRAINT ALL;

    WITH StdCte AS (

    --remove duplicates in FSP
      SELECT fsp.CountryId
           , fsp.WgId
           , fsp.AvailabilityId   
           , fsp.DurationId       
           , fsp.ReactionTimeId   
           , fsp.ReactionTypeId   
           , fsp.ServiceLocationId
           , fsp.ProactiveSlaId   
           , row_number() OVER(PARTITION BY fsp.CountryId, fsp.WgId
                 ORDER BY 
                     fsp.CountryId
                   , fsp.WgId
                   , fsp.AvailabilityId    
                   , fsp.DurationId        
                   , fsp.ReactionTimeId    
                   , fsp.ReactionTypeId    
                   , fsp.ServiceLocationId 
                   , fsp.ProactiveSlaId    ) AS rownum

        from Fsp.HwFspCodeTranslation fsp
        where fsp.IsStandardWarranty = 1 
    )
    , StdCte2 as (
        select * from StdCte WHERE rownum = 1
    )
    , Fsp as (

        --find FSP for countries

        select    fsp.CountryId
                , fsp.WgId
                , fsp.AvailabilityId
                , fsp.DurationId
                , fsp.ReactionTimeId
                , fsp.ReactionTypeId
                , fsp.ServiceLocationId
                , fsp.ProactiveSlaId
        from StdCte2 fsp
        where CountryId is not null
    )
    , Fsp2 as (
        
        --create default country FSP

        select    c.Id as CountryId
                , fsp.WgId
                , fsp.AvailabilityId
                , fsp.DurationId
                , fsp.ReactionTimeId
                , fsp.ReactionTypeId
                , fsp.ServiceLocationId
                , fsp.ProactiveSlaId
        from StdCte2 fsp, InputAtoms.Country c
        where fsp.CountryId is null and c.IsMaster = 1
    )
    , StdFsp as (

        --get country FSP(if exists), or default country FSP

        select 
                  coalesce(fsp.CountryId          , fsp2.CountryId        ) as CountryId        
                , coalesce(fsp.WgId               , fsp2.WgId             ) as WgId             
                , coalesce(fsp.AvailabilityId     , fsp2.AvailabilityId   ) as AvailabilityId   
                , coalesce(fsp.DurationId         , fsp2.DurationId       ) as DurationId       
                , coalesce(fsp.ReactionTimeId     , fsp2.ReactionTimeId   ) as ReactionTimeId   
                , coalesce(fsp.ReactionTypeId     , fsp2.ReactionTypeId   ) as ReactionTypeId   
                , coalesce(fsp.ServiceLocationId  , fsp2.ServiceLocationId) as ServiceLocationId
                , coalesce(fsp.ProactiveSlaId     , fsp2.ProactiveSlaId   ) as ProactiveSlaId   
        from Fsp2 fsp2
        left join Fsp fsp on fsp.CountryId = fsp2.CountryId and fsp.WgId = fsp2.WgId
    )
    insert into Fsp.HwStandardWarranty(Country, Wg, AvailabilityId, Duration, ReactionTimeId, ReactionTypeId, ServiceLocationId, ProactiveSlaId)
        select    fsp.CountryId, fsp.WgId, fsp.AvailabilityId, fsp.DurationId, fsp.ReactionTimeId, fsp.ReactionTypeId, fsp.ServiceLocationId, fsp.ProactiveSlaId
        from StdFsp fsp;

    -- Enable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty CHECK CONSTRAINT ALL;

END
GO

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
         , std.ProActiveSlaId
    FROM fsp.HwStandardWarranty std
    INNER JOIN Dependencies.Duration dur on dur.Id = std.Duration
GO

update Fsp.HwFspCodeTranslation set Name = Name;
GO

IF OBJECT_ID('Hardware.AddMarkupFactorOrFixValue') IS NOT NULL
  DROP FUNCTION Hardware.AddMarkupFactorOrFixValue;
go 

IF OBJECT_ID('Hardware.MarkupOrFixValue') IS NOT NULL
  DROP FUNCTION Hardware.MarkupOrFixValue;
go 

CREATE FUNCTION Hardware.MarkupOrFixValue (
    @value float,
    @markupFactor float,
    @fixed float
)
RETURNS float 
AS
BEGIN

    if @markupFactor > 0
        begin
            return @value * @markupFactor;
        end
    else if @fixed > 0
        begin
            return @fixed;
        end

    RETURN 0;

END
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
    SELECT   m.Id

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

            , stdw.DurationValue   as StdWarranty

            , case when @approved = 0 then afr.AFR1  else AFR1_Approved       end as AFR1 
            , case when @approved = 0 then afr.AFR2  else AFR2_Approved       end as AFR2 
            , case when @approved = 0 then afr.AFR3  else afr.AFR3_Approved   end as AFR3 
            , case when @approved = 0 then afr.AFR4  else afr.AFR4_Approved   end as AFR4 
            , case when @approved = 0 then afr.AFR5  else afr.AFR5_Approved   end as AFR5 
            , case when @approved = 0 then afr.AFRP1 else afr.AFRP1_Approved  end as AFRP1
       
            , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved       end as MaterialCostWarranty
            , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved            end as MaterialCostOow     
                                                                                                                      
            , case when @approved = 0 then tax.TaxAndDuties                   else tax.TaxAndDuties_Approved               end as TaxAndDuties
                                                                                                                      
            , case when @approved = 0 then r.Cost                             else r.Cost_Approved                         end as Reinsurance
                                                                                                                      
            , case when @approved = 0 then fscStd.LabourCost                  else fscStd.LabourCost_Approved              end as StdLabourCost             
            , case when @approved = 0 then fscStd.TravelCost                  else fscStd.TravelCost_Approved              end as StdTravelCost             
            , case when @approved = 0 then fscStd.PerformanceRate             else fscStd.PerformanceRate_Approved         end as StdPerformanceRate        
                                                                                                                      
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end as TravelCost             
            , case when @approved = 0 then fsc.TimeAndMaterialShare           else fsc.TimeAndMaterialShare_Approved       end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.PerformanceRate                else fsc.PerformanceRate_Approved            end as PerformanceRate        
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             
            , case when @approved = 0 then fsc.OnsiteHourlyRates              else fsc.OnsiteHourlyRates_Approved          end as OnsiteHourlyRates      
                                                                                                                      
            , case when @approved = 0 then ssc.[1stLevelSupportCosts]         else ssc.[1stLevelSupportCosts_Approved]     end as [1stLevelSupportCosts] 
            , case when @approved = 0 then ssc.[2ndLevelSupportCosts]         else ssc.[2ndLevelSupportCosts_Approved]     end as [2ndLevelSupportCosts] 
                                                                                                                      
            , case when @approved = 0 then ssc.ServiceSupport                 else ssc.ServiceSupport_Approved             end as ServiceSupport
         
            , case when @approved = 0 then lcStd.ExpressDelivery              else lcStd.ExpressDelivery_Approved          end as StdExpressDelivery         
            , case when @approved = 0 then lcStd.HighAvailabilityHandling     else lcStd.HighAvailabilityHandling_Approved end as StdHighAvailabilityHandling
            , case when @approved = 0 then lcStd.StandardDelivery             else lcStd.StandardDelivery_Approved         end as StdStandardDelivery        
            , case when @approved = 0 then lcStd.StandardHandling             else lcStd.StandardHandling_Approved         end as StdStandardHandling        
            , case when @approved = 0 then lcStd.ReturnDeliveryFactory        else lcStd.ReturnDeliveryFactory_Approved    end as StdReturnDeliveryFactory   
            , case when @approved = 0 then lcStd.TaxiCourierDelivery          else lcStd.TaxiCourierDelivery_Approved      end as StdTaxiCourierDelivery     

            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
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

    FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fscStd ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId AND fscStd.ReactionTypeId = stdw.ReactionTypeId AND fscStd.ReactionTimeId = stdw.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Hardware.LogisticsCostView lcStd on lcStd.Country = stdw.Country AND lcStd.Wg = stdw.Wg AND lcStd.ReactionTime = stdw.ReactionTimeId AND lcStd.ReactionType = stdw.ReactionTypeId

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

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

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
GO

ALTER FUNCTION Report.PoStandardWarrantyMaterial
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
    with cte as (
        select 
                m.Id
              , c.CountryGroup
              , c.LUTCode
              , wg.Name as Wg
              , wg.Description as WgDescription
              , pla.Name as Pla
              , dur.Value as Year
              , dur.IsProlongation
              , (dur.Name + ' ' + loc.Name) as ServiceLevel
              , rtime.Name as ReactionTime
              , rtype.Name as ReactionType
              , av.Name    as Availability
              , prosla.ExternalName as ProActiveSla

              , stdw.DurationValue as StdWarranty

              , mcw.MaterialCostWarranty_Approved as MaterialCostWarranty

              , afr.AFR1_Approved as AFR1
              , afr.AFR2_Approved as AFR2
              , afr.AFR3_Approved as AFR3
              , afr.AFR4_Approved as AFR4
              , afr.AFR5_Approved as AFR5

              , null as SparesAvailability

        from Portfolio.GetBySla(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
    )
    , cte2 as (
        select    
              m.*

                , case when m.StdWarranty >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdWarranty >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdWarranty >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdWarranty >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdWarranty >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5
        from cte m
    )
    select    m.Id
            , m.CountryGroup
            , m.LUTCode
            , m.Wg
            , m.WgDescription
            , m.Pla
            , m.ServiceLevel
            , m.ReactionTime
            , m.ReactionType
            , m.Availability
            , m.ProActiveSla

            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.mat1, m.mat2, m.mat3, m.mat4, m.mat5, 0) as MaterialW

            , m.MaterialCostWarranty

            , m.AFR1
            , m.AFR2
            , m.AFR3
            , m.AFR4
            , m.AFR5

            , m.SparesAvailability
    from cte2 m
)
GO

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

                , coalesce(tax.TaxAndDuties, 0) as TaxAndDuties, coalesce(tax.TaxAndDuties_Approved, 0) as TaxAndDuties_Approved

                , fsc.TravelCost + fsc.LabourCost + coalesce(fsc.PerformanceRate, 0) as FieldServicePerYearStdw
                , fsc.TravelCost_Approved + fsc.LabourCost_Approved + coalesce(fsc.PerformanceRate_Approved, 0)  as FieldServicePerYearStdw_Approved

                , ssc.ServiceSupport         , ssc.ServiceSupport_Approved

                , lc.StandardHandling + lc.HighAvailabilityHandling + lc.StandardDelivery + lc.ExpressDelivery + lc.TaxiCourierDelivery + lc.ReturnDeliveryFactory as LogisticPerYearStdw
                , lc.StandardHandling_Approved + lc.HighAvailabilityHandling_Approved + lc.StandardDelivery_Approved + lc.ExpressDelivery_Approved + lc.TaxiCourierDelivery_Approved + lc.ReturnDeliveryFactory_Approved as LogisticPerYearStdw_Approved

                , coalesce(case when afEx.Id is not null then af.Fee end, 0) as AvailabilityFee
                , coalesce(case when afEx.Id is not null then af.Fee_Approved end, 0) as AvailabilityFee_Approved

                , msw.MarkupFactorStandardWarranty , msw.MarkupFactorStandardWarranty_Approved  
                , msw.MarkupStandardWarranty       , msw.MarkupStandardWarranty_Approved        

        FROM Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN InputAtoms.Country c on c.id = m.CountryId

        INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        INNER JOIN InputAtoms.WgView wg2 on wg2.id = m.WgId

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId 

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Country = stdw.Country AND fsc.Wg = stdw.Wg AND fsc.ServiceLocation = stdw.ServiceLocationId AND fsc.ReactionTypeId = stdw.ReactionTypeId AND fsc.ReactionTimeId = stdw.ReactionTimeId

        LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = stdw.Country AND lc.Wg = stdw.Wg AND lc.ReactionTime = stdw.ReactionTimeId AND lc.ReactionType = stdw.ReactionTypeId

        LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp on   fsp.SlaHash = m.SlaHash 
                                                and fsp.CountryId = m.CountryId
                                                and fsp.WgId = m.WgId
                                                and fsp.AvailabilityId = m.AvailabilityId
                                                and fsp.DurationId = m.DurationId
                                                and fsp.ReactionTimeId = m.ReactionTimeId
                                                and fsp.ReactionTypeId = m.ReactionTypeId
                                                and fsp.ServiceLocationId = m.ServiceLocationId
                                                and fsp.ProactiveSlaId = m.ProActiveSlaId
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
                , 0     as LocalServiceStandardWarranty1P

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
                , 0     as LocalServiceStandardWarranty1P_Approved

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
         
            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.LocalServiceStandardWarranty1, m.LocalServiceStandardWarranty2, m.LocalServiceStandardWarranty3, m.LocalServiceStandardWarranty4, m.LocalServiceStandardWarranty5, m.LocalServiceStandardWarranty1P) as StandardWarranty
            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.LocalServiceStandardWarranty1_Approved, m.LocalServiceStandardWarranty2_Approved, m.LocalServiceStandardWarranty3_Approved, m.LocalServiceStandardWarranty4_Approved, m.LocalServiceStandardWarranty5_Approved, m.LocalServiceStandardWarranty1P_Approved) as StandardWarranty_Approved
    from CostCte2 m
)

go

