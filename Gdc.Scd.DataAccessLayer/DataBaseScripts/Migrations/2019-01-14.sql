
DROP INDEX [IX_HwFspCodeTranslation_AvailabilityId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_CountryId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_DurationId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ProactiveSlaId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ReactionTimeId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ReactionTypeId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ServiceLocationId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_WgId] ON [Fsp].[HwFspCodeTranslation]
GO

ALTER TABLE Fsp.HwFspCodeTranslation 
    ADD SlaHash  AS checksum (
                        cast(coalesce(CountryId, 0) as nvarchar(20)) + ',' +
                        cast(WgId                   as nvarchar(20)) + ',' +
                        cast(AvailabilityId         as nvarchar(20)) + ',' +
                        cast(DurationId             as nvarchar(20)) + ',' +
                        cast(ReactionTimeId         as nvarchar(20)) + ',' +
                        cast(ReactionTypeId         as nvarchar(20)) + ',' +
                        cast(ServiceLocationId      as nvarchar(20)) + ',' +
                        cast(ProactiveSlaId         as nvarchar(20))
                    );
GO

CREATE INDEX IX_HwFspCodeTranslation_SlaHash ON Fsp.HwFspCodeTranslation(SlaHash);
GO

---------------------------------------------------------------------

ALTER TABLE Portfolio.LocalPortfolio
    ADD SlaHash  AS checksum (
                    cast(CountryId         as nvarchar(20)) + ',' +
                    cast(WgId              as nvarchar(20)) + ',' +
                    cast(AvailabilityId    as nvarchar(20)) + ',' +
                    cast(DurationId        as nvarchar(20)) + ',' +
                    cast(ReactionTimeId    as nvarchar(20)) + ',' +
                    cast(ReactionTypeId    as nvarchar(20)) + ',' +
                    cast(ServiceLocationId as nvarchar(20)) + ',' +
                    cast(ProactiveSlaId    as nvarchar(20)) 
                );

DROP INDEX [ix_Atom_InstallBase] ON [Hardware].[InstallBase]
GO

alter table Hardware.InstallBase  DROP COLUMN InstalledBaseCountryPla;
alter table Hardware.InstallBase  DROP COLUMN InstalledBaseCountryPla_Approved;
GO

alter table Hardware.InstallBase  
    ADD TotalIb float
      , TotalIb_Approved float
      , TotalIbClusterPla float
      , TotalIbClusterPla_Approved float;
GO  

CREATE NONCLUSTERED INDEX [ix_Atom_InstallBase] ON [Hardware].[InstallBase]
(
	[Country] ASC,
	[Wg] ASC
)
INCLUDE ( 
    InstalledBaseCountry,
    InstalledBaseCountry_Approved,
    TotalIb,
    TotalIb_Approved,
    TotalIbClusterPla,
    TotalIbClusterPla_Approved
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF OBJECT_ID('Hardware.InstallBaseUpdated', 'TR') IS NOT NULL
  DROP TRIGGER Hardware.InstallBaseUpdated;
go

CREATE TRIGGER [Hardware].[InstallBaseUpdated]
ON [Hardware].[InstallBase]
After INSERT, UPDATE
AS BEGIN

    with ibCte as (
        select ib.*, pla.Id as PlaId, cpla.Id as ClusterPlaId 
        from Hardware.InstallBase ib
        JOIN InputAtoms.Pla pla on pla.id = ib.Pla
        JOIN InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId
    )
    , totalIb_Cte as (
        select Country
             , sum(InstalledBaseCountry) as totalIb
             , sum(InstalledBaseCountry_Approved) as totalIb_Approved
        from ibCte
        group by Country
    )
    , totalIb_PLA_Cte as (
        select Country
             , ClusterPlaId
             , sum(InstalledBaseCountry) as totalIbCluster
             , sum(InstalledBaseCountry_Approved) as totalIbCluster_Approved
        from ibCte
        group by Country, ClusterPlaId
    )
    UPDATE ib
        SET 
              ib.TotalIb           = t1.totalIb
            , ib.TotalIb_Approved  = t1.totalIb_Approved
            , ib.TotalIbClusterPla = t2.totalIbCluster
            , ib.TotalIbClusterPla_Approved = t2.totalIbCluster_Approved
    from ibCte ib
    join totalIb_Cte t1 on t1.Country = ib.Country
    join totalIb_PLA_Cte t2 on t2.Country = ib.Country and t2.ClusterPlaId = ib.ClusterPlaId

END

go

update Hardware.InstallBase set InstalledBaseCountry = InstalledBaseCountry + 0;

go

ALTER VIEW [Hardware].[ServiceSupportCostView] as
    select ssc.Country,
           ssc.ClusterRegion,
           ssc.ClusterPla,

           ssc.[1stLevelSupportCostsCountry] * er.Value as '1stLevelSupportCosts',
           ssc.[1stLevelSupportCostsCountry_Approved] * er.Value as '1stLevelSupportCosts_Approved',

           (case 
                when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] * er.Value 
                else ssc.[2ndLevelSupportCostsClusterRegion]
            end) as '2ndLevelSupportCosts', 

           (case 
                when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] * er.Value 
                else ssc.[2ndLevelSupportCostsClusterRegion_Approved]
            end) as '2ndLevelSupportCosts_Approved'

    from Hardware.ServiceSupportCost ssc
    join InputAtoms.Country c on c.Id = ssc.Country
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
GO

ALTER FUNCTION [Portfolio].[GetBySlaPaging](
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
                    where   (m.CountryId = @cnt)
                        and (@wg is null or m.WgId = @wg)
                        and (@av is null or m.AvailabilityId = @av)
                        and (@dur is null or m.DurationId = @dur)
                        and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
                        and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
                        and (@loc is null or          m.ServiceLocationId = @loc)
                        and (@pro is null or          m.ProActiveSlaId = @pro)
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
            where   (m.CountryId = @cnt)
                and (@wg is null or m.WgId = @wg)
                and (@av is null or m.AvailabilityId = @av)
                and (@dur is null or m.DurationId = @dur)
                and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
                and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
                and (@loc is null or          m.ServiceLocationId = @loc)
                and (@pro is null or          m.ProActiveSlaId = @pro)

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

         , case when stdw.DurationValue is not null then stdw.DurationValue 
                when stdw2.DurationValue is not null then stdw2.DurationValue 
            end as StdWarranty

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

         , case when @approved = 0 then 
                    (case 
                         when ib.TotalIb <> 0 and ib.TotalIbClusterPla <> 0 
                            then ssc.[1stLevelSupportCosts] / ib.TotalIb + ssc.[2ndLevelSupportCosts] / ib.TotalIbClusterPla
                     end)
                else 
                    (case 
                         when ib.TotalIb_Approved <> 0 and ib.TotalIbClusterPla_Approved <> 0 
                            then ssc.[1stLevelSupportCosts_Approved] / ib.TotalIb_Approved + ssc.[2ndLevelSupportCosts_Approved] / ib.TotalIbClusterPla_Approved
                     end)
            end  as ServiceSupport
         
         , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved          end as ExpressDelivery         
         , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling
         , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved         end as StandardDelivery        
         , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved         end as StandardHandling        
         , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved    end as ReturnDeliveryFactory   
         , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved      end as TaxiCourierDelivery     

         , case when afEx.id is null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                 else 0
           end as AvailabilityFee

         , case when @approved = 0 then moc.Markup                         else moc.Markup_Approved                       end as Markup                      
         , case when @approved = 0 then moc.MarkupFactor                   else moc.MarkupFactor_Approved                 end as MarkupFactor                
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

    LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId --find local standard warranty portfolio
    LEFT JOIN Fsp.HwStandardWarrantyView stdw2 on stdw2.Wg = m.WgId and stdw2.Country is null    --find principle standard warranty portfolio, if local does not exist

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.HddRetention hdd on hdd.Wg = m.WgId AND hdd.Year = m.DurationId

    LEFT JOIN Hardware.InstallBase ib on ib.Wg = m.WgId AND ib.Country = m.CountryId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Year = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeId = m.ReactionTimeId AND moc.ReactionTypeId = m.ReactionTypeId AND moc.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCostView man on man.PortfolioId = m.Id
)
go

ALTER FUNCTION [Hardware].[GetCostsFull](
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
    with CostCte as (
        select    m.*
                , m.Year * m.ServiceSupport as ServiceSupportCost

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * (m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate as FieldServicePerYear

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
    , CostCte2_2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.TaxAndDuties * m.mat1 else 0 end as tax1
                , case when m.StdWarranty >= 2 then m.TaxAndDuties * m.mat2 else 0 end as tax2
                , case when m.StdWarranty >= 3 then m.TaxAndDuties * m.mat3 else 0 end as tax3
                , case when m.StdWarranty >= 4 then m.TaxAndDuties * m.mat4 else 0 end as tax4
                , case when m.StdWarranty >= 5 then m.TaxAndDuties * m.mat5 else 0 end as tax5
                , 0  as tax1P

                , case when m.StdWarranty >= 1 then 0 else m.TaxAndDuties * m.matO1 end as taxO1
                , case when m.StdWarranty >= 2 then 0 else m.TaxAndDuties * m.matO2 end as taxO2
                , case when m.StdWarranty >= 3 then 0 else m.TaxAndDuties * m.matO3 end as taxO3
                , case when m.StdWarranty >= 4 then 0 else m.TaxAndDuties * m.matO4 end as taxO4
                , case when m.StdWarranty >= 5 then 0 else m.TaxAndDuties * m.matO5 end as taxO5
                , m.matO1P * m.AFRP1 as taxO1P

                , m.mat1  + m.matO1                     as matCost1
                , m.mat2  + m.matO2                     as matCost2
                , m.mat3  + m.matO3                     as matCost3
                , m.mat4  + m.matO4                     as matCost4
                , m.mat5  + m.matO5                     as matCost5
                , m.mat1P + m.matO1P                    as matCost1P

                , m.TaxAndDuties * (m.mat1  + m.matO1)  as TaxAndDuties1
                , m.TaxAndDuties * (m.mat2  + m.matO2)  as TaxAndDuties2
                , m.TaxAndDuties * (m.mat3  + m.matO3)  as TaxAndDuties3
                , m.TaxAndDuties * (m.mat4  + m.matO4)  as TaxAndDuties4
                , m.TaxAndDuties * (m.mat5  + m.matO5)  as TaxAndDuties5
                , m.TaxAndDuties * (m.mat1P + m.matO1P) as TaxAndDuties1P

        from CostCte2 m
    )
    , CostCte3 as (
        select    m.*

                , Hardware.AddMarkup(m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.Reinsurance, m.MarkupFactor, m.Markup)  as OtherDirect1
                , Hardware.AddMarkup(m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.Reinsurance, m.MarkupFactor, m.Markup)  as OtherDirect2
                , Hardware.AddMarkup(m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.Reinsurance, m.MarkupFactor, m.Markup)  as OtherDirect3
                , Hardware.AddMarkup(m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.Reinsurance, m.MarkupFactor, m.Markup)  as OtherDirect4
                , Hardware.AddMarkup(m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.Reinsurance, m.MarkupFactor, m.Markup)  as OtherDirect5
                , Hardware.AddMarkup(m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.Reinsurance, m.MarkupFactor, m.Markup)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic1, m.tax1, m.AFR1, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) 
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic2, m.tax2, m.AFR2, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) 
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic3, m.tax3, m.AFR3, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) 
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic4, m.tax4, m.AFR4, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) 
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic5, m.tax5, m.AFR5, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) 
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
             , m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance + m.AvailabilityFee - m.Credit1  as ServiceTC1
             , m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance + m.AvailabilityFee - m.Credit2  as ServiceTC2
             , m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance + m.AvailabilityFee - m.Credit3  as ServiceTC3
             , m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance + m.AvailabilityFee - m.Credit4  as ServiceTC4
             , m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance + m.AvailabilityFee - m.Credit5  as ServiceTC5
             , m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance + m.AvailabilityFee - m.Credit1P as ServiceTC1P
        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
             , Hardware.AddMarkup(m.ServiceTC1, m.MarkupFactor, m.Markup) as ServiceTP1
             , Hardware.AddMarkup(m.ServiceTC2, m.MarkupFactor, m.Markup) as ServiceTP2
             , Hardware.AddMarkup(m.ServiceTC3, m.MarkupFactor, m.Markup) as ServiceTP3
             , Hardware.AddMarkup(m.ServiceTC4, m.MarkupFactor, m.Markup) as ServiceTP4
             , Hardware.AddMarkup(m.ServiceTC5, m.MarkupFactor, m.Markup) as ServiceTP5
             , Hardware.AddMarkup(m.ServiceTC1P, m.MarkupFactor, m.Markup) as ServiceTP1P
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

         , m.AvailabilityFee
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

         , m.SlaHash

       from CostCte6 m
)
GO

ALTER FUNCTION [Portfolio].[GetBySla](
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
RETURN 
(
    select m.*
        from Portfolio.LocalPortfolio m
        where   m.CountryId = @cnt
            and (@wg is null or m.WgId = @wg)
            and (@av is null or m.AvailabilityId = @av)
            and (@dur is null or m.DurationId = @dur)
            and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
            and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
            and (@loc is null or          m.ServiceLocationId = @loc)
            and (@pro is null or          m.ProActiveSlaId = @pro)
);
go

ALTER FUNCTION [Report].[GetCostsFull](
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
RETURN 
(
    select 
           fsp.Name as Fsp
         , fsp.ServiceDescription as FspDescription

         , m.*

    FROM Hardware.GetCostsFull(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, -1) m

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
go