IF OBJECT_ID('Report.GetCosts') IS NOT NULL
  DROP FUNCTION Report.GetCosts;
go 

IF OBJECT_ID('Report.GetCostsFull') IS NOT NULL
  DROP FUNCTION Report.GetCostsFull;
go 

IF OBJECT_ID('Report.GetCalcMember') IS NOT NULL
  DROP FUNCTION Report.GetCalcMember;
go 

IF OBJECT_ID('Report.GetSwResultBySla') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla;
go 

IF OBJECT_ID('Report.GetSwResultBySla2') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla2;
go 

IF OBJECT_ID('Report.GetMatrixBySlaCountry') IS NOT NULL
  DROP FUNCTION Report.GetMatrixBySlaCountry;
go 

IF OBJECT_ID('Report.GetMatrixBySla') IS NOT NULL
  DROP FUNCTION Report.GetMatrixBySla;
go 

IF OBJECT_ID('SoftwareSolution.ServiceCostCalculationView', 'V') IS NOT NULL
  DROP VIEW SoftwareSolution.ServiceCostCalculationView;
go

IF OBJECT_ID('InputAtoms.CountryView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.CountryView;
go

IF OBJECT_ID('InputAtoms.WgSogView', 'V') IS NOT NULL
  DROP VIEW InputAtoms.WgSogView;
go

CREATE VIEW InputAtoms.CountryView WITH SCHEMABINDING AS
    select c.Id
         , c.Name
         , c.ISO3CountryCode
         , c.IsMaster
         , c.SAPCountryCode
         , cg.Id as CountryGroupId
         , cg.Name as CountryGroup
         , cg.LUTCode
         , cr.Id as ClusterRegionId
         , cr.Name as ClusterRegion
         , r.Id as RegionId
         , r.Name as Region
         , cur.Id as CurrencyId
         , cur.Name as Currency
    from InputAtoms.Country c
    left join InputAtoms.CountryGroup cg on cg.Id = c.CountryGroupId
    left join InputAtoms.Region r on r.Id = c.RegionId
    left join InputAtoms.ClusterRegion cr on cr.Id = c.ClusterRegionId
    left join [References].Currency cur on cur.Id = c.CurrencyId

GO

CREATE VIEW InputAtoms.WgSogView as 
    select wg.*
         , sog.Name as Sog
         , sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
GO

CREATE FUNCTION [Report].[GetMatrixBySla]
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint
)
RETURNS TABLE 
AS
RETURN (
    select m.*
    from Matrix.Matrix m
    where m.Denied = 0
      and (@cnt is null or m.CountryId = @cnt)
      and (@wg is null or m.WgId = @wg)
      and (@av is null or m.AvailabilityId = @av)
      and (@dur is null or m.DurationId = @dur)
      and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
      and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
      and (@loc is null or m.ServiceLocationId = @loc)
)
GO

CREATE FUNCTION [Report].[GetMatrixBySlaCountry]
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint
)
RETURNS TABLE 
AS
RETURN (
    select * from Report.GetMatrixBySla(coalesce(@cnt, -1), @wg, @av, @dur, @reactiontime, @reactiontype, @loc)
)
GO

CREATE view SoftwareSolution.ServiceCostCalculationView as
    select  sc.Year as YearId
          , y.Name as Year
          , y.Value as YearValue
          , sc.Availability as AvailabilityId
          , av.Name as Availability
          , sc.Sog as SogId
          , sog.Sog
          , sog.SogDescription
          , sog.Description
      
          , sc.DealerPrice_Approved as DealerPrice
          , sc.MaintenanceListPrice_Approved as MaintenanceListPrice
          , sc.Reinsurance_Approved as Reinsurance
          , sc.ServiceSupport_Approved as ServiceSupport
          , sc.TransferPrice_Approved as TransferPrice

    from SoftwareSolution.SwSpMaintenanceCostView sc
    join Dependencies.Availability av on av.Id = sc.Availability
    join Dependencies.Year y on y.id = sc.Year
    left join InputAtoms.WgSogView sog on sog.id = sc.Sog

GO

CREATE FUNCTION Report.GetSwResultBySla
(
    @sog bigint,
    @av bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select sc.*
    from SoftwareSolution.ServiceCostCalculationView sc
    where sc.SogId = @sog
      and (@av is null or sc.AvailabilityId = @av)
      and (@year is null or sc.YearId = @year)
)
GO

CREATE FUNCTION [Report].[GetCalcMember] (
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT m.Id

        --FSP
         , fsp.Name Fsp
         , fsp.ServiceDescription as FspDescription

        --SLA
         , m.CountryId
         , c.Name as Country
         , m.WgId
         , wg.Name as Wg
         , m.DurationId
         , dur.Name as Duration
         , dur.Value as Year
         , dur.IsProlongation
         , m.AvailabilityId
         , av.Name as Availability
         , m.ReactionTimeId
         , rtime.Name as ReactionTime
         , m.ReactionTypeId
         , rtype.Name as ReactionType
         , m.ServiceLocationId
         , loc.Name as ServiceLocation

         , AFR1_Approved       as AFR1 
         , AFR2_Approved       as AFR2 
         , afr.AFR3_Approved   as AFR3 
         , afr.AFR4_Approved   as AFR4 
         , afr.AFR5_Approved   as AFR5 
         , afr.AFRP1_Approved  as AFRP1
       
         , hdd.HddRet_Approved                  as HddRet              
         
         , mcw.MaterialCostWarranty_Approved    as MaterialCostWarranty
         , mco.MaterialCostOow_Approved         as MaterialCostOow     

         , mcw.MaterialCostWarranty_Approved * tax.TaxAndDuties_Approved as TaxAndDutiesW

         , mco.MaterialCostOow_Approved * tax.TaxAndDuties_Approved as TaxAndDutiesOow

         , r.Cost_Approved                      as Reinsurance
         , fsc.LabourCost_Approved              as LabourCost             
         , fsc.TravelCost_Approved              as TravelCost             
         , fsc.TimeAndMaterialShare_Approved    as TimeAndMaterialShare   
         , fsc.PerformanceRate_Approved         as PerformanceRate        
         , fsc.TravelTime_Approved              as TravelTime             
         , fsc.RepairTime_Approved              as RepairTime             
         , fsc.OnsiteHourlyRates_Approved       as OnsiteHourlyRates      
         
         , ssc.[1stLevelSupportCosts_Approved]  as [1stLevelSupportCosts] 
         , ssc.[2ndLevelSupportCosts_Approved]  as [2ndLevelSupportCosts] 
         , ib.InstalledBaseCountry_Approved     as InstalledBaseCountry    
         , ib.InstalledBaseCountryPla_Approved  as InstalledBaseCountryPla 

         , case 
                when ib.InstalledBaseCountry_Approved <> 0 and ib.InstalledBaseCountryPla_Approved <> 0 
                then ssc.[1stLevelSupportCosts_Approved] / ib.InstalledBaseCountry_Approved + ssc.[2ndLevelSupportCosts_Approved] / ib.InstalledBaseCountryPla_Approved
            end  as ServiceSupport
         
         , lc.ExpressDelivery_Approved          as ExpressDelivery         
         , lc.HighAvailabilityHandling_Approved as HighAvailabilityHandling
         , lc.StandardDelivery_Approved         as StandardDelivery        
         , lc.StandardHandling_Approved         as StandardHandling        
         , lc.ReturnDeliveryFactory_Approved    as ReturnDeliveryFactory   
         , lc.TaxiCourierDelivery_Approved      as TaxiCourierDelivery     

         , case 
                 when afEx.id is null then af.Fee_Approved
                 else 0
           end as AvailabilityFee

         , moc.Markup_Approved                       as Markup                      
         , moc.MarkupFactor_Approved                 as MarkupFactor                
         , msw.MarkupFactorStandardWarranty_Approved as MarkupFactorStandardWarranty
         , msw.MarkupStandardWarranty_Approved       as MarkupStandardWarranty      
         
         , pro.Setup_Approved + pro.Service_Approved * dur.Value as ProActive
         
         , man.ListPrice_Approved                    as ListPrice                   
         , man.DealerDiscount_Approved               as DealerDiscount              
         , man.DealerPrice_Approved                  as DealerPrice                 
         , man.ServiceTC_Approved                    as ServiceTCManual                   
         , man.ServiceTP_Approved                    as ServiceTPManual                   

    FROM Report.GetMatrixBySlaCountry(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc) m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.Wg wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.HddRetention hdd on hdd.Wg = m.WgId AND hdd.Year = m.DurationId

    LEFT JOIN Hardware.InstallBase ib on ib.Wg = m.WgId AND ib.Country = m.CountryId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.Wg = m.WgId

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Year = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeId = m.ReactionTimeId AND moc.ReactionTypeId = m.ReactionTypeId AND moc.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.AvailabilityFeeCalcView af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActiveView pro ON pro.Country = m.CountryId AND pro.Wg = m.WgId

    LEFT JOIN Hardware.ManualCost man on man.MatrixId = m.Id

    LEFT JOIN Fsp.HwFspCodeTranslation fsp on fsp.CountryId = m.CountryId
                                        and fsp.WgId = m.WgId
                                        and fsp.AvailabilityId = m.AvailabilityId
                                        and fsp.DurationId = m.DurationId
                                        and fsp.ReactionTimeId = m.ReactionTimeId
                                        and fsp.ReactionTypeId = m.ReactionTypeId
                                        and fsp.ServiceLocationId = m.ServiceLocationId
)
GO

CREATE FUNCTION Report.GetCostsFull(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint
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
        from Report.GetCalcMember(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc) m
    )
    , CostCte2 as (
        select    m.*

                , m.MaterialCostWarranty * m.AFR1 as mat1
                , m.MaterialCostWarranty * m.AFR2 as mat2
                , m.MaterialCostWarranty * m.AFR3 as mat3
                , m.MaterialCostWarranty * m.AFR4 as mat4
                , m.MaterialCostWarranty * m.AFR5 as mat5
                , m.MaterialCostWarranty * m.AFRP1 as mat1P

                , m.MaterialCostOow * m.AFR1 as matO1
                , m.MaterialCostOow * m.AFR2 as matO2
                , m.MaterialCostOow * m.AFR3 as matO3
                , m.MaterialCostOow * m.AFR4 as matO4
                , m.MaterialCostOow * m.AFR5 as matO5
                , m.MaterialCostOow * m.AFRP1 as matO1P

                , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYear * m.AFR1 as Logistic1
                , m.LogisticPerYear * m.AFR2 as Logistic2
                , m.LogisticPerYear * m.AFR3 as Logistic3
                , m.LogisticPerYear * m.AFR4 as Logistic4
                , m.LogisticPerYear * m.AFR5 as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte3 as (
        select    m.*
                , Hardware.AddMarkup(m.FieldServiceCost1 + m.ServiceSupport + 1 + m.Logistic1 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect1
                , Hardware.AddMarkup(m.FieldServiceCost2 + m.ServiceSupport + 1 + m.Logistic2 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect2
                , Hardware.AddMarkup(m.FieldServiceCost3 + m.ServiceSupport + 1 + m.Logistic3 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect3
                , Hardware.AddMarkup(m.FieldServiceCost4 + m.ServiceSupport + 1 + m.Logistic4 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect4
                , Hardware.AddMarkup(m.FieldServiceCost5 + m.ServiceSupport + 1 + m.Logistic5 + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect5
                , Hardware.AddMarkup(m.FieldServiceCost1P + m.ServiceSupport + 1 + m.Logistic1P + m.Reinsurance, m.MarkupFactor, m.Markup) as OtherDirect1P

                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic1, m.TaxAndDutiesW, m.AFR1, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty1
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic2, m.TaxAndDutiesW, m.AFR2, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty2
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic3, m.TaxAndDutiesW, m.AFR3, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty3
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic4, m.TaxAndDutiesW, m.AFR4, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty4
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic5, m.TaxAndDutiesW, m.AFR5, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty5
                , Hardware.CalcLocSrvStandardWarranty(m.LabourCost, m.TravelCost, m.ServiceSupport, m.Logistic1P, m.TaxAndDutiesW, m.AFRP1, m.AvailabilityFee, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty) as LocalServiceStandardWarranty1P

        from CostCte2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
             , m.mat1P + m.LocalServiceStandardWarranty1P as Credit1P
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*
             , m.FieldServiceCost1 + m.ServiceSupport + m.mat1 + m.Logistic1 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit1 as ServiceTC1
             , m.FieldServiceCost2 + m.ServiceSupport + m.mat2 + m.Logistic2 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit2 as ServiceTC2
             , m.FieldServiceCost3 + m.ServiceSupport + m.mat3 + m.Logistic3 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit3 as ServiceTC3
             , m.FieldServiceCost4 + m.ServiceSupport + m.mat4 + m.Logistic4 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit4 as ServiceTC4
             , m.FieldServiceCost5 + m.ServiceSupport + m.mat5 + m.Logistic5 + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit5 as ServiceTC5
             , m.FieldServiceCost1P + m.ServiceSupport + m.mat1P + m.Logistic1P + m.TaxAndDutiesW + m.Reinsurance + m.AvailabilityFee - m.Credit1P as ServiceTC1P
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

         , m.Fsp
         , m.FspDescription

         , m.CountryId
         , m.Country
         , m.WgId
         , m.Wg
         , m.DurationId
         , m.Duration
         , m.Year
         , m.IsProlongation
         , m.AvailabilityId
         , m.Availability
         , m.ReactionTimeId
         , m.ReactionTime
         , m.ReactionTypeId
         , m.ReactionType
         , m.ServiceLocationId
         , m.ServiceLocation

         , m.StandardHandling
         , m.HighAvailabilityHandling
         , m.StandardDelivery
         , m.ExpressDelivery
         , m.TaxiCourierDelivery
         , m.ReturnDeliveryFactory

         , m.AFR1 
         , m.AFR2 
         , m.AFR3 
         , m.AFR4 
         , m.AFR5 
         , m.AFRP1

         --Cost

         , m.AvailabilityFee
         , m.HddRet
         , m.TaxAndDutiesW
         , m.TaxAndDutiesOow
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

       from CostCte6 m
)
GO

CREATE FUNCTION [Report].[GetCosts](
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select Id

         , Fsp
         , FspDescription

         , CountryId
         , Country
         , WgId
         , Wg
         , DurationId
         , Duration
         , Year
         , IsProlongation
         , AvailabilityId
         , Availability
         , ReactionTimeId
         , ReactionTime
         , ReactionTypeId
         , ReactionType
         , ServiceLocationId
         , ServiceLocation

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

         , ListPrice
         , DealerDiscount
         , DealerPrice

         , coalesce(ServiceTCManual, ServiceTC) ServiceTC
         , coalesce(ServiceTPManual, ServiceTP) ServiceTP

    from Report.GetCostsFull(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc)
)