USE [Scd_2]
GO

sp_rename '[Hardware].[Reinsurance].[Year]', 'Duration';
GO

ALTER TABLE [Hardware].[Reinsurance] DROP CONSTRAINT [FK_HardwareReinsuranceYear_DependenciesYear]
GO

ALTER TABLE [Hardware].[Reinsurance]  WITH CHECK ADD  CONSTRAINT [FK_HardwareReinsuranceDuration_DependenciesDuration] FOREIGN KEY([Duration])
REFERENCES [Dependencies].[Duration] ([Id])
GO

ALTER TABLE [Hardware].[Reinsurance] CHECK CONSTRAINT [FK_HardwareReinsuranceDuration_DependenciesDuration]
GO

DROP TRIGGER [Hardware].[Reinsurance_Updated]
GO

CREATE TRIGGER [Hardware].[Reinsurance_Updated]
ON [Hardware].[Reinsurance]
After INSERT, UPDATE
AS BEGIN

    declare @NBD_9x5 bigint;
    declare @4h_9x5 bigint;
    declare @4h_24x7 bigint;

    select @NBD_9x5 = id 
    from Dependencies.ReactionTime_Avalability
    where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = 'NBD')
       and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')

    select @4h_9x5 = id 
    from Dependencies.ReactionTime_Avalability
    where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
       and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '9X5')

    select @4h_24x7 = id 
    from Dependencies.ReactionTime_Avalability
    where  ReactionTimeId = (select id from Dependencies.ReactionTime where UPPER(Name) = '4H')
       and AvailabilityId = (select id from Dependencies.Availability where UPPER(Name) = '24X7')

    TRUNCATE TABLE Hardware.ReinsuranceYear;

    -- Disable all table constraints
    ALTER TABLE Hardware.ReinsuranceYear NOCHECK CONSTRAINT ALL;

    INSERT INTO Hardware.ReinsuranceYear(
                      Wg
                
                    , ReinsuranceFlatfee1                     
                    , ReinsuranceFlatfee2                     
                    , ReinsuranceFlatfee3                     
                    , ReinsuranceFlatfee4                     
                    , ReinsuranceFlatfee5                     
                    , ReinsuranceFlatfeeP1                    
                
                    , ReinsuranceFlatfee1_Approved            
                    , ReinsuranceFlatfee2_Approved            
                    , ReinsuranceFlatfee3_Approved            
                    , ReinsuranceFlatfee4_Approved            
                    , ReinsuranceFlatfee5_Approved            
                    , ReinsuranceFlatfeeP1_Approved           
                
                    , ReinsuranceUpliftFactor_NBD_9x5         
                    , ReinsuranceUpliftFactor_4h_9x5          
                    , ReinsuranceUpliftFactor_4h_24x7         
                
                    , ReinsuranceUpliftFactor_NBD_9x5_Approved
                    , ReinsuranceUpliftFactor_4h_9x5_Approved 
                    , ReinsuranceUpliftFactor_4h_24x7_Approved
                )
    select   r.Wg

           , max(case when d.IsProlongation = 0 and d.Value = 1  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 2  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 3  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 4  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 0 and d.Value = 5  then ReinsuranceFlatfee end) 
           , max(case when d.IsProlongation = 1 and d.Value = 1  then ReinsuranceFlatfee end) 

           , max(case when d.IsProlongation = 0 and d.Value = 1  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 2  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 3  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 4  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 0 and d.Value = 5  then ReinsuranceFlatfee_Approved end) 
           , max(case when d.IsProlongation = 1 and d.Value = 1  then ReinsuranceFlatfee_Approved end) 

           , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor end) 
           , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor end) 
           , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor end) 

           , max(case when r.ReactionTimeAvailability = @NBD_9x5 then r.ReinsuranceUpliftFactor_Approved end) 
           , max(case when r.ReactionTimeAvailability = @4h_9x5  then r.ReinsuranceUpliftFactor_Approved end) 
           , max(case when r.ReactionTimeAvailability = @4h_24x7 then r.ReinsuranceUpliftFactor_Approved end) 

    from Hardware.Reinsurance r
    join Dependencies.Duration d on d.Id = r.Duration

    where r.ReactionTimeAvailability in (@NBD_9x5, @4h_9x5, @4h_24x7) 
      and r.DeactivatedDateTime is null
    group by r.Wg;

    -- Enable all table constraints
    ALTER TABLE Hardware.ReinsuranceYear CHECK CONSTRAINT ALL;

END
GO

ALTER TABLE [Hardware].[Reinsurance] ENABLE TRIGGER [Reinsurance_Updated]
GO

sp_rename '[History].[Hardware_Reinsurance].[Year]', 'Duration';
GO

ALTER TABLE [History].[Hardware_Reinsurance] DROP CONSTRAINT [FK_HistoryHardware_ReinsuranceYear_DependenciesYear]
GO

ALTER TABLE [History].[Hardware_Reinsurance]  WITH CHECK ADD  CONSTRAINT [FK_HistoryHardware_ReinsuranceDuration_DependenciesDuration] FOREIGN KEY([Duration])
REFERENCES [Dependencies].[Duration] ([Id])
GO

ALTER TABLE [History].[Hardware_Reinsurance] CHECK CONSTRAINT [FK_HistoryHardware_ReinsuranceDuration_DependenciesDuration]
GO

DROP VIEW [Hardware].[ReinsuranceView]
GO

CREATE VIEW [Hardware].[ReinsuranceView] as
    SELECT r.Wg, 
           r.Duration,
           rta.AvailabilityId, 
           rta.ReactionTimeId,

           r.ReinsuranceFlatfee * r.ReinsuranceUpliftFactor / 100 / er.Value as Cost,

           r.ReinsuranceFlatfee_Approved * r.ReinsuranceUpliftFactor_Approved / 100 / er2.Value as Cost_Approved

    FROM Hardware.Reinsurance r
    JOIN Dependencies.ReactionTime_Avalability rta on rta.Id = r.ReactionTimeAvailability
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance
    LEFT JOIN [References].ExchangeRate er2 on er2.CurrencyId = r.CurrencyReinsurance_Approved
GO

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

