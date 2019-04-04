IF OBJECT_ID('Fsp.HwStandardWarranty', 'U') IS NOT NULL
  DROP TABLE Fsp.HwStandardWarranty;
go

CREATE TABLE Fsp.HwStandardWarranty(
    Country bigint NOT NULL foreign key references InputAtoms.Country(Id)
  , Wg bigint NOT NULL foreign key references InputAtoms.Wg(Id)

  , FspId bigint NOT NULL foreign key references Fsp.HwFspCodeTranslation(Id)
  , Fsp nvarchar(255) NOT NULL

  , AvailabilityId bigint NOT NULL foreign key references Dependencies.Availability(Id)

  , DurationId bigint NOT NULL foreign key references Dependencies.Duration(Id)
  , Duration nvarchar(50)
  , IsProlongation bit
  , DurationValue int

  , ReactionTimeId bigint NOT NULL foreign key references Dependencies.ReactionTime(id)
  , ReactionTypeId bigint NOT NULL foreign key references Dependencies.ReactionType(id)

  , ServiceLocationId bigint NOT NULL foreign key references Dependencies.ServiceLocation(id)
  , ServiceLocation nvarchar(50)

  , ProActiveSlaId bigint NOT NULL foreign key references Dependencies.ProActiveSla(id)

  , ReactionTime_Avalability bigint
  , ReactionTime_ReactionType bigint
  , ReactionTime_ReactionType_Avalability bigint

  , CONSTRAINT PK_HwStandardWarranty PRIMARY KEY NONCLUSTERED (Country, Wg)
)
GO

ALTER TRIGGER [Fsp].[HwFspCodeTranslation_Updated]
ON [Fsp].[HwFspCodeTranslation]
After INSERT, UPDATE
AS BEGIN

    truncate table Fsp.HwStandardWarranty;

    -- Disable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty NOCHECK CONSTRAINT ALL;

    WITH StdCte AS (

    --remove duplicates in FSP
        SELECT 
              fsp.Id   as FspId
            , fsp.Name as Fsp
            , fsp.CountryId
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

        select   *
        from StdCte2 fsp
        where CountryId is not null
    )
    , Fsp2 as (
        
        --create default country FSP

        select    fsp.FspId
                , fsp.Fsp
                , c.Id as CountryId
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
                  coalesce(fsp.FspId              , fsp2.FspId            ) as FspId
                , coalesce(fsp.Fsp                , fsp2.Fsp              ) as Fsp
                , coalesce(fsp.CountryId          , fsp2.CountryId        ) as CountryId        
                , coalesce(fsp.WgId               , fsp2.WgId             ) as WgId             
                , coalesce(fsp.AvailabilityId     , fsp2.AvailabilityId   ) as AvailabilityId   
                , coalesce(fsp.DurationId         , fsp2.DurationId       ) as DurationId       
                , coalesce(fsp.ReactionTimeId     , fsp2.ReactionTimeId   ) as ReactionTimeId   
                , coalesce(fsp.ReactionTypeId     , fsp2.ReactionTypeId   ) as ReactionTypeId   
                , coalesce(fsp.ServiceLocationId  , fsp2.ServiceLocationId) as ServiceLocationId
                , coalesce(fsp.ProactiveSlaId     , fsp2.ProactiveSlaId   ) as ProactiveSlaId   
        from Fsp2 fsp2
        full join Fsp fsp on fsp.CountryId = fsp2.CountryId and fsp.WgId = fsp2.WgId
    )
    insert into Fsp.HwStandardWarranty(
                          Country
                        , Wg
                        , FspId
                        , Fsp
                        , AvailabilityId

                        , DurationId 
                        , Duration
                        , IsProlongation
                        , DurationValue 

                        , ReactionTimeId
                        , ReactionTypeId

                        , ServiceLocationId
                        , ServiceLocation

                        , ProactiveSlaId
                        , ReactionTime_Avalability              
                        , ReactionTime_ReactionType             
                        , ReactionTime_ReactionType_Avalability)
        select    fsp.CountryId
                , fsp.WgId
                , fsp.FspId
                , fsp.Fsp
                , fsp.AvailabilityId

                , fsp.DurationId
                , dur.Name
                , dur.IsProlongation
                , dur.Value as DurationValue

                , fsp.ReactionTimeId
                , fsp.ReactionTypeId

                , fsp.ServiceLocationId
                , loc.Name as ServiceLocation
        
                , fsp.ProactiveSlaId
                , rta.Id
                , rtt.Id
                , rtta.Id
        from StdFsp fsp
        INNER JOIN Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = fsp.AvailabilityId and rta.ReactionTimeId = fsp.ReactionTimeId
        INNER JOIN Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = fsp.ReactionTimeId and rtt.ReactionTypeId = fsp.ReactionTypeId
        INNER JOIN Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = fsp.AvailabilityId and rtta.ReactionTimeId = fsp.ReactionTimeId and rtta.ReactionTypeId = fsp.ReactionTypeId

        INNER JOIN Dependencies.Duration dur on dur.Id = fsp.DurationId
        INNER JOIN Dependencies.ServiceLocation loc on loc.id = fsp.ServiceLocationId;


    -- Enable all table constraints
    ALTER TABLE Fsp.HwStandardWarranty CHECK CONSTRAINT ALL;

END

go

update Fsp.HwFspCodeTranslation set CountryId = CountryId where Id < 100;
GO

ALTER FUNCTION [Hardware].[CalcStdw](
    @approved       bit = 0,
    @cnt            dbo.ListID READONLY,
    @wg             dbo.ListID READONLY
)
RETURNS @tbl TABLE  (
          CountryId                    bigint
        , Country                      nvarchar(255)
        , CurrencyId                   bigint
        , Currency                     nvarchar(255)
        , ClusterRegionId              bigint
        , ExchangeRate                 float

        , WgId                         bigint
        , Wg                           nvarchar(255)
        , SogId                        bigint
        , Sog                          nvarchar(255)
        , ClusterPlaId                 bigint
        , RoleCodeId                   bigint

        , StdFspId                     bigint
        , StdFsp                       nvarchar(255)

        , StdWarranty                  int
        , StdWarrantyLocation          nvarchar(255)

        , AFR1                         float 
        , AFR2                         float
        , AFR3                         float
        , AFR4                         float
        , AFR5                         float
        , AFRP1                        float

        , OnsiteHourlyRates            float

        --####### PROACTIVE COST ###################
        , LocalRemoteAccessSetup       float
        , LocalRegularUpdate           float
        , LocalPreparation             float
        , LocalRemoteCustomerBriefing  float
        , LocalOnsiteCustomerBriefing  float
        , Travel                       float
        , CentralExecutionReport       float

        , Fee                          float

        , MatW1                        float
        , MatW2                        float
        , MatW3                        float
        , MatW4                        float
        , MatW5                        float
        , MaterialW                    float

        , MatOow1                      float
        , MatOow2                      float
        , MatOow3                      float
        , MatOow4                      float
        , MatOow5                      float
        , MatOow1p                     float

        , MatCost1                     float
        , MatCost2                     float
        , MatCost3                     float
        , MatCost4                     float
        , MatCost5                     float
        , MatCost1P                    float

        , TaxW1                        float
        , TaxW2                        float
        , TaxW3                        float
        , TaxW4                        float
        , TaxW5                        float
        , TaxAndDutiesW                float

        , TaxOow1                      float
        , TaxOow2                      float
        , TaxOow3                      float
        , TaxOow4                      float
        , TaxOow5                      float
        , TaxOow1P                     float

        , TaxAndDuties1                float
        , TaxAndDuties2                float
        , TaxAndDuties3                float
        , TaxAndDuties4                float
        , TaxAndDuties5                float
        , TaxAndDuties1P               float

        , ServiceSupportPerYear        float
        , LocalServiceStandardWarranty float
        
        , Credit1                      float
        , Credit2                      float
        , Credit3                      float
        , Credit4                      float
        , Credit5                      float
        , Credits                      float
        
        , PRIMARY KEY CLUSTERED(CountryId, WgId)
    )
AS
BEGIN

    with WgCte as (
        select wg.Id as WgId
             , wg.Name as Wg
             , wg.SogId
             , sog.Name as Sog
             , pla.ClusterPlaId
             , RoleCodeId

             , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                           end as AFR1 
             , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                           end as AFR2 
             , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
             , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
             , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
             , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1

        from InputAtoms.Wg wg
        left join InputAtoms.Sog sog on sog.Id = wg.SogId
        left join InputAtoms.Pla pla on pla.id = wg.PlaId
        left join Hardware.AfrYear afr on afr.Wg = wg.Id
        where wg.WgType = 1 and wg.DeactivatedDateTime is null  and (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    )
    , CntCte as (
        select c.Id as CountryId
             , c.Name as Country
             , c.CurrencyId
             , cur.Name as Currency
             , c.ClusterRegionId
             , er.Value as ExchangeRate 
             , case when @approved = 0 then tax.TaxAndDuties_norm              else tax.TaxAndDuties_norm_Approved          end as TaxAndDuties

        from InputAtoms.Country c
        LEFT JOIN [References].Currency cur on cur.Id = c.CurrencyId
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = c.Id
        where exists(select * from @cnt where id = c.Id)
    )
    , WgCnt as (
        select c.*, wg.*
        from CntCte c, WgCte wg
    )
    , Std as (
        select  m.*

              , case when @approved = 0 then hr.OnsiteHourlyRates               else hr.OnsiteHourlyRates_Approved           end as OnsiteHourlyRates      

              , stdw.FspId                                    as StdFspId
              , stdw.Fsp                                      as StdFsp
              , stdw.AvailabilityId                           as StdAvailabilityId 
              , stdw.Duration                                 as StdDuration
              , stdw.DurationId                               as StdDurationId
              , stdw.DurationValue                            as StdDurationValue
              , stdw.IsProlongation                           as StdIsProlongation
              , stdw.ProActiveSlaId                           as StdProActiveSlaId
              , stdw.ReactionTime_Avalability                 as StdReactionTime_Avalability
              , stdw.ReactionTime_ReactionType                as StdReactionTime_ReactionType
              , stdw.ReactionTime_ReactionType_Avalability    as StdReactionTime_ReactionType_Avalability
              , stdw.ServiceLocation                          as StdServiceLocation
              , stdw.ServiceLocationId                        as StdServiceLocationId

              , case when @approved = 0 then mcw.MaterialCostIw                      else mcw.MaterialCostIw_Approved              end as MaterialCostWarranty
              , case when @approved = 0 then mcw.MaterialCostOow                     else mcw.MaterialCostOow_Approved                   end as MaterialCostOow     

              , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / m.ExchangeRate as MarkupStandardWarranty      
              , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end                  as MarkupFactorStandardWarranty

              --##### SERVICE SUPPORT COST #########                                                                                               
             , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]        else ssc.[1stLevelSupportCostsCountry_Approved]     end / m.ExchangeRate as [1stLevelSupportCosts] 
             , case when @approved = 0 
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion] end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                 end as [2ndLevelSupportCosts] 
             , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
             , case when @approved = 0
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                 end as TotalIbPla

              , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee

              --####### PROACTIVE COST ###################

              , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate   else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved end as LocalRemoteAccessSetup
              , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate             else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved           end as LocalRegularUpdate
              , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate                 else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved               end as LocalPreparation
              , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalRemoteCustomerBriefing
              , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalOnsiteCustomerBriefing
              , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate                            else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved                          end as Travel
              , case when @approved = 0 then pro.CentralExecutionShcReportCost                                    else pro.CentralExecutionShcReportCost_Approved                                           end as CentralExecutionReport

              --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0 then fscStd.LabourCost                  else fscStd.LabourCost_Approved              end / m.ExchangeRate as StdLabourCost             
              , case when @approved = 0 then fscStd.TravelCost                  else fscStd.TravelCost_Approved              end / m.ExchangeRate as StdTravelCost             
              , case when @approved = 0 then fstStd.PerformanceRate             else fstStd.PerformanceRate_Approved         end / m.ExchangeRate as StdPerformanceRate        

               --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0 then lcStd.ExpressDelivery              else lcStd.ExpressDelivery_Approved          end / m.ExchangeRate as StdExpressDelivery         
              , case when @approved = 0 then lcStd.HighAvailabilityHandling     else lcStd.HighAvailabilityHandling_Approved end / m.ExchangeRate as StdHighAvailabilityHandling
              , case when @approved = 0 then lcStd.StandardDelivery             else lcStd.StandardDelivery_Approved         end / m.ExchangeRate as StdStandardDelivery        
              , case when @approved = 0 then lcStd.StandardHandling             else lcStd.StandardHandling_Approved         end / m.ExchangeRate as StdStandardHandling        
              , case when @approved = 0 then lcStd.ReturnDeliveryFactory        else lcStd.ReturnDeliveryFactory_Approved    end / m.ExchangeRate as StdReturnDeliveryFactory   
              , case when @approved = 0 then lcStd.TaxiCourierDelivery          else lcStd.TaxiCourierDelivery_Approved      end / m.ExchangeRate as StdTaxiCourierDelivery     

        from WgCnt m

        LEFT JOIN Hardware.RoleCodeHourlyRates hr ON hr.Country = m.CountryId and hr.RoleCode = m.RoleCodeId and hr.DeactivatedDateTime is null

        LEFT JOIN Fsp.HwStandardWarranty stdw ON stdw.Wg = m.WgId and stdw.Country = m.CountryId

        LEFT JOIN Hardware.ServiceSupportCost ssc ON ssc.Country = m.CountryId and ssc.ClusterPla = m.ClusterPlaId and ssc.DeactivatedDateTime is null

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw ON mcw.Country = m.CountryId and mcw.Wg = m.WgId

        LEFT JOIN Hardware.MarkupStandardWaranty msw ON msw.Country = m.CountryId AND msw.Wg = m.WgId and msw.DeactivatedDateTime is null

        LEFT JOIN Hardware.AvailabilityFeeCalc af ON af.Country = m.CountryId AND af.Wg = m.WgId 

        LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId and pro.DeactivatedDateTime is null

        LEFT JOIN Hardware.FieldServiceCalc fscStd     ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId 
        LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType 

        LEFT JOIN Hardware.LogisticsCosts lcStd        ON lcStd.Country  = stdw.Country AND lcStd.Wg = stdw.Wg  AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType and lcStd.DeactivatedDateTime is null
    )
    , CostCte as (
        select    m.*

                , case when m.TaxAndDuties is null then 0 else m.TaxAndDuties end as TaxAndDutiesOrZero

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

                , m.StdLabourCost + m.StdTravelCost + coalesce(m.StdPerformanceRate, 0) as FieldServicePerYearStdw

                , m.StdStandardHandling + m.StdHighAvailabilityHandling + m.StdStandardDelivery + m.StdExpressDelivery + m.StdTaxiCourierDelivery + m.StdReturnDeliveryFactory as LogisticPerYearStdw

        from Std m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdDurationValue >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdDurationValue >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdDurationValue >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdDurationValue >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdDurationValue >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5

                , case when m.StdDurationValue >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdDurationValue >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdDurationValue >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdDurationValue >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdDurationValue >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1                                                  as matO1P

        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdDurationValue >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdDurationValue >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdDurationValue >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdDurationValue >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdDurationValue >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5

                , case when m.StdDurationValue >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdDurationValue >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdDurationValue >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdDurationValue >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdDurationValue >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5

        from CostCte2 m
    )
    , CostCte3 as (
        select   m.*

               , case when m.StdDurationValue >= 1 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty1
               , case when m.StdDurationValue >= 2 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty2
               , case when m.StdDurationValue >= 3 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty3
               , case when m.StdDurationValue >= 4 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty4
               , case when m.StdDurationValue >= 5 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                       else 0 
                   end as LocalServiceStandardWarranty5

        from CostCte2_2 m
    )
    insert into @tbl(
                 CountryId                    
               , Country                      
               , CurrencyId                   
               , Currency                     
               , ClusterRegionId              
               , ExchangeRate                 
               
               , WgId                         
               , Wg                           
               , SogId                        
               , Sog                          
               , ClusterPlaId                 
               , RoleCodeId                   

               , StdFspId
               , StdFsp  

               , StdWarranty         
               , StdWarrantyLocation 
               
               , AFR1                         
               , AFR2                         
               , AFR3                         
               , AFR4                         
               , AFR5                         
               , AFRP1                        

               , OnsiteHourlyRates

               , LocalRemoteAccessSetup     
               , LocalRegularUpdate         
               , LocalPreparation           
               , LocalRemoteCustomerBriefing
               , LocalOnsiteCustomerBriefing
               , Travel                     
               , CentralExecutionReport     
               
               , Fee                          

               , MatW1                
               , MatW2                
               , MatW3                
               , MatW4                
               , MatW5                
               , MaterialW            
               
               , MatOow1              
               , MatOow2              
               , MatOow3              
               , MatOow4              
               , MatOow5              
               , MatOow1p             
               
               , MatCost1             
               , MatCost2             
               , MatCost3             
               , MatCost4             
               , MatCost5             
               , MatCost1P            
               
               , TaxW1                
               , TaxW2                
               , TaxW3                
               , TaxW4                
               , TaxW5                
               , TaxAndDutiesW        
               
               , TaxOow1              
               , TaxOow2              
               , TaxOow3              
               , TaxOow4              
               , TaxOow5              
               , TaxOow1P             
               
               , TaxAndDuties1        
               , TaxAndDuties2        
               , TaxAndDuties3        
               , TaxAndDuties4        
               , TaxAndDuties5        
               , TaxAndDuties1P       

               , ServiceSupportPerYear
               , LocalServiceStandardWarranty 
               
               , Credit1                      
               , Credit2                      
               , Credit3                      
               , Credit4                      
               , Credit5                      
               , Credits                      
        )
    select    m.CountryId                    
            , m.Country                      
            , m.CurrencyId                   
            , m.Currency                     
            , m.ClusterRegionId              
            , m.ExchangeRate                 

            , m.WgId        
            , m.Wg          
            , m.SogId       
            , m.Sog         
            , m.ClusterPlaId
            , m.RoleCodeId  

            , m.StdFspId
            , m.StdFsp
            , m.StdDurationValue
            , m.StdServiceLocation

            , m.AFR1 
            , m.AFR2 
            , m.AFR3 
            , m.AFR4 
            , m.AFR5 
            , m.AFRP1

            , m.OnsiteHourlyRates

            , m.LocalRemoteAccessSetup     
            , m.LocalRegularUpdate         
            , m.LocalPreparation           
            , m.LocalRemoteCustomerBriefing
            , m.LocalOnsiteCustomerBriefing
            , m.Travel                     
            , m.CentralExecutionReport     

            , m.Fee

            , m.mat1                
            , m.mat2                
            , m.mat3                
            , m.mat4                
            , m.mat5                
            , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
            
            , m.matO1              
            , m.matO2              
            , m.matO3              
            , m.matO4              
            , m.matO5              
            , m.matO1P
            
            , m.mat1  + m.matO1  as matCost1
            , m.mat2  + m.matO2  as matCost2
            , m.mat3  + m.matO3  as matCost3
            , m.mat4  + m.matO4  as matCost4
            , m.mat5  + m.matO5  as matCost5
            , m.matO1P           as matCost1P
            
            , m.tax1                
            , m.tax2                
            , m.tax3                
            , m.tax4                
            , m.tax5                
            , m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
            
            , m.TaxAndDutiesOrZero * m.matO1              
            , m.TaxAndDutiesOrZero * m.matO2              
            , m.TaxAndDutiesOrZero * m.matO3              
            , m.TaxAndDutiesOrZero * m.matO4              
            , m.TaxAndDutiesOrZero * m.matO5              
            , m.TaxAndDutiesOrZero * m.matO1P             
            
            , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
            , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
            , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
            , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
            , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
            , m.TaxAndDutiesOrZero * m.matO1P as TaxAndDuties1P

            , m.ServiceSupportPerYear

            , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty

            , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
            , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
            , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
            , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
            , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5

            , m.mat1 + m.LocalServiceStandardWarranty1   +
                m.mat2 + m.LocalServiceStandardWarranty2 +
                m.mat3 + m.LocalServiceStandardWarranty3 +
                m.mat4 + m.LocalServiceStandardWarranty4 +
                m.mat5 + m.LocalServiceStandardWarranty5 as Credit

    from CostCte3 m;

    RETURN;
END
GO

if Report.GetReportFilterTypeByName('wgall', 1) is null insert into Report.ReportFilterType(Name, MultiSelect) values ('wgall', 1);

go

if not exists (select Id from Report.Report where upper(Name) = 'AFR') 
    insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) values ('AFR', 'AFR overview', 1,  1, 'Report.Afr');
go

IF OBJECT_ID('Report.Afr') IS NOT NULL
  DROP FUNCTION Report.Afr;
go 

CREATE FUNCTION Report.Afr
(
    @wg dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select  wg.Name as Wg
          , wg.Description as WgDescription
          , pla.Name as Pla
          , afr.AFR1_Approved * 100 as AFR1
          , afr.AFR2_Approved * 100 as AFR2
          , afr.AFR3_Approved * 100 as AFR3
          , afr.AFR4_Approved * 100 as AFR4
          , afr.AFR5_Approved * 100 as AFR5
    from Hardware.AfrYear afr
    join InputAtoms.Wg wg on wg.Id = afr.Wg and wg.DeactivatedDateTime is null
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId

    where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = afr.Wg))
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'AFR');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Pla', 'Warranty PLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR1', 'Failure Rate 1st year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR2', 'Failure Rate 2nd year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR3', 'Failure Rate 3rd year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR4', 'Failure Rate 4th year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR5', 'Failure Rate 5th year', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgall', 1), 'wg', 'Warranty Group');

GO

if not exists (select Id from Report.Report where upper(Name) = 'STANDARD-WARRANTY-OVERVIEW') 
    insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) values ('Standard-Warranty-overview', 'Standard Warranty overview', 1,  1, 'Report.StandardWarranty');
go

IF OBJECT_ID('Report.StandardWarranty') IS NOT NULL
  DROP FUNCTION Report.StandardWarranty;
go 

CREATE FUNCTION Report.StandardWarranty
(
    @cnt           bigint,
    @wg dbo.ListID readonly
)
RETURNS @tbl TABLE (
      Country                      nvarchar(255)
    , Pla                          nvarchar(255)
    , Wg                           nvarchar(255)
    , WgDescription                nvarchar(255)

    , Duration                     nvarchar(255)
    , Location                     nvarchar(255)

    , MaterialW                    float
    , MaterialAndTax               float
    , LocalServiceStandardWarranty float

    , TaxAndDuties                 float
)
AS
begin

    declare @cntTbl dbo.ListID;
    insert into @cntTbl(id) values (@cnt);

    insert into @tbl
    select std.Country
         , pla.Name as Pla
         , wg.Name as Wg
         , wg.Description as WgDescription

         , dur.Name as Duration
         , std.StdWarrantyLocation as Location

         , std.MaterialW
         , std.MaterialW * std.TaxAndDutiesW as MaterialAndTax
         , std.LocalServiceStandardWarranty

         , tax.TaxAndDuties_Approved as TaxAndDuties

    from Hardware.CalcStdw(1, @cntTbl, @wg) std
    join InputAtoms.Wg wg on wg.Id = std.WgId
    join InputAtoms.Pla pla on pla.Id = wg.PlaId
    join Dependencies.Duration dur on dur.Id = std.StdWarranty
    left join Hardware.TaxAndDuties tax on tax.Country = std.CountryId and tax.DeactivatedDateTime is null;

    return;

end
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'STANDARD-WARRANTY-OVERVIEW');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Pla', 'Warranty PLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Standard Warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Location', 'Standard Warranty service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'MaterialW', 'Material Costs w/o tax & duties per warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'MaterialAndTax', 'Material Cost with tax & duties per warrany duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'LocalServiceStandardWarranty', 'Standard Warranty Costs local (incl. tax & duties)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TaxAndDuties', 'Tax & Duties', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgstandard', 1), 'wg', 'Warranty Group');
