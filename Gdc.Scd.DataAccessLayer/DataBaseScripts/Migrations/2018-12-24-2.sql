IF OBJECT_ID('Report.GetCosts') IS NOT NULL
  DROP FUNCTION Report.GetCosts;
go 

IF OBJECT_ID('Report.GetCostsFull') IS NOT NULL
  DROP FUNCTION Report.GetCostsFull;
go 

IF OBJECT_ID('Report.GetSwResultBySla') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla;
go 

IF OBJECT_ID('Report.GetSwResultBySla2') IS NOT NULL
  DROP FUNCTION Report.GetSwResultBySla2;
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
RETURNS TABLE 
AS
RETURN 
(
    select 
           fsp.Name as Fsp
         , fsp.ServiceDescription as FspDescription

         , m.*

    FROM Hardware.GetCostsFull(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, -1) m
    LEFT JOIN Fsp.HwFspCodeTranslation fsp on fsp.CountryId = m.CountryId
                                   and fsp.WgId = m.WgId
                                   and fsp.AvailabilityId = m.AvailabilityId
                                   and fsp.DurationId = m.DurationId
                                   and fsp.ReactionTimeId = m.ReactionTimeId
                                   and fsp.ReactionTypeId = m.ReactionTypeId
                                   and fsp.ServiceLocationId = m.ServiceLocationId
                                   and fsp.ProactiveSlaId = m.ProActiveSlaId
)
go

CREATE FUNCTION [Report].[GetCosts](
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
         , ProActiveSlaId
         , ProActiveSla

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

    FROM Report.GetCostsFull(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
)


go

---------------------------------------------------------------------------------------------------------


IF OBJECT_ID('Report.CalcOutputNewVsOld') IS NOT NULL
  DROP FUNCTION Report.CalcOutputNewVsOld;
go 

CREATE FUNCTION Report.CalcOutputNewVsOld
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

    FROM Hardware.GetCostsFull(0, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, -1) m --not approved

    INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

    LEFT JOIN Fsp.HwFspCodeTranslation fsp on fsp.CountryId = m.CountryId
                                   and fsp.WgId = m.WgId
                                   and fsp.AvailabilityId = m.AvailabilityId
                                   and fsp.DurationId = m.DurationId
                                   and fsp.ReactionTimeId = m.ReactionTimeId
                                   and fsp.ReactionTypeId = m.ReactionTypeId
                                   and fsp.ServiceLocationId = m.ServiceLocationId
                                   and fsp.ProactiveSlaId = m.ProActiveSlaId
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCOUTPUT-NEW-VS-OLD');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Portfolio Alignment', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceProduct', 'Service Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'StandardWarranty', 'NEW - Service standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'StandardWarrantyOld', 'OLD - Service standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'Bw', 'b/w %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

GO
IF OBJECT_ID('Report.CalcOutputVsFREEZE') IS NOT NULL
  DROP FUNCTION Report.CalcOutputVsFREEZE;
go 

CREATE FUNCTION Report.CalcOutputVsFREEZE
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
        SELECT m.Id

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

             , case when stdw.DurationValue is not null then stdw.DurationValue 
                    when stdw2.DurationValue is not null then stdw2.DurationValue 
                end as StdWarranty

             , afr.AFR1 , AFR1_Approved
             , afr.AFR2, AFR2_Approved       
             , afr.AFR3, afr.AFR3_Approved   
             , afr.AFR4, afr.AFR4_Approved   
             , afr.AFR5, afr.AFR5_Approved   
             , afr.AFRP1, afr.AFRP1_Approved

             , mcw.MaterialCostWarranty, mcw.MaterialCostWarranty_Approved

             , tax.TaxAndDuties, tax.TaxAndDuties_Approved

             , fsc.LabourCost             , fsc.LabourCost_Approved              
             , fsc.TravelCost             , fsc.TravelCost_Approved              
             , fsc.TimeAndMaterialShare   , fsc.TimeAndMaterialShare_Approved    
             , fsc.PerformanceRate        , fsc.PerformanceRate_Approved         
             , fsc.TravelTime             , fsc.TravelTime_Approved              
             , fsc.RepairTime             , fsc.RepairTime_Approved              
             , fsc.OnsiteHourlyRates      , fsc.OnsiteHourlyRates_Approved       

             , Hardware.CalcSrvSupportCost(ssc.[1stLevelSupportCosts], ssc.[2ndLevelSupportCosts], ib.InstalledBaseCountry, ib.InstalledBaseCountryPla) as ServiceSupport
             , Hardware.CalcSrvSupportCost(ssc.[1stLevelSupportCosts_Approved], ssc.[2ndLevelSupportCosts_Approved], ib.InstalledBaseCountry_Approved, ib.InstalledBaseCountryPla_Approved) as ServiceSupport_Approved

             , lc.StandardHandling + lc.HighAvailabilityHandling + lc.StandardDelivery + lc.ExpressDelivery + lc.TaxiCourierDelivery + lc.ReturnDeliveryFactory as LogisticPerYear
             , lc.StandardHandling_Approved + lc.HighAvailabilityHandling_Approved + lc.StandardDelivery_Approved + lc.ExpressDelivery_Approved + lc.TaxiCourierDelivery_Approved + lc.ReturnDeliveryFactory_Approved as LogisticPerYear_Approved

             , case when afEx.id is null then af.Fee          else 0 end as AvailabilityFee
             , case when afEx.id is null then af.Fee_Approved else 0 end as AvailabilityFee_Approved

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

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId --find local standard warranty portfolio
        LEFT JOIN Fsp.HwStandardWarrantyView stdw2 on stdw2.Wg = m.WgId and stdw2.Country is null    --find principle standard warranty portfolio, if local does not exist

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        LEFT JOIN Hardware.InstallBase ib on ib.Wg = m.WgId AND ib.Country = m.CountryId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

        LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

        LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

        LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp on fsp.CountryId = m.CountryId
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
                , 0  as tax1P

                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR1_Approved as tax1_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR2_Approved as tax2_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR3_Approved as tax3_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR4_Approved as tax4_Approved
                , m.TaxAndDuties_Approved * m.MaterialCostWarranty_Approved * m.AFR5_Approved as tax5_Approved
                , 0  as tax1P_Approved

                , m.LogisticPerYear * m.AFR1 as Logistic1
                , m.LogisticPerYear * m.AFR2 as Logistic2
                , m.LogisticPerYear * m.AFR3 as Logistic3
                , m.LogisticPerYear * m.AFR4 as Logistic4
                , m.LogisticPerYear * m.AFR5 as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

                , m.LogisticPerYear_Approved * m.AFR1_Approved   as Logistic1_Approved
                , m.LogisticPerYear_Approved * m.AFR2_Approved   as Logistic2_Approved
                , m.LogisticPerYear_Approved * m.AFR3_Approved   as Logistic3_Approved
                , m.LogisticPerYear_Approved * m.AFR4_Approved   as Logistic4_Approved
                , m.LogisticPerYear_Approved * m.AFR5_Approved   as Logistic5_Approved
                , m.LogisticPerYear_Approved * m.AFRP1_Approved  as Logistic1P_Approved

        from cte m
    )
    , CostCte2 as (
        select    m.*

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

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, m.Logistic1_Approved, m.tax1_Approved, m.AFR1_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved) 
                        else 0 
                    end as LocalServiceStandardWarranty1_Approved
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, m.Logistic2_Approved, m.tax2_Approved, m.AFR2_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved) 
                        else 0 
                    end as LocalServiceStandardWarranty2_Approved
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, m.Logistic3_Approved, m.tax3_Approved, m.AFR3_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved) 
                        else 0 
                    end as LocalServiceStandardWarranty3_Approved
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, m.Logistic4_Approved, m.tax4_Approved, m.AFR4_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved) 
                        else 0 
                    end as LocalServiceStandardWarranty4_Approved
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.LabourCost_Approved, m.TravelCost_Approved, m.ServiceSupport_Approved, m.Logistic5_Approved, m.tax5_Approved, m.AFR5_Approved, m.AvailabilityFee_Approved, m.MarkupFactorStandardWarranty_Approved, m.MarkupStandardWarranty_Approved) 
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

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCOUTPUT-VS-FREEZE');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Portfolio Alignment', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceProduct', 'Service Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'StandardWarranty', 'Not approved standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'StandardWarranty_Approved', 'Approved standard warranty', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

GO
IF OBJECT_ID('Report.CalcParameterHwNotApproved') IS NOT NULL
  DROP FUNCTION Report.CalcParameterHwNotApproved;
go 

CREATE FUNCTION Report.CalcParameterHwNotApproved
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
    with ReinsuranceCte as (
        select r.Wg
             , r.Year
             , SUM(CASE WHEN UPPER(rt.Name) = 'NBD' THEN r.Cost END) AS  ReinsuranceNBD
             , SUM(CASE WHEN UPPER(av.Name) = '24X7' THEN r.Cost END) AS Reinsurance27x7
        from Hardware.ReinsuranceView r
        join Dependencies.Availability av on av.Id = r.AvailabilityId 
        join Dependencies.ReactionTime rt on rt.id = r.ReactionTimeId
        group by r.Wg, r.Year
    )
    , CostCte as (
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
              , null as OohUplift
              , null as Uplift

              , lc.StandardHandling as StandardHandling
              , null as LogisticTransportcost

              , null as FslFlatfee
      
              , mcw.MaterialCostWarranty * tax.TaxAndDuties as TaxAndDutiesW
              , mco.MaterialCostOow      * tax.TaxAndDuties as TaxAndDutiesOow

              , null as Markup
              , null as MarkupIndirect
              , null as MarkupFactor
              , null as MarkupBaseW
      
              , afr.AFR1 as AFR1
              , afr.AFR2 as AFR2
              , afr.AFR3 as AFR3
              , afr.AFR4 as AFR4
              , afr.AFR5 as AFR5

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

              , null as '2ndLevelRatio'
              , null as '2ndLevelSupportRate'
              , ssc.[1stLevelSupportCosts]
              , null as OohUpliftSsc
           
              , r.ReinsuranceNBD
              , r.ReinsuranceNBD as ReinsuranceNBD_Oow
              , r.Reinsurance27x7
              , r.Reinsurance27x7 as Reinsurance27x7_Oow
           
              , null as MaterialPerIncident
           
              , mcw.MaterialCostWarranty as MaterialCostWarranty
              , mco.MaterialCostOow as MaterialCostOow
           
              , null as OnSiteInterventions
           
              , dur.Value as Duration
           
              , null as CallRate
              , null as GlobalProduct
           
              , null as DealerPrice
              , null as ListPrice
           
              , null as SparesAvailability
              , null as UsageFtsLogistics
              , null as ReinsuranceCalcMode
              , null as ReinsuranceContract

        from Portfolio.GetBySla(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        JOIN InputAtoms.WgView wg2 on wg2.Id = m.WgId

        JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

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

        LEFT JOIN Hardware.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN ReinsuranceCte r on r.Wg = m.WgId and r.Year = m.DurationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp on fsp.CountryId = m.CountryId
                                    and fsp.WgId = m.WgId
                                    and fsp.AvailabilityId = m.AvailabilityId
                                    and fsp.DurationId = m.DurationId
                                    and fsp.ReactionTimeId = m.ReactionTimeId
                                    and fsp.ReactionTypeId = m.ReactionTypeId
                                    and fsp.ServiceLocationId = m.ServiceLocationId
                                    and fsp.ProactiveSlaId = m.ProActiveSlaId
    )
    select    m.*
            , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
            , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
            , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
            , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
            , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
    from CostCte m
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCULATION-PARAMETER-HW-NOT-APPROVED');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Sales Product Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SCD_ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sla', 'SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Local Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'G_MATNR', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FspDescription', 'G_MAKTX', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'LabourCost', 'Labour cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TravelCost', 'Travel cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PerformanceRate', 'Performance rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TravelTime', 'Travel time (MTTT)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'RepairTime', 'Repair time (MTTR)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OnsiteHourlyRate', 'Onsite hourly rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OohUplift', 'OOH Uplift Field Service Cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Uplift', 'Uplift Field Service Cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardHandling', 'Logistics handling cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'LogisticTransportcost', 'Logistics transport cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FslFlatfee', 'FSL flatfee', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxAndDutiesW', 'Customs tax and duty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxAndDutiesOow', 'Customs tax and duty OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Markup', 'Markp for Opex, Interest & Other', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MarkupIndirect', 'Markup for indirect costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MarkupFactor', 'Markup factor for other direct cost and contingency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MarkupBaseW', 'Markup for base warranty local cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR1', 'AFR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR2', 'AFR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR3', 'AFR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR4', 'AFR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR5', 'AFR5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost1', 'Calculated Field Service Cost 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost2', 'Calculated Field Service Cost 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost3', 'Calculated Field Service Cost 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost4', 'Calculated Field Service Cost 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost5', 'Calculated Field Service Cost 5 years', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, '2ndLevelRatio', '2nd level ratio', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, '2ndLevelSupportRate', '2nd level support rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, '1stLevelSupportCosts', '1st level Service Desk cost (5x9)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OohUpliftSsc', 'OOH Uplift SSC', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceNBD', 'Reinsurance NBD', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceNBD_Oow', 'Reinsurance NBD OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Reinsurance27x7', 'Reinsurance 7x24', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Reinsurance27x7_Oow', 'Reinsurance 7x24 OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MaterialPerIncident', 'Material quantity per incident', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MaterialCostWarranty', 'Material Cost BaseW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MaterialCostOow', 'Material cost OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OnSiteInterventions', 'On-site interventions with spare parts usage in %', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Warranty duration', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CallRate', 'Call rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'GlobalProduct', 'Global Product', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'DealerPrice', 'Markup for Margin (Dealer Price)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ListPrice', 'Markup for Margin (List Price)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SparesAvailability', 'Spares availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'UsageFtsLogistics', 'Usage of FTS Logistics', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceCalcMode', 'Calculation mode for reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceContract', 'Reinsurance Contract', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');



IF OBJECT_ID('Report.CalcParameterHw') IS NOT NULL
  DROP FUNCTION Report.CalcParameterHw;
go 

CREATE FUNCTION Report.CalcParameterHw
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
    with ReinsuranceCte as (
        select r.Wg
             , r.Year
             , SUM(CASE WHEN UPPER(rt.Name) = 'NBD' THEN r.Cost_Approved END) AS  ReinsuranceNBD
             , SUM(CASE WHEN UPPER(av.Name) = '24X7' THEN r.Cost_Approved END) AS Reinsurance27x7
        from Hardware.ReinsuranceView r
        join Dependencies.Availability av on av.Id = r.AvailabilityId 
        join Dependencies.ReactionTime rt on rt.id = r.ReactionTimeId
        group by r.Wg, r.Year
    )
    , CostCte as (
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
              , null as OohUplift
              , null as Uplift

              , lc.StandardHandling_Approved as StandardHandling
              , null as LogisticTransportcost

              , null as FslFlatfee
      
              , mcw.MaterialCostWarranty_Approved * tax.TaxAndDuties_Approved as TaxAndDutiesW
              , mco.MaterialCostOow_Approved      * tax.TaxAndDuties_Approved as TaxAndDutiesOow

              , null as Markup
              , null as MarkupIndirect
              , null as MarkupFactor
              , null as MarkupBaseW
      
              , afr.AFR1_Approved as AFR1
              , afr.AFR2_Approved as AFR2
              , afr.AFR3_Approved as AFR3
              , afr.AFR4_Approved as AFR4
              , afr.AFR5_Approved as AFR5

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

              , null as '2ndLevelRatio'
              , null as '2ndLevelSupportRate'
              , ssc.[1stLevelSupportCosts]
              , null as OohUpliftSsc
           
              , r.ReinsuranceNBD
              , r.ReinsuranceNBD as ReinsuranceNBD_Oow
              , r.Reinsurance27x7
              , r.Reinsurance27x7 as Reinsurance27x7_Oow
           
              , null as MaterialPerIncident
           
              , mcw.MaterialCostWarranty_Approved as MaterialCostWarranty
              , mco.MaterialCostOow_Approved as MaterialCostOow
           
              , null as OnSiteInterventions
           
              , dur.Value as Duration
           
              , null as CallRate
              , null as GlobalProduct
           
              , null as DealerPrice
              , null as ListPrice
           
              , null as SparesAvailability
              , null as UsageFtsLogistics
              , null as ReinsuranceCalcMode
              , null as ReinsuranceContract

        from Portfolio.GetBySla(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        JOIN InputAtoms.WgView wg2 on wg2.Id = m.WgId

        JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

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

        LEFT JOIN Hardware.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN ReinsuranceCte r on r.Wg = m.WgId and r.Year = m.DurationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp on fsp.CountryId = m.CountryId
                                    and fsp.WgId = m.WgId
                                    and fsp.AvailabilityId = m.AvailabilityId
                                    and fsp.DurationId = m.DurationId
                                    and fsp.ReactionTimeId = m.ReactionTimeId
                                    and fsp.ReactionTypeId = m.ReactionTypeId
                                    and fsp.ServiceLocationId = m.ServiceLocationId
                                    and fsp.ProactiveSlaId = m.ProActiveSlaId
    )
    select    m.*
            , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
            , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
            , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
            , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
            , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
    from CostCte m
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCULATION-PARAMETER-HW');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Sales Product Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SCD_ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sla', 'SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Local Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'G_MATNR', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FspDescription', 'G_MAKTX', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'LabourCost', 'Labour cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TravelCost', 'Travel cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PerformanceRate', 'Performance rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TravelTime', 'Travel time (MTTT)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'RepairTime', 'Repair time (MTTR)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OnsiteHourlyRate', 'Onsite hourly rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OohUplift', 'OOH Uplift Field Service Cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Uplift', 'Uplift Field Service Cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardHandling', 'Logistics handling cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'LogisticTransportcost', 'Logistics transport cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FslFlatfee', 'FSL flatfee', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxAndDutiesW', 'Customs tax and duty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxAndDutiesOow', 'Customs tax and duty OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Markup', 'Markp for Opex, Interest & Other', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MarkupIndirect', 'Markup for indirect costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MarkupFactor', 'Markup factor for other direct cost and contingency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MarkupBaseW', 'Markup for base warranty local cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR1', 'AFR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR2', 'AFR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR3', 'AFR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR4', 'AFR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR5', 'AFR5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost1', 'Calculated Field Service Cost 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost2', 'Calculated Field Service Cost 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost3', 'Calculated Field Service Cost 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost4', 'Calculated Field Service Cost 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'FieldServiceCost5', 'Calculated Field Service Cost 5 years', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, '2ndLevelRatio', '2nd level ratio', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, '2ndLevelSupportRate', '2nd level support rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, '1stLevelSupportCosts', '1st level Service Desk cost (5x9)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OohUpliftSsc', 'OOH Uplift SSC', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceNBD', 'Reinsurance NBD', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceNBD_Oow', 'Reinsurance NBD OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Reinsurance27x7', 'Reinsurance 7x24', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Reinsurance27x7_Oow', 'Reinsurance 7x24 OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MaterialPerIncident', 'Material quantity per incident', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MaterialCostWarranty', 'Material Cost BaseW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'MaterialCostOow', 'Material cost OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'OnSiteInterventions', 'On-site interventions with spare parts usage in %', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Warranty duration', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CallRate', 'Call rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'GlobalProduct', 'Global Product', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'DealerPrice', 'Markup for Margin (Dealer Price)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ListPrice', 'Markup for Margin (List Price)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SparesAvailability', 'Spares availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'UsageFtsLogistics', 'Usage of FTS Logistics', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceCalcMode', 'Calculation mode for reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReinsuranceContract', 'Reinsurance Contract', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');



IF OBJECT_ID('Report.CalcParameterProActive') IS NOT NULL
  DROP FUNCTION Report.CalcParameterProActive;
go 

CREATE FUNCTION Report.CalcParameterProActive
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select c.Name AS Country
         , wg.Description as WgDescription
         , wg.Name as Wg
         , wg.SCD_ServiceType as ServiceType
         , 'EUR' as Currency
         , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteSetup
         , pro.LocalRemoteShcCustomerBriefingEffort_Approved as LocalRemoteShc
         , pro.LocalOnSiteShcCustomerBriefingEffort_Approved as LocalOnsiteShc
         , pro.LocalRegularUpdateReadyEffort_Approved as LocalRegularUpdate
         , pro.LocalPreparationShcEffort_Approved as LocalPreparationShc
         , pro.TravellingTime_Approved as TravellingTime
         , pro.OnSiteHourlyRate_Approved as OnSiteHourlyRate
         , null as CentralSetup
         , pro.CentralExecutionShcReportCost_Approved as CentralShc

         , wg.Sog

    from Hardware.ProActive pro
    join InputAtoms.Country c on c.id = pro.Country
    join InputAtoms.WgSogView wg on wg.Id = pro.Wg

    where pro.Country = @cnt
      and (@wg is null or pro.Wg = @wg)
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCULATION-PARAMETER-PROACTIVE');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalRemoteSetup', 'Local Remote-Access setup preparation', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalRemoteShc', 'Local remote SHC customer briefing', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalOnsiteShc', 'Local onsite SHC customer briefing', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalRegularUpdate', 'Local regular update ready for service', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalPreparationShc', 'Local preparation SHC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'TravellingTime', 'Travelling Time (MTTT)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'OnSiteHourlyRate', 'Onsite hourly rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'CentralSetup', 'Setup Central Remote-Access & AutoCall', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'CentralShc', 'Central execution SHC & report', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');

GO



IF OBJECT_ID('Report.Contract') IS NOT NULL
  DROP FUNCTION Report.Contract;
go 

CREATE FUNCTION Report.Contract
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

         , m.ServiceTP1 AS ServiceTP1
         , m.ServiceTP2 AS ServiceTP2
         , m.ServiceTP3 AS ServiceTP3
         , m.ServiceTP4 AS ServiceTP4
         , m.ServiceTP5 AS ServiceTP5

         , m.ServiceTP1 / 12 AS ServiceTPMonthly1
         , m.ServiceTP2 / 12 AS ServiceTPMonthly2
         , m.ServiceTP3 / 12 AS ServiceTPMonthly3
         , m.ServiceTP4 / 12 AS ServiceTPMonthly4
         , m.ServiceTP5 / 12 AS ServiceTPMonthly5

         , m.StdWarranty as WarrantyLevel
         , null as PortfolioType
         , wg.Sog as Sog

    from Hardware.GetCostsFull(1, @cnt, @wg, @av, (select top(1) id from Dependencies.Duration where IsProlongation = 0 and Value = 5), @reactiontime, @reactiontype, @loc, @pro, 0, -1) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CONTRACT');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SLA', 'SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP1', 'Service Tranfer Price yearly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP2', 'Service Tranfer Price yearly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP3', 'Service Tranfer Price yearly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP4', 'Service Tranfer Price yearly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP5', 'Service Tranfer Price yearly - year5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTPMonthly1', 'Service Tranfer Price monthly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTPMonthly2', 'Service Tranfer Price monthly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTPMonthly3', 'Service Tranfer Price monthly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTPMonthly4', 'Service Tranfer Price monthly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTPMonthly5', 'Service Tranfer Price monthly - year5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WarrantyLevel', 'Warranty Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PortfolioType', 'Portfolio Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

IF OBJECT_ID('Report.FlatFeeReport') IS NOT NULL
  DROP FUNCTION Report.FlatFeeReport;
go 

CREATE FUNCTION Report.FlatFeeReport
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    select    c.Name as Country
            , c.CountryGroup
            , wg.Name as Wg
            , fee.Fee_Approved as Fee

    from Hardware.AvailabilityFeeCalc fee
    join InputAtoms.CountryView c on c.Id = fee.Country
    join InputAtoms.WgView wg on wg.id = fee.Wg

    where (@cnt is null or fee.Country = @cnt)
      and (@wg is null or fee.Wg = @wg)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'FLAT-FEE-REPORTS');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Fee', 'FSL Flatfee monthly (EUR)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');

GO
IF OBJECT_ID('Report.HddRetentionCentral') IS NOT NULL
  DROP FUNCTION Report.HddRetentionCentral;
go 

CREATE FUNCTION Report.HddRetentionCentral
(
    @wg bigint,
    @dur bigint
)
RETURNS TABLE 
AS
RETURN (
    select wg.Name as Wg
         , wg.Description as WgDescription
         , dur.Name as Duration
         , hdd.HddRet_Approved as HddRet
         , null as TP
         , null as DealerPrice
         , null as ListPrice
    from Hardware.HddRetention hdd
    join InputAtoms.WgSogView wg on wg.Id = hdd.Wg
    join Dependencies.Duration dur on dur.Id = hdd.Year
    where (@wg is null or wg.Id = @wg)
      and (@dur is null or dur.Id = @dur)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-CENTRAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Duration', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'HddRet', 'HDD retention', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Duration');

GO
IF OBJECT_ID('Report.HddRetentionByCountry') IS NOT NULL
  DROP FUNCTION Report.HddRetentionByCountry;
go 

CREATE FUNCTION Report.HddRetentionByCountry
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
    select m.Id
         , m.Country
         , c.CountryGroup
         , wg.Name as Wg
         , wg.Description as WgDescription
         , m.Fsp
         , m.Fsp as TopFsp
     
         , m.HddRet
         , m.ServiceTP
         , m.DealerPrice
         , m.ListPrice

         , wg.Sog

         , m.Availability
         , m.Duration
         , m.ReactionTime
         , m.ReactionType
         , m.ServiceLocation
         , m.ProActiveSla

    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.CountryView c on c.id = m.CountryId
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)

go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-COUNTRY');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Support Pack Code', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TopFsp', 'TopUp Code', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'HddRet', 'HDD retention', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Duration');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');


IF OBJECT_ID('Report.HddRetentionParameter') IS NOT NULL
  DROP FUNCTION Report.HddRetentionParameter;
go 

CREATE FUNCTION Report.HddRetentionParameter
(
    @wg bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
           wg.Name as Wg
         , wg.Description as WgDescription
         , y.Name as Year
         , hdd.HddFr_Approved as HddFr
         , hdd.HddMaterialCost_Approved as HddMaterialCost

    from Hardware.HddRetention hdd
    join InputAtoms.WgSogView wg on wg.Id = hdd.Wg
    join Dependencies.Year y on y.Id = hdd.Year

    where (@wg is null or wg.Id = @wg)
      and (@year is null or hdd.Year = @year)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-PARAMETER');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Year', 'Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'HddFr', 'FR(year)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'HddMaterialCost', 'MC(Year)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'year', 'Year');

GO
IF OBJECT_ID('Report.LocapDetailed') IS NOT NULL
  DROP FUNCTION Report.LocapDetailed;
go 

CREATE FUNCTION Report.LocapDetailed
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
select     m.Id
         , m.Fsp
         , wg.Description as WgDescription
         , wg.Name as Wg
         , wg.SogDescription as SogDescription
         , m.ServiceLocation as ServiceLevel
         , m.ReactionTime
         , m.Year as ServicePeriod
         , wg.Sog as Sog
         , m.ProActiveSla
         , m.ProActive + m.ServiceTP as Dcos
         , m.ServiceTP as ServiceTP
         , m.ListPrice
         , m.DealerPrice
         , m.Country
         , m.FieldServiceCost as FieldServiceCost
         , m.ServiceSupportCost as ServiceSupportCost 
         , m.MaterialOow as MaterialOow
         , m.MaterialW as MaterialW
         , m.TaxAndDutiesW as TaxAndDutiesW
         , m.Logistic as LogisticW
         , m.Logistic as LogisticOow
         , m.Reinsurance as Reinsurance
         , m.Reinsurance as ReinsuranceOow
         , m.OtherDirect as OtherDirect
         , m.Credits as Credits
         , null as IndirectCostOpex
         , null as ServiceType
         , null as PlausiCheck
         , null as PortfolioType
    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOCAP-DETAILED');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Service Offering Group (SOG) Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Dcos', 'Service DCOS', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP', 'Service TP (Full cost)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Bottom-up calculated)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Bottom-up calculated)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'FieldServiceCost', 'Field Service Cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceSupportCost', 'Service Support Cost Maintenance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'MaterialOow', 'Material Cost OOW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'MaterialW', 'Material Cost Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TaxAndDutiesW', 'Customs Duty base warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'LogisticW', 'Logistics Cost Base Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'LogisticOow', 'Logistics Cost OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ReinsuranceOow', 'Reinsurance OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'OtherDirect', 'Other direct cost and contingency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Credits', 'Credits', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'IndirectCostOpex', 'Indirect cost and OPEX', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceType', 'Service type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PlausiCheck', 'Plausi Check', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PortfolioType', 'Portfolio Type', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

IF OBJECT_ID('Report.Locap') IS NOT NULL
  DROP FUNCTION Report.Locap;
go 

CREATE FUNCTION Report.Locap
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
    select m.Id
         , m.Fsp
         , wg.Description as WgDescription
         , m.FspDescription as ServiceLevel
         , m.ReactionTime
         , m.Year as ServicePeriod
         , wg.Name as Wg
         , (m.ProActive + m.ServiceTP) as Dcos
         , m.ServiceTP
         , m.Country
         , null as ServiceType
         , null as PlausiCheck
         , null as PortfolioType
         , null as ReleaseCreated
         , wg.Sog
    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)
GO

declare @reportId bigint = (select Id from Report.Report where Name = 'Locap');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Dcos', 'Service DCOS', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP', 'Service TP (Full cost)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceType', 'Service type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PlausiCheck', 'Plausi Check', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PortfolioType', 'Portfolio Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReleaseCreated', 'Release created', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

GO
IF OBJECT_ID('Report.LogisticCostCalcCentral') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCalcCentral;
go 

CREATE FUNCTION Report.LogisticCostCalcCentral
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

    from Hardware.GetCostsFull(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, -1) m
    join InputAtoms.CountryView c on c.Id = m.CountryId
    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-CALC-CENTRAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Region', 'Alias Region', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLevel', 'Service Level Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTC', 'Transport cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Handling', 'Handling cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TaxAndDutiesW', 'Tax and Duties cost (in Warranty)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TaxAndDutiesOow', 'Tax and Duties cost (out of Warranty)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'LogisticW', 'Logistics cost (in Warranty)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'LogisticOow', 'Logistics cost (out of Warranty)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Fee', 'FSL Flatfee yearly cost', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');


IF OBJECT_ID('Report.LogisticCostCalcCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCalcCountry;
go 

CREATE FUNCTION Report.LogisticCostCalcCountry
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
    select Id
         , Region
         , Country
         , Wg

         , ServiceLevel
         , ReactionTime
         , ReactionType
         , Duration
         , Availability
         , ProActiveSla

         , ServiceTC
         , Handling
         , TaxAndDutiesW
         , TaxAndDutiesOow

         , LogisticW
         , LogisticOow

         , Fee

    from Report.LogisticCostCalcCentral(coalesce(@cnt, -1), @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-CALC-COUNTRY');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Region', 'Alias Region', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLevel', 'Service Level Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTC', 'Transport cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Handling', 'Handling cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TaxAndDutiesW', 'Tax and Duties cost (in Warranty)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TaxAndDutiesOow', 'Tax and Duties cost (out of Warranty)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'LogisticW', 'Logistics cost (in Warranty)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'LogisticOow', 'Logistics cost (out of Warranty)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Fee', 'FSL Flatfee yearly cost', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

IF OBJECT_ID('Report.LogisticCostCentral') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCentral;
go 

CREATE FUNCTION Report.LogisticCostCentral
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select c.Region
         , c.Name as Country
         , wg.Name as Wg

         , 'EUR' as Currency

         , (time.Name + ' ' + type.Name) as ReactionType

         , l.StandardHandling_Approved as StandardHandling
         , l.HighAvailabilityHandling_Approved as HighAvailabilityHandling
         , l.StandardDelivery_Approved as StandardDelivery
         , l.ExpressDelivery_Approved as ExpressDelivery
         , l.TaxiCourierDelivery_Approved as TaxiCourierDelivery
         , l.ReturnDeliveryFactory_Approved as ReturnDeliveryFactory

    from Hardware.LogisticsCostView l
    join InputAtoms.CountryView c on c.Id = l.Country
    join InputAtoms.WgSogView wg on wg.Id = l.Wg
    JOIN Dependencies.ReactionTime time ON time.id = l.ReactionTime
    JOIN Dependencies.ReactionType type ON type.Id = l.ReactionType

    where (@cnt is null or l.Country = @cnt)
      and (@wg is null or l.Wg = @wg)
      and (@reactiontime is null or l.ReactionTime = @reactiontime)
      and (@reactiontype is null or l.ReactionType = @reactiontype)

)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-CENTRAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Region', 'Alias Region', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');

IF OBJECT_ID('Report.LogisticCostCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCountry;
go 

CREATE FUNCTION Report.LogisticCostCountry
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select Region
         , Country
         , Wg

         , Currency

         , ReactionType

         , StandardHandling
         , HighAvailabilityHandling
         , StandardDelivery
         , ExpressDelivery
         , TaxiCourierDelivery
         , ReturnDeliveryFactory

    from Report.LogisticCostCentral(coalesce(@cnt, -1), @wg, @reactiontime, @reactiontype)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-COUNTRY');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Region', 'Alias Region', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');

IF OBJECT_ID('Report.LogisticCostInputCentral') IS NOT NULL
  DROP FUNCTION Report.LogisticCostInputCentral;
go 

CREATE FUNCTION Report.LogisticCostInputCentral
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select c.Region
         , c.Name as Country
         , wg.Name as Wg

         , c.Currency as Currency

         , (time.Name + ' ' + type.Name) as ReactionType

         , l.StandardHandling_Approved as StandardHandling
         , l.HighAvailabilityHandling_Approved as HighAvailabilityHandling
         , l.StandardDelivery_Approved as StandardDelivery
         , l.ExpressDelivery_Approved as ExpressDelivery
         , l.TaxiCourierDelivery_Approved as TaxiCourierDelivery
         , l.ReturnDeliveryFactory_Approved as ReturnDeliveryFactory

    FROM Hardware.LogisticsCosts l
    join InputAtoms.CountryView c on c.Id = l.Country
    join InputAtoms.WgSogView wg on wg.Id = l.Wg
    JOIN Dependencies.ReactionTime_ReactionType rtt on rtt.Id = l.ReactionTimeType
    JOIN Dependencies.ReactionTime time ON time.id = rtt.ReactionTimeId
    JOIN Dependencies.ReactionType type ON type.Id = rtt.ReactionTypeId

    where (@cnt is null or l.Country = @cnt)
      and (@wg is null or l.Wg = @wg)
      and (@reactiontime is null or rtt.ReactionTimeId = @reactiontime)
      and (@reactiontype is null or rtt.ReactionTypeId = @reactiontype)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-INPUT-CENTRAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Region', 'Alias Region', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');

IF OBJECT_ID('Report.LogisticCostInputCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostInputCountry;
go 

CREATE FUNCTION Report.LogisticCostInputCountry
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select Region
         , Country
         , Wg

         , Currency

         , ReactionType

         , StandardHandling
         , HighAvailabilityHandling
         , StandardDelivery
         , ExpressDelivery
         , TaxiCourierDelivery
         , ReturnDeliveryFactory

    FROM Report.LogisticCostInputCentral(coalesce(@cnt, -1), @wg, @reactiontime, @reactiontype)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-INPUT-COUNTRY');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Region', 'Alias Region', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');

IF OBJECT_ID('Report.PoStandardWarrantyMaterial') IS NOT NULL
  DROP FUNCTION Report.PoStandardWarrantyMaterial;
go 

CREATE FUNCTION Report.PoStandardWarrantyMaterial
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

              , case when stdw.DurationValue is not null then stdw.DurationValue 
                    when stdw2.DurationValue is not null then stdw2.DurationValue 
                 end as StdWarranty

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

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId --find local standard warranty portfolio
        LEFT JOIN Fsp.HwStandardWarrantyView stdw2 on stdw2.Wg = m.WgId and stdw2.Country is null    --find principle standard warranty portfolio, if local does not exist

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

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'PO-STANDARD-WARRANTY-MATERIAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'LUTCode', 'CountryCode LUT', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Pla', 'Pla', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'MaterialW', 'Material Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'MaterialCostWarranty', 'Material Cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR1', 'FR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR2', 'FR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR3', 'FR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR4', 'FR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'AFR5', 'FR5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SparesAvailability', 'Spares availability (year)', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');
IF OBJECT_ID('Report.ProActive') IS NOT NULL
  DROP FUNCTION Report.ProActive;
go 

CREATE FUNCTION Report.ProActive
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
            , m.Country
            , c.CountryGroup
            , null as InfSolution
            , m.Wg
            , m.Fsp
            , null as SpDescription
            , null as Sp

            , case 
                    when dur.IsProlongation = 1 then 'Prolongation'
                    else CAST(dur.Value as varchar(1))
              end as Duration

            , m.Availability
            , m.ReactionTime
            , m.ReactionType
            , m.ServiceLocation
            , m.ProActiveSla

            , m.ServiceTP - m.ProActive as ReActive
            , m.ProActive
            , m.ServiceTP

    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    JOIN InputAtoms.CountryView c on c.Id = m.CountryId
    join Dependencies.Duration dur on dur.Id = m.DurationId
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'PROACTIVE-REPORTS');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'InfSolution', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SolutionPack Product no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SpDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SolutionPack Service Short Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Serice Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ReActive', 'Thereof ReActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ProActive', 'Thereof ProActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP', 'Service TP (Full cost)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

GO
IF OBJECT_ID('Report.SolutionPackPriceListDetail') IS NOT NULL
  DROP FUNCTION Report.SolutionPackPriceListDetail;
go 

CREATE FUNCTION Report.SolutionPackPriceListDetail
(
    @sog bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
              sog.Description as SogDescription
            , null as Digit
            , fsp.Name
            , sog.Name as Sog

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.[2ndLevelSupportCosts_Approved] as SupportCost
            
            , sw.Reinsurance_Approved as Reinsurance

            , sw.TransferPrice_Approved as TP
            , sw.DealerPrice_Approved as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.SwSpMaintenanceCostView sw
    join InputAtoms.Sog sog on sog.id = sw.Sog
    join Fsp.SwFspCodeTranslation fsp on fsp.SogId = sw.Sog
    where (@sog is null or sw.Sog = @sog)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SOLUTIONPACK-PRICE-LIST-DETAILS');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Digit', 'SW Product Order no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'Service Offering Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SolutionPack Product no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SpDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SolutionPack Service Short Description', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'SupportCost', 'Technical Solution Support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 5, 'sog', 'Service Offering Group');

GO
IF OBJECT_ID('Report.SolutionPackPriceList') IS NOT NULL
  DROP FUNCTION Report.SolutionPackPriceList;
go 

CREATE FUNCTION Report.SolutionPackPriceList
(
    @sog bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
              sog.Description as SogDescription
            , sog.Name as Sog
            , fsp.Name

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.TransferPrice_Approved as TP
            , sw.DealerPrice_Approved as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.SwSpMaintenanceCostView sw
    join InputAtoms.Sog sog on sog.id = sw.Sog
    join Fsp.SwFspCodeTranslation fsp on fsp.SogId = sw.Sog
    where (@sog is null or sw.Sog = @sog)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SOLUTIONPACK-PRICE-LIST');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'Service Offering Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SolutionPack Product no.', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SpDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SolutionPack Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 5, 'sog', 'Service Offering Group');

GO
IF OBJECT_ID('Report.SolutionPackProActiveCosting') IS NOT NULL
  DROP FUNCTION Report.SolutionPackProActiveCosting;
go 

CREATE FUNCTION Report.SolutionPackProActiveCosting
(
    @cnt bigint,
    @sog bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select    c.CountryGroup
            , c.Name as Country

            , null as InfSolution
            , sog.Name as Wg
            , sog.Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription
            , sog.Description as Sp

            , case 
                when y.IsProlongation = 1 then 'Prolongation'
                else CAST(y.Value as varchar(15))
             end as Duration

             , sc.TransferPrice_Approved - pro.ProActive_Approved as ReActive
             , pro.ProActive_Approved as ProActive
             , sc.TransferPrice_Approved as ServiceTP

    from SoftwareSolution.ProActiveView pro
    join Dependencies.Year y on y.id = pro.Year
    join InputAtoms.CountryView c on c.id = pro.Country
    join InputAtoms.WgSogView sog on sog.id = pro.Sog
    left join SoftwareSolution.SwSpMaintenanceCostView sc on sc.Year = pro.Year and sc.Sog = pro.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sc.Availability
                                          and fsp.DurationId = sc.Year
                                          and fsp.SogId = sc.Sog

    where (@cnt is null or pro.Country = @cnt)
      and (@sog is null or pro.Sog = @sog)
      and (@year is null or pro.Year = @year)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SOLUTIONPACK-PROACTIVE-COSTING');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'InfSolution', 'Infrastructure Solution', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SolutionPack Product no.', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceDescription', 'SolutionPack Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SolutionPack Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Serice Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ReActive', 'Thereof ReActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ProActive', 'Thereof ProActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP', 'Service TP (Full cost)', 1, 1);


set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 5, 'sog', 'Service Offering Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 13, 'year', 'Service period');

GO
IF OBJECT_ID('Report.SwServicePriceListDetail') IS NOT NULL
  DROP FUNCTION Report.SwServicePriceListDetail;
go 

CREATE FUNCTION Report.SwServicePriceListDetail
(
    @sog bigint,
    @av bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
              sog.Description as SogDescription
            , sog.Name as Sog
            , null as Fsp2
            , fsp.Name as Fsp

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.ServiceSupport_Approved as ServiceSupport
            , sw.Reinsurance_Approved as Reinsurance

            , sw.TransferPrice_Approved as TP
            , sw.DealerPrice_Approved as DealerPrice
            , sw.MaintenanceListPrice_Approved as ListPrice

    from SoftwareSolution.SwSpMaintenanceCostView sw
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
                                          and fsp.SogId = sw.Sog
    where (@sog is null or sw.Sog = @sog)
      and (@av is null or sw.Availability = @av)
      and (@year is null or sw.Year = @year)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SW-SERVICE-PRICE-LIST-DETAILED');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp2', 'SW Product Order no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'Service Offering Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SW Service Product no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SpDescription', 'SW Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SW Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceSupport', 'Technical Solution Support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 5, 'sog', 'Service Offering Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 13, 'year', 'Year');

GO

IF OBJECT_ID('Report.SwServicePriceList') IS NOT NULL
  DROP FUNCTION Report.SwServicePriceList;
go 

CREATE FUNCTION Report.SwServicePriceList
(
    @sog bigint,
    @av bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
              sog.Description as SogDescription
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as SpDescription
            , null as Sp

            , sw.TransferPrice_Approved as TP
            , sw.DealerPrice_Approved as DealerPrice
            , sw.MaintenanceListPrice_Approved as ListPrice

    from SoftwareSolution.SwSpMaintenanceCostView sw
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
                                          and fsp.SogId = sw.Sog
    where (@sog is null or sw.Sog = @sog)
      and (@av is null or sw.Availability = @av)
      and (@year is null or sw.Year = @year)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SW-SERVICE-PRICE-LIST');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Infrastructure Solution', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'Service Offering Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'SW Service Product no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SpDescription', 'SW Service Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sp', 'SW Service Short Description', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TP', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 5, 'sog', 'Service Offering Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 13, 'year', 'Year');

GO
