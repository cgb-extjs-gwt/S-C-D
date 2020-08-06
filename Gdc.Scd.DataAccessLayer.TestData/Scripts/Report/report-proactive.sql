IF OBJECT_ID('Report.spProActive') IS NOT NULL
  DROP PROCEDURE Report.spProActive;
go 

CREATE PROCEDURE [Report].[spProActive]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       bigint,
    @limit        int
)
AS
BEGIN

    declare @cntGroup nvarchar(255) = (select Name from InputAtoms.CountryGroup where Id = (select CountryGroupId from InputAtoms.Country where id = @cnt))

    declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where SogId in (select wg.SogId from InputAtoms.Wg wg where @wg is null or wg.Id = @wg)
        and IsSoftware = 0
        and SogId is not null
        and Deactivated = 0;

    declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

    declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; if @pro is not null insert into @proTable(id) values(@pro);

    declare @countries table (
          Id bigint not null
        , Name nvarchar(128)
        , Currency nvarchar(16)
        , ExchangeRate float
    );

    insert into @countries
    select c.Id, c.Name, cur.Name as Currency, er.Value as ExchangeRate
    from InputAtoms.Country c 
    left join [References].Currency cur on cur.Id = c.CurrencyId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where c.Id = @cnt;

    declare @warranty_groups table (
          Id bigint not null primary key
        , Name nvarchar(255)
        , Pla nvarchar(255)
        , Sog nvarchar(255)
        , SogDescription nvarchar(255)
    );

    insert into @warranty_groups
    select    wg.Id
            , wg.Name as Wg
            , pla.Name as Pla
            , sog.Name as Sog
            , sog.Description
    from InputAtoms.Wg wg 
    inner join InputAtoms.Sog sog on sog.id = wg.SogId
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId
    where wg.Deactivated = 0 and wg.IsSoftware = 0;

    with cte as (
        select m.*

             , (sum(mc.ReActiveTP_Released * ib.InstalledBaseCountryNorm)                          over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_tp_reactive
             , (sum(case when mc.ReActiveTP_Released <> 0 then ib.InstalledBaseCountryNorm end)    over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_tp_reactive
             
             , (sum(mc.ProActive_Released * ib.InstalledBaseCountryNorm)                           over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_x_pro
             , (sum(case when mc.ProActive_Released <> 0 then ib.InstalledBaseCountryNorm end)     over(partition by m.CountryId, wg.SogId, m.AvailabilityId, m.DurationId, m.ReactionTimeId, m.ReactionTypeId, m.ServiceLocationId, m.ProActiveSlaId)) as sum_ib_by_pro

        FROM Portfolio.GetBySlaPaging(@cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, null, null) m

        join InputAtoms.Wg wg on wg.id = m.WgId and wg.Deactivated = 0
        left join Hardware.GetInstallBaseOverSog(1, @cntTable) ib on ib.Country = m.CountryId and ib.Wg = m.WgId
        LEFT JOIN Hardware.ManualCost mc on mc.PortfolioId = m.Id
    )
    , cte2 as (
        select 

              ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum
            
            , m.Id
            , m.CountryId
            , m.WgId
            , m.AvailabilityId
            , m.DurationId
            , m.ReactionTimeId
            , m.ReactionTypeId
            , m.ServiceLocationId
            , m.ProActiveSlaId

            , case when m.sum_ib_x_tp_reactive <> 0 and m.sum_ib_by_tp_reactive <> 0 then m.sum_ib_x_tp_reactive / m.sum_ib_by_tp_reactive else 0 end as ReactiveTpSog

            , case when m.sum_ib_x_pro <> 0 and m.sum_ib_by_pro <> 0 then m.sum_ib_x_pro / m.sum_ib_by_pro else 0 end as ProActiveSog

            , fsp.Name as Fsp
            , fsp.ServiceDescription as FspDescription

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla and fsp.IsStandardWarranty <> 1

    )
    select    m.rownum
            , m.Id
            , c.Name as Country
            , @cntGroup as CountryGroup
            
            , m.Fsp
            , wg.Name as Wg
            , wg.Pla as PLA

            , loc.Name as ServiceLocation
            , rtime.Name as ReactionTime
            , rtype.Name as ReactionType
            , av.Name as Availability
            , proSla.ExternalName as ProActiveSla

            , case when dur.IsProlongation = 1 then 'Prolongation' else CAST(dur.Value as varchar(1)) end as Duration

             , m.ReactiveTpSog * c.ExchangeRate as ReActive
             , m.ProActiveSog * c.ExchangeRate as ProActive
             , (m.ReactiveTpSog + coalesce(m.ProActiveSog, 0)) * c.ExchangeRate as ServiceTP

            , c.Currency

            , wg.Sog
            , wg.SogDescription

            , m.FspDescription

    from cte2 m

    INNER JOIN @countries c on c.Id = m.CountryId

    INNER JOIN @warranty_groups wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.Id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'PROACTIVE-REPORTS');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PLA', 'PLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReActive', 'Thereof HW Service cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ProActive', 'Thereof ProActive cost (TP)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP', 'Service TP (Full cost)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Service Offering Group (SOG) Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'FspDescription', 'Service type', 1, 1);

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