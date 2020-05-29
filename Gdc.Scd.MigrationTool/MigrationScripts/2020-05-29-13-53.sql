IF OBJECT_ID('Report.spLocapGlobalSupport') IS NOT NULL
  DROP PROCEDURE Report.spLocapGlobalSupport;
go 

CREATE PROCEDURE [Report].[spLocapGlobalSupport]
(
    @approved bit,
    @cnt      dbo.ListID readonly,
    @sog      bigint,
    @wg       dbo.ListID readonly,
    @av       dbo.ListID readonly,
    @dur      dbo.ListID readonly,
    @rtime    dbo.ListID readonly,
    @rtype    dbo.ListID readonly,
    @loc      dbo.ListID readonly,
    @pro      dbo.ListID readonly,
    @lastid   int,
    @limit    int
)
AS
BEGIN

    if OBJECT_ID('tempdb..#tmp') is not null drop table #tmp;

    --Calc for Emeia countries by SOG

    declare @emeiaCnt dbo.ListID;
    declare @emeiaWg dbo.ListId;

    insert into @emeiaCnt(id)
    select c.Id
    from InputAtoms.Country c 
    join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
    where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 1;

    insert into @emeiaWg
    select id
    from InputAtoms.Wg 
    where (@sog is null or SogId = @sog)
    and SogId in (select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id)))
    and IsSoftware = 0
    and SogId is not null
    and DeactivatedDateTime is null;

    with cte as (
        select m.* 
                , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSog(1, @emeiaCnt, @emeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro) m
        where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
    )
    , cte2 as (
        select    m.*
                , cnt.ISO3CountryCode
                , fsp.Name as Fsp
                , fsp.ServiceDescription as FspDescription

                , sog.SogDescription
                , dur.Name as StdDuration

        from cte m 
        inner join InputAtoms.Country cnt on cnt.id = m.CountryId
        inner join InputAtoms.WgSogView sog on sog.Id = m.WgId
        inner join Dependencies.Duration dur on dur.Id = m.StdWarranty
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    c.Country
            , c.ISO3CountryCode
            , c.Fsp
            , c.FspDescription

            , c.SogDescription
            , c.Sog        
            , c.Wg

            , c.ServiceLocation
            , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
            , case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
            , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

            , c.StdDuration 
            , c.StdWarrantyLocation
            , c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
            , case when @approved = 1 then c.ServiceTpSog else c.ServiceTpSog_Released end as ServiceTP
            , c.DealerPrice
            , c.ListPrice
    into #tmp
    from cte2 c

    --Calc for non-Emeia countries by WG

    declare @nonEmeiaCnt dbo.ListID;
    declare @nonEmeiaWg dbo.ListID;

    insert into @nonEmeiaCnt(id)
    select c.Id
    from InputAtoms.Country c 
    join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
    where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 0;

    insert into @nonEmeiaWg(id)
    select id
    from InputAtoms.Wg wg 
    where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
          and (@sog is null or wg.SogId = @sog)
          and IsSoftware = 0
          and DeactivatedDateTime is null;

    insert into #tmp
    select    c.Country
            , cnt.ISO3CountryCode
            , fsp.Name as Fsp
            , fsp.ServiceDescription as FspDescription

            , sog.SogDescription
            , c.Sog        
            , c.Wg

            , c.ServiceLocation
            , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
            , case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
            , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

            , dur.Name as StdDuration
            , c.StdWarrantyLocation 
            , c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
            , case when @approved = 1 then c.ServiceTP else c.ServiceTP_Released end as ServiceTP
            , c.DealerPrice
            , c.ListPrice
    from Hardware.GetCosts(1, @nonEmeiaCnt, @nonEmeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro, null, null) c
    inner join InputAtoms.Country cnt on cnt.id = c.CountryId
    inner join InputAtoms.WgSogView sog on sog.Id = c.WgId
    inner join Dependencies.Duration dur on dur.Id = c.StdWarranty
    left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = c.SlaHash and fsp.Sla = c.Sla;

    if @limit > 0
        select * from (
            select ROW_NUMBER() over(order by Country,Wg) as rownum, * from #tmp
        ) t
        where rownum > @lastid and rownum <= @lastid + @limit;
    else
        select * from #tmp order by Country,Wg;

END
GO


IF OBJECT_ID('Report.spLocapGlobalSupportApproved') IS NOT NULL
  DROP PROCEDURE Report.spLocapGlobalSupportApproved;
go 

CREATE PROCEDURE [Report].[spLocapGlobalSupportApproved]
(
    @cnt     dbo.ListID readonly,
    @sog     bigint,
    @wg      dbo.ListID readonly,
    @av      dbo.ListID readonly,
    @dur     dbo.ListID readonly,
    @rtime   dbo.ListID readonly,
    @rtype   dbo.ListID readonly,
    @loc     dbo.ListID readonly,
    @lastid  int,
    @limit   int
)
AS
BEGIN
    declare @pro dbo.ListId; insert into @pro(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';
    exec Report.spLocapGlobalSupport 1, @cnt, @sog, @wg, @av, @dur, @rtime, @rtype, @loc, @pro, @lastid, @limit;
END
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = ('Locap-Global-Support-approved'));
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'FspDescription', 'Portfolio Alignment', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ISO3CountryCode', 'Country specific order code', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Service Offering Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceProduct', 'Service Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarrantyLocation', 'Standard Warranty Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdDuration', 'Standard Warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'LocalServiceStandardWarranty', 'Standard Warranty cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ServiceTP', 'Service TP (Full cost)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'DealerPrice', 'Dealer Price (local input) for non-FTS countries', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ListPrice', 'List Price (local input) for non-FTS countries', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country'         , 1), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('sog'             , 0), 'sog', 'SOG');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg'              , 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability'    , 1), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('duration'        , 1), 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontime'    , 1), 'rtime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype'    , 1), 'rtype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation' , 1), 'loc', 'Service location');



IF OBJECT_ID('[Report].[spLocapGlobalSupportReleased]') IS NOT NULL
  DROP PROCEDURE [Report].[spLocapGlobalSupportReleased];
go 

CREATE PROCEDURE [Report].[spLocapGlobalSupportReleased]
(
    @cnt     dbo.ListID readonly,
    @sog     bigint,
    @wg      dbo.ListID readonly,
    @av      dbo.ListID readonly,
    @dur     dbo.ListID readonly,
    @rtime   dbo.ListID readonly,
    @rtype   dbo.ListID readonly,
    @loc     dbo.ListID readonly,
    @lastid  int,
    @limit   int
)
AS
BEGIN

    if OBJECT_ID('tempdb..#tmp') is not null drop table #tmp;

    --Calc for Emeia countries by SOG

    declare @emeiaCnt dbo.ListID;
    declare @emeiaWg dbo.ListId;

    insert into @emeiaCnt(id)
    select c.Id
    from InputAtoms.Country c 
    join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
    where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 1;

    insert into @emeiaWg
    select id
    from InputAtoms.Wg 
    where (@sog is null or SogId = @sog)
    and SogId in (select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id)))
    and IsSoftware = 0
    and SogId is not null
    and DeactivatedDateTime is null;

    declare @pro dbo.ListId; insert into @pro(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
                , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSog(1, @emeiaCnt, @emeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro) m
        where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
              and m.ServiceTpSog_Released is not null
    )
    , cte2 as (
        select    m.*
                , cnt.ISO3CountryCode
                , fsp.Name as Fsp
                , fsp.ServiceDescription as FspDescription

                , sog.Description as SogDescription
                , dur.Name as StdDuration

        from cte m
        inner join InputAtoms.Country cnt on cnt.id = m.CountryId
        inner join InputAtoms.Sog sog on sog.Id = m.SogId
        inner join Dependencies.Duration dur on dur.Id = m.StdWarranty
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    c.Country
            , c.ISO3CountryCode
            , c.Fsp
            , c.FspDescription

            , c.SogDescription
            , c.Sog        
            , c.Wg

            , c.ServiceLocation
            , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
            , case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
            , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

            , c.StdDuration 
            , c.StdWarrantyLocation
            , c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
            , c.ServiceTpSog_Released as ServiceTP
            , c.DealerPrice
            , c.ListPrice

            , c.ReleaseDate
            , c.ReleaseUser as ReleasedBy

    into #tmp
    from cte2 c

    --Calc for non-Emeia countries by WG

    declare @nonEmeiaCnt dbo.ListID;
    declare @nonEmeiaWg dbo.ListID;

    insert into @nonEmeiaCnt(id)
    select c.Id
    from InputAtoms.Country c 
    join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
    where exists (select * from @cnt where id = c.Id) and cr.IsEmeia = 0;

    insert into @nonEmeiaWg(id)
    select id
    from InputAtoms.Wg wg 
    where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
          and (@sog is null or wg.SogId = @sog)
          and IsSoftware = 0
          and DeactivatedDateTime is null;

    insert into #tmp
    select    c.Country
            , cnt.ISO3CountryCode
            , fsp.Name as Fsp
            , fsp.ServiceDescription as FspDescription

            , sog.Description as SogDescription
            , c.Sog        
            , c.Wg

            , c.ServiceLocation
            , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
            , case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
            , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

            , dur.Name as StdDuration
            , c.StdWarrantyLocation 
            , c.LocalServiceStandardWarrantyWithRisk as LocalServiceStandardWarranty
            , c.ServiceTP_Released as ServiceTP
            , c.DealerPrice
            , c.ListPrice

            , c.ReleaseDate
            , c.ReleaseUserName as ReleasedBy

    from Hardware.GetCosts(1, @nonEmeiaCnt, @nonEmeiaWg, @av, @dur, @rtime, @rtype, @loc, @pro, null, null) c
    inner join InputAtoms.Country cnt on cnt.id = c.CountryId
    inner join InputAtoms.Sog sog on sog.Id = c.SogId
    inner join Dependencies.Duration dur on dur.Id = c.StdWarranty
    left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = c.SlaHash and fsp.Sla = c.Sla
    where c.ServiceTP_Released is not null;

    if @limit > 0
        select * from (
            select ROW_NUMBER() over(order by Country,Wg) as rownum, * from #tmp
        ) t
        where rownum > @lastid and rownum <= @lastid + @limit;
    else
        select * from #tmp order by Country,Wg;
END

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = ('Locap-Global-Support'));
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'FspDescription', 'Portfolio Alignment', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ISO3CountryCode', 'Country specific order code', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Service Offering Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceProduct', 'Service Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarrantyLocation', 'Standard Warranty Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdDuration', 'Standard Warranty duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'LocalServiceStandardWarranty', 'Standard Warranty cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ServiceTP', 'Service TP (Full cost)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'DealerPrice', 'Dealer Price (local input) for non-FTS countries', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ListPrice', 'List Price (local input) for non-FTS countries', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('datetime'), 'ReleaseDate', 'Release date', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReleasedBy', 'Released by', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country'         , 1), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('sog'             , 0), 'sog', 'SOG');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg'              , 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability'    , 1), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('duration'        , 1), 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontime'    , 1), 'rtime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype'    , 1), 'rtype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation' , 1), 'loc', 'Service location');

GO

