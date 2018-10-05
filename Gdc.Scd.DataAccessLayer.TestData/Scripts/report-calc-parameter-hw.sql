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
    @loc bigint
)
RETURNS TABLE 
AS
RETURN (
    with ReinsuranceCte as (
        select r.Wg
             , r.Duration
             , SUM(CASE WHEN UPPER(rt.Name) = 'NBD' THEN r.Cost_Approved END) AS  ReinsuranceNBD
             , SUM(CASE WHEN UPPER(av.Name) = '24X7' THEN r.Cost_Approved END) AS Reinsurance27x7
        from Hardware.ReinsuranceView r
        join Dependencies.Availability av on av.Id = r.AvailabilityId 
        join Dependencies.ReactionTime rt on rt.id = r.ReactionTimeId
        group by r.Wg, r.Duration
    )
    select 
            m.Country
          , wg.Description as WgDescription
          , wg.Name as Wg
          , wg.SogDescription
          , wg.SCD_ServiceType
          , null as Sla
          , m.ServiceLocation as ServiceLocation
          , m.ReactionTime
          , m.ReactionType
          , m.Availability
          , c.Currency
          , m.Fsp
          , m.FspDescription

          --cost blocks

          , fsc.LabourCost_Approved as LabourCost
          , fsc.TravelCost_Approved as TravelCost
          , null as PerformanceRateNbd
          , fsc.TravelTime_Approved as TravelTime
          , fsc.RepairTime_Approved as RepairTime
          , fsc.OnsiteHourlyRates_Approved as OnsiteHourlyRate
          , null as OohUplift
          , null as Uplift

          , lc.StandardHandling_Approved as StandardHandling
          , null as LogisticTransportcost

          , null as FslFlatfee
      
          , Hardware.CalcTaxAndDutiesWar(mcw.MaterialCostWarranty_Approved, tax.TaxAndDuties_Approved) as TaxAndDutiesW
          , Hardware.CalcTaxAndDutiesWar(mco.MaterialCostOow_Approved, tax.TaxAndDuties_Approved) as TaxAndDutiesOow

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
                        fsc.TimeAndMaterialShare_Approved, 
                        fsc.TravelCost_Approved, 
                        fsc.LabourCost_Approved, 
                        fsc.PerformanceRate_Approved, 
                        fsc.TravelTime_Approved, 
                        fsc.RepairTime_Approved, 
                        fsc.OnsiteHourlyRates_Approved, 
                        afr.AFR1
                    ) as FieldServiceCost1  

          , Hardware.CalcFieldServiceCost(
                            fsc.TimeAndMaterialShare_Approved, 
                            fsc.TravelCost_Approved, 
                            fsc.LabourCost_Approved, 
                            fsc.PerformanceRate_Approved, 
                            fsc.TravelTime_Approved, 
                            fsc.RepairTime_Approved, 
                            fsc.OnsiteHourlyRates_Approved, 
                            afr.AFR1 + afr.AFR2
                        ) as FieldServiceCost2

            , Hardware.CalcFieldServiceCost(
                            fsc.TimeAndMaterialShare_Approved, 
                            fsc.TravelCost_Approved, 
                            fsc.LabourCost_Approved, 
                            fsc.PerformanceRate_Approved, 
                            fsc.TravelTime_Approved, 
                            fsc.RepairTime_Approved, 
                            fsc.OnsiteHourlyRates_Approved, 
                            afr.AFR1 + afr.AFR2 + afr.AFR3
                        ) as FieldServiceCost3

            , Hardware.CalcFieldServiceCost(
                            fsc.TimeAndMaterialShare_Approved, 
                            fsc.TravelCost_Approved, 
                            fsc.LabourCost_Approved, 
                            fsc.PerformanceRate_Approved, 
                            fsc.TravelTime_Approved, 
                            fsc.RepairTime_Approved, 
                            fsc.OnsiteHourlyRates_Approved, 
                            afr.AFR1 + afr.AFR2 + afr.AFR3 + afr.AFR4
                        ) as FieldServiceCost4

            , Hardware.CalcFieldServiceCost(
                            fsc.TimeAndMaterialShare_Approved, 
                            fsc.TravelCost_Approved, 
                            fsc.LabourCost_Approved, 
                            fsc.PerformanceRate_Approved, 
                            fsc.TravelTime_Approved, 
                            fsc.RepairTime_Approved, 
                            fsc.OnsiteHourlyRates_Approved, 
                            afr.AFR1 + afr.AFR2 + afr.AFR3 + afr.AFR4 + afr.AFR5
                        ) as FieldServiceCost5

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

    from Report.GetMatrixBySla(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc) m

    JOIN InputAtoms.CountryView c on c.Id = m.CountryId

    JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

    JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

    JOIN Atom.Afr5YearView afr on afr.Wg = m.WgId

    --cost blocks
    JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId 
                                            AND fsc.Country = m.CountryId 
                                            AND fsc.ServiceLocation = m.ServiceLocationId
                                            AND fsc.ReactionTypeId = m.ReactionTypeId
                                            AND fsc.ReactionTimeId = m.ReactionTimeId

    JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId 
                                        AND lc.Wg = m.WgId
                                        AND lc.ReactionTime = m.ReactionTimeId
                                        AND lc.ReactionType = m.ReactionTypeId

    JOIN Atom.TaxAndDutiesView tax on tax.Wg = m.WgId AND tax.Country = m.CountryId

    JOIN Atom.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    JOIN Atom.MaterialCostOow mco on mco.Wg = m.WgId AND mco.ClusterRegion = c.ClusterRegionId

    JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.Wg = m.WgId

    LEFT JOIN ReinsuranceCte r on r.Wg = m.WgId and r.Duration = m.DurationId
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
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PerformanceRateNbd', 'Performance rate NBD', 1, 1);

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