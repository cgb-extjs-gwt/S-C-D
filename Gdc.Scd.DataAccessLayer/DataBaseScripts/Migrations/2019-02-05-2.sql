IF OBJECT_ID('Hardware.MaterialCostOowCalc', 'U') IS NOT NULL
  DROP TABLE Hardware.MaterialCostOowCalc;
go

CREATE TABLE Hardware.MaterialCostOowCalc (
    [Country] [bigint] NOT NULL foreign key references InputAtoms.Country(Id),
    [Wg] [bigint] NOT NULL foreign key references InputAtoms.Wg(Id),
    [MaterialCostOow] [float] NULL,
    [MaterialCostOow_Approved] [float] NULL,
    CONSTRAINT PK_MaterialCostOowCalc PRIMARY KEY NONCLUSTERED (Country, Wg)
)
GO

IF OBJECT_ID('Hardware.SpUpdateMaterialCostOowCalc') IS NOT NULL
  DROP PROCEDURE Hardware.SpUpdateMaterialCostOowCalc;
go

CREATE PROCEDURE Hardware.SpUpdateMaterialCostOowCalc
AS
BEGIN

    SET NOCOUNT ON;

    truncate table Hardware.MaterialCostOowCalc;

    -- Disable all table constraints
    ALTER TABLE Hardware.MaterialCostOowCalc NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.MaterialCostOowCalc(Country, Wg, MaterialCostOow, MaterialCostOow_Approved)
        select NonEmeiaCountry as Country, Wg, MaterialCostOow, MaterialCostOow_Approved
        from Hardware.MaterialCostOow
        where DeactivatedDateTime is null

        union 

        select EmeiaCountry as Country, Wg, MaterialCostOow, MaterialCostOow_Approved
        from Hardware.MaterialCostOowEmeia
        where DeactivatedDateTime is null

    -- Enable all table constraints
    ALTER TABLE Hardware.MaterialCostOowCalc CHECK CONSTRAINT ALL;

END
go

IF OBJECT_ID('Hardware.MaterialCostOowUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.MaterialCostOowUpdated;
go

CREATE TRIGGER Hardware.MaterialCostOowUpdated
ON Hardware.MaterialCostOow
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostOowCalc;
END
go

IF OBJECT_ID('Hardware.MaterialCostOowEmeiaUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.MaterialCostOowEmeiaUpdated;
go

CREATE TRIGGER Hardware.MaterialCostOowEmeiaUpdated
ON Hardware.MaterialCostOowEmeia
After INSERT, UPDATE
AS BEGIN
    exec Hardware.SpUpdateMaterialCostOowCalc;
END
go

exec Hardware.SpUpdateMaterialCostOowCalc;
go

ALTER FUNCTION [Hardware].[GetCalcMember] (
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint,
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

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Year = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

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

             --FSP
              , fsp.Name Fsp
              , fsp.ServiceDescription as FspDescription

              --cost blocks

              , fsc.LabourCost_Approved as LabourCost
              , fsc.TravelCost_Approved as TravelCost
              , fsc.PerformanceRate_Approved as PerformanceRate
              , fsc.TravelTime_Approved as TravelTime
              , fsc.RepairTime_Approved as RepairTime
              , fsc.OnsiteHourlyRates_Approved as OnsiteHourlyRate

              , lc.StandardHandling_Approved as StandardHandling

              , lc.StandardHandling_Approved + 
                lc.HighAvailabilityHandling_Approved + 
                lc.StandardDelivery_Approved + 
                lc.ExpressDelivery_Approved + 
                lc.TaxiCourierDelivery_Approved + 
                lc.ReturnDeliveryFactory_Approved as LogisticPerYear

              , case when afEx.id is null then af.Fee_Approved else 0 end as AvailabilityFee
      
              , tax.TaxAndDuties_Approved as TaxAndDutiesW

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
                            fsc.TimeAndMaterialShare_Approved, 
                            fsc.TravelCost_Approved, 
                            fsc.LabourCost_Approved, 
                            fsc.PerformanceRate_Approved, 
                            fsc.TravelTime_Approved, 
                            fsc.RepairTime_Approved, 
                            fsc.OnsiteHourlyRates_Approved, 
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

        from Portfolio.GetBySla(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        INNER JOIN InputAtoms.WgView wg2 on wg2.Id = m.WgId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId 
                                                AND fsc.Country = m.CountryId 
                                                AND fsc.ServiceLocation = m.ServiceLocationId
                                                AND fsc.ReactionTypeId = m.ReactionTypeId
                                                AND fsc.ReactionTimeId = m.ReactionTimeId

        LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTime = m.ReactionTimeId
                                            AND lc.ReactionType = m.ReactionTypeId

        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN Hardware.ReinsuranceYear r on r.Wg = m.WgId

        LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId 
                                                   AND moc.Country = m.CountryId 
                                                   AND moc.ReactionTimeId = m.ReactionTimeId 
                                                   AND moc.ReactionTypeId = m.ReactionTypeId 
                                                   AND moc.AvailabilityId = m.AvailabilityId

        LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId 
                                                        AND msw.Country = m.CountryId 
                                                        AND msw.ReactionTimeId = m.ReactionTimeId 
                                                        AND msw.ReactionTypeId = m.ReactionTypeId 
                                                        AND msw.AvailabilityId = m.AvailabilityId

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId 
                                            AND afEx.ReactionTimeId = m.ReactionTimeId 
                                            AND afEx.ReactionTypeId = m.ReactionTypeId 
                                            AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                               and fsp.CountryId = m.CountryId
                                               and fsp.WgId = m.WgId
                                               and fsp.AvailabilityId = m.AvailabilityId
                                               and fsp.DurationId= m.DurationId
                                               and fsp.ReactionTimeId = m.ReactionTimeId
                                               and fsp.ReactionTypeId = m.ReactionTypeId
                                               and fsp.ServiceLocationId = m.ServiceLocationId
                                               and fsp.ProactiveSlaId = m.ProActiveSlaId
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
              , c.Currency

             --FSP
              , fsp.Name Fsp
              , fsp.ServiceDescription as FspDescription

              --cost blocks

              , fsc.LabourCost as LabourCost
              , fsc.TravelCost as TravelCost
              , fsc.PerformanceRate as PerformanceRate
              , fsc.TravelTime as TravelTime
              , fsc.RepairTime as RepairTime
              , fsc.OnsiteHourlyRates as OnsiteHourlyRate

              , lc.StandardHandling as StandardHandling

              , lc.StandardHandling + 
                lc.HighAvailabilityHandling + 
                lc.StandardDelivery + 
                lc.ExpressDelivery + 
                lc.TaxiCourierDelivery + 
                lc.ReturnDeliveryFactory as LogisticPerYear

              , case when afEx.id is null then af.Fee else 0 end as AvailabilityFee
      
              , tax.TaxAndDuties as TaxAndDutiesW

              , moc.Markup       as MarkupOtherCost
              , moc.MarkupFactor as MarkupFactorOtherCost

              , msw.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , msw.MarkupStandardWarranty       as MarkupStandardWarranty
      
              , afr.AFR1  as AFR1
              , afr.AFR2  as AFR2
              , afr.AFR3  as AFR3
              , afr.AFR4  as AFR4
              , afr.AFR5  as AFR5
              , afr.AFRP1 as AFRP1

              , Hardware.CalcFieldServiceCost(
                            fsc.TimeAndMaterialShare, 
                            fsc.TravelCost, 
                            fsc.LabourCost, 
                            fsc.PerformanceRate, 
                            fsc.TravelTime, 
                            fsc.RepairTime, 
                            fsc.OnsiteHourlyRates, 
                            1
                        ) as FieldServicePerYear

              , ssc.[1stLevelSupportCosts]           as [1stLevelSupportCosts]
              , ssc.[2ndLevelSupportCosts]           as [2ndLevelSupportCosts]
           
              , r.ReinsuranceFlatfee1                as ReinsuranceFlatfee1
              , r.ReinsuranceFlatfee2                as ReinsuranceFlatfee2
              , r.ReinsuranceFlatfee3                as ReinsuranceFlatfee3
              , r.ReinsuranceFlatfee4                as ReinsuranceFlatfee4
              , r.ReinsuranceFlatfee5                as ReinsuranceFlatfee5
              , r.ReinsuranceFlatfeeP1               as ReinsuranceFlatfeeP1
              , r.ReinsuranceUpliftFactor_4h_24x7    as ReinsuranceUpliftFactor_4h_24x7
              , r.ReinsuranceUpliftFactor_4h_9x5     as ReinsuranceUpliftFactor_4h_9x5
              , r.ReinsuranceUpliftFactor_NBD_9x5    as ReinsuranceUpliftFactor_NBD_9x5

              , mcw.MaterialCostWarranty as MaterialCostWarranty
              , mco.MaterialCostOow as MaterialCostOow

              , dur.Value as Duration
              , dur.IsProlongation

        from Portfolio.GetBySla(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        INNER JOIN InputAtoms.WgView wg2 on wg2.Id = m.WgId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId 
                                                AND fsc.Country = m.CountryId 
                                                AND fsc.ServiceLocation = m.ServiceLocationId
                                                AND fsc.ReactionTypeId = m.ReactionTypeId
                                                AND fsc.ReactionTimeId = m.ReactionTimeId

        LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTime = m.ReactionTimeId
                                            AND lc.ReactionType = m.ReactionTypeId

        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN Hardware.ReinsuranceYear r on r.Wg = m.WgId

        LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId 
                                                   AND moc.Country = m.CountryId 
                                                   AND moc.ReactionTimeId = m.ReactionTimeId 
                                                   AND moc.ReactionTypeId = m.ReactionTypeId 
                                                   AND moc.AvailabilityId = m.AvailabilityId

        LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId 
                                                        AND msw.Country = m.CountryId 
                                                        AND msw.ReactionTimeId = m.ReactionTimeId 
                                                        AND msw.ReactionTypeId = m.ReactionTypeId 
                                                        AND msw.AvailabilityId = m.AvailabilityId

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId 
                                            AND afEx.ReactionTimeId = m.ReactionTimeId 
                                            AND afEx.ReactionTypeId = m.ReactionTypeId 
                                            AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                               and fsp.CountryId = m.CountryId
                                               and fsp.WgId = m.WgId
                                               and fsp.AvailabilityId = m.AvailabilityId
                                               and fsp.DurationId= m.DurationId
                                               and fsp.ReactionTimeId = m.ReactionTimeId
                                               and fsp.ReactionTypeId = m.ReactionTypeId
                                               and fsp.ServiceLocationId = m.ServiceLocationId
                                               and fsp.ProactiveSlaId = m.ProActiveSlaId
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


