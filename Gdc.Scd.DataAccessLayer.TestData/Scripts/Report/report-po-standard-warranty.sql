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
              , (dur.Name + ' ' + loc.Name) as ServiceLevel
              , rtime.Name as ReactionTime
              , rtype.Name as ReactionType
              , av.Name    as Availability
              , prosla.ExternalName as ProActiveSla

              , mcw.MaterialCostIw_Approved as MaterialCostWarranty

              , afr.AFR1_Approved as AFR1
              , afr.AFR2_Approved as AFR2
              , afr.AFR3_Approved as AFR3
              , afr.AFR4_Approved as AFR4
              , afr.AFR5_Approved as AFR5

              , case when stdw.DurationValue >= 1 then mcw.MaterialCostIw_Approved * afr.AFR1_Approved else 0 end as mat1
              , case when stdw.DurationValue >= 2 then mcw.MaterialCostIw_Approved * afr.AFR2_Approved else 0 end as mat2
              , case when stdw.DurationValue >= 3 then mcw.MaterialCostIw_Approved * afr.AFR3_Approved else 0 end as mat3
              , case when stdw.DurationValue >= 4 then mcw.MaterialCostIw_Approved * afr.AFR4_Approved else 0 end as mat4
              , case when stdw.DurationValue >= 5 then mcw.MaterialCostIw_Approved * afr.AFR5_Approved else 0 end as mat5

              , null as SparesAvailability

        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, null, @reactiontime, @reactiontype, @loc, @pro) m

        JOIN InputAtoms.CountryView c on c.Id = m.CountryId

        JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

        LEFT JOIN InputAtoms.Pla pla on pla.id = wg.PlaId

        JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

        LEFT JOIN Fsp.HwStandardWarranty stdw on stdw.Country = m.CountryId and stdw.Wg = m.WgId

        LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw on mcw.Country = m.CountryId and mcw.Wg = m.WgId
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

            , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW

            , m.MaterialCostWarranty

            , m.AFR1
            , m.AFR2
            , m.AFR3
            , m.AFR4
            , m.AFR5

            , m.SparesAvailability
    from cte m
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'PO-STANDARD-WARRANTY-MATERIAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'LUTCode', 'CountryCode LUT', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Pla', 'Pla', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'MaterialW', 'Material Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'MaterialCostWarranty', 'Material Cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'AFR1', 'FR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'AFR2', 'FR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'AFR3', 'FR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'AFR4', 'FR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'AFR5', 'FR5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SparesAvailability', 'Spares availability (year)', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('usercountry', 0), 'cnt', 'Country Name');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgstandard', 0), 'wg', 'Warranty Group');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability', 0), 'av', 'Availability');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontime', 0), 'reactiontime', 'Reaction time');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;                                                                        
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');
