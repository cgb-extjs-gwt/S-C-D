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

              , ib.InstalledBaseCountry as IB

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
      
              , afr.AFR1     as AFR1
              , afr.AFR2     as AFR2
              , afr.AFR3     as AFR3
              , afr.AFR4     as AFR4
              , afr.AFR5     as AFR5
              , afr.AFRP1    as AFRP1

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

              , ssc.[1stLevelSupportCosts] as '1stLevelSupportCosts'
              , ssc.[2ndLevelSupportCosts] as '2ndLevelSupportCosts'
           
              , r.ReinsuranceNBD
              , r.ReinsuranceNBD as ReinsuranceNBD_Oow
              , r.Reinsurance27x7
              , r.Reinsurance27x7 as Reinsurance27x7_Oow
           
              , mcw.MaterialCostWarranty as MaterialCostWarranty
              , mco.MaterialCostOow as MaterialCostOow

              , dur.Value as Duration
              , dur.IsProlongation

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

        LEFT JOIN Hardware.InstallBase ib on ib.Country = m.CountryId and ib.Wg = m.WgId

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg2.ClusterPla

        LEFT JOIN ReinsuranceCte r on r.Wg = m.WgId and r.Year = m.DurationId

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
    select    m.*

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
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'IB', 'Installed base', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LabourCost', 'Labour cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'TravelCost', 'Travel cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'PerformanceRate', 'Performance rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'TravelTime', 'Travel time (MTTT)', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'RepairTime', 'Repair time (MTTR)', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'OnsiteHourlyRate', 'Onsite hourly rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'StandardHandling', 'Logistics handling cost', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LogisticTransportcost', 'Logistics transport cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'AvailabilityFee', 'Availability Fee', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'TaxAndDutiesW', 'Tax & duties', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'MarkupFactorOtherCost', 'Markup factor for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'MarkupOtherCost', 'Markup for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'MarkupFactorStandardWarranty', 'Markup factor for standard warranty local cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'MarkupStandardWarranty', 'Markup for standard warranty local cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'AFR1', 'AFR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'AFR2', 'AFR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'AFR3', 'AFR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'AFR4', 'AFR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'AFR5', 'AFR5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'FieldServiceCost1', 'Calculated Field Service Cost 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'FieldServiceCost2', 'Calculated Field Service Cost 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'FieldServiceCost3', 'Calculated Field Service Cost 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'FieldServiceCost4', 'Calculated Field Service Cost 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'FieldServiceCost5', 'Calculated Field Service Cost 5 years', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, '2ndLevelSupportCosts', '2nd level support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, '1stLevelSupportCosts', '1st level support cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'ReinsuranceNBD', 'Reinsurance NBD', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'ReinsuranceNBD_Oow', 'Reinsurance NBD OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'Reinsurance27x7', 'Reinsurance 7x24', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'Reinsurance27x7_Oow', 'Reinsurance 7x24 OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'MaterialCostWarranty', 'Material cost iW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'MaterialCostOow', 'Material cost OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Warranty duration', 1, 1);

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



