CREATE FUNCTION Report.Contract
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
    with cte as (
        select @cnt as CountryId
             , @wg as WgId
             , m.AvailabilityId 
             , m.ReactionTimeId 
             , m.ReactionTypeId 
             , m.ServiceLocationId
             , sum(case when dur.Value = 1 then sc.ServiceTP_Approved end) as ServiceTP1
             , sum(case when dur.Value = 2 then sc.ServiceTP_Approved end) as ServiceTP2
             , sum(case when dur.Value = 3 then sc.ServiceTP_Approved end) as ServiceTP3
             , sum(case when dur.Value = 4 then sc.ServiceTP_Approved end) as ServiceTP4
             , sum(case when dur.Value = 5 then sc.ServiceTP_Approved end) as ServiceTP5
        from Matrix m
        join Hardware.ServiceCostCalculation sc on sc.MatrixId = m.Id
        join InputAtoms.Wg wg on wg.id = m.WgId and wg.DeactivatedDateTime is null
        join Dependencies.Duration dur on dur.Id = m.DurationId and dur.IsProlongation = 0
        where m.Denied = 0 
            and m.CountryId = @cnt
            and m.WgId = @wg
            and (@av is null or m.AvailabilityId = @av)
            and (@reactiontime is null or m.ReactionTimeId = @reactiontime)
            and (@reactiontype is null or m.ReactionTypeId = @reactiontype)
            and (@loc is null or m.ServiceLocationId = @loc)
        group by m.AvailabilityId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId
    )
    select cnt.Name as Country
         , wg.Name as Wg
         , wg.Description as WgDescription
         , null as SLA
         , loc.Name as ServiceLocation
         , rtime.Name as ReactionTime
         , rtype.Name as ReactionType
         , av.Name as Availability

         , CAST(ROUND(m.ServiceTP1, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTP1
         , CAST(ROUND(m.ServiceTP2, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTP2
         , CAST(ROUND(m.ServiceTP3, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTP3
         , CAST(ROUND(m.ServiceTP4, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTP4
         , CAST(ROUND(m.ServiceTP5, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTP5

         , CAST(ROUND(m.ServiceTP1 / 12, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTPMonthly1
         , CAST(ROUND(m.ServiceTP2 / 12, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTPMonthly2
         , CAST(ROUND(m.ServiceTP3 / 12, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTPMonthly3
         , CAST(ROUND(m.ServiceTP4 / 12, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTPMonthly4
         , CAST(ROUND(m.ServiceTP5 / 12, 2) AS VARCHAR(20)) + ' EUR' AS ServiceTPMonthly5

         , null as WarrantyLevel
         , null as PortfolioType
         , sog.Name as Sog

    from cte m
    join InputAtoms.CountryView cnt on cnt.Id = m.CountryId
    join InputAtoms.WgView wg on wg.id = m.WgId
    join Dependencies.Availability av on av.Id= m.AvailabilityId
    join Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
    join Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId
    left join InputAtoms.Sog sog on sog.Id = wg.SogId
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CONTRACT');
declare @index int = 0;

update Report.Report set SqlFunc = 'Report.Contract' where id = @reportId;

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
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTP1', 'Service Tranfer Price yearly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTP2', 'Service Tranfer Price yearly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTP3', 'Service Tranfer Price yearly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTP4', 'Service Tranfer Price yearly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTP5', 'Service Tranfer Price yearly - year5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTPMonthly1', 'Service Tranfer Price monthly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTPMonthly2', 'Service Tranfer Price monthly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTPMonthly3', 'Service Tranfer Price monthly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTPMonthly4', 'Service Tranfer Price monthly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceTPMonthly5', 'Service Tranfer Price monthly - year5', 1, 1);

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

