IF OBJECT_ID('Report.CalcOutputVsFREEZE') IS NOT NULL
  DROP FUNCTION Report.CalcOutputVsFREEZE;
go 

CREATE FUNCTION [Report].[CalcOutputVsFREEZE]
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

        LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId

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
         
            , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
            , m.LocalServiceStandardWarranty1_Approved + m.LocalServiceStandardWarranty2_Approved + m.LocalServiceStandardWarranty3_Approved + m.LocalServiceStandardWarranty4_Approved + m.LocalServiceStandardWarranty5_Approved as StandardWarranty_Approved
            , cur.Name as Currency
    from CostCte2 m
    join InputAtoms.Country cnt on cnt.id = @cnt
    join [References].Currency cur on cur.Id = cnt.CurrencyId
    join [References].ExchangeRate er on er.CurrencyId = cur.Id
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCOUTPUT-VS-FREEZE');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Portfolio Alignment', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceProduct', 'Service Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardWarranty', 'Not approved standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardWarranty_Approved', 'Approved standard warranty', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg', 0), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability', 0), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('duration', 0), 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontime', 0), 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');

GO