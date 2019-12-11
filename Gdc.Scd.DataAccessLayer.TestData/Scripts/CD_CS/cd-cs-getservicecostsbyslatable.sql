IF OBJECT_ID('[Report].[GetServiceCostsBySlaTable]') is not null
    drop procedure [Report].[GetServiceCostsBySlaTable]
GO

IF type_id('[Report].[SlaString]') IS NOT NULL
    DROP TYPE [Report].[SlaString];
GO

CREATE TYPE [Report].[SlaString] AS TABLE(
    Wg              nvarchar(255),
    Availability    nvarchar(255),
    Duration        nvarchar(255),
    ReactionTime    nvarchar(255),
    ReactionType    nvarchar(255),
    ServiceLocation nvarchar(255),
    ProActiveSla    nvarchar(255) 
)
GO

CREATE PROCEDURE [Report].[GetServiceCostsBySlaTable]
(
    @cnt bigint,
    @sla Report.SlaString readonly
)
AS
BEGIN

    declare @pro bigint = (select id from Dependencies.ProActiveSla where upper(ExternalName) = 'NONE');

    declare @tmp table (
          CountryId bigint
        , WgId              bigint    
        , AvailabilityId    bigint
        , DurationId        bigint
        , ReactionTimeId    bigint
        , ReactionTypeId    bigint
        , ServiceLocationId bigint
        , ProactiveSlaId    bigint

        , Wg              nvarchar(255)    
        , Availability    nvarchar(255)
        , Duration        nvarchar(255)
        , ReactionTime    nvarchar(255)
        , ReactionType    nvarchar(255)
        , ServiceLocation nvarchar(255)

        , AvailabilityOrder int not null
        , ReactionTimeOrder int not null
        , ReactionTypeOrder int not null

        , Sla AS cast(CountryId         as nvarchar(20)) + ',' +
                 cast(WgId              as nvarchar(20)) + ',' +
                 cast(AvailabilityId    as nvarchar(20)) + ',' +
                 cast(DurationId        as nvarchar(20)) + ',' +
                 cast(ReactionTimeId    as nvarchar(20)) + ',' +
                 cast(ReactionTypeId    as nvarchar(20)) + ',' +
                 cast(ServiceLocationId as nvarchar(20)) + ',' +
                 cast(ProactiveSlaId    as nvarchar(20)) persisted

        , SlaHash  AS checksum (
                cast(CountryId         as nvarchar(20)) + ',' +
                cast(WgId              as nvarchar(20)) + ',' +
                cast(AvailabilityId    as nvarchar(20)) + ',' +
                cast(DurationId        as nvarchar(20)) + ',' +
                cast(ReactionTimeId    as nvarchar(20)) + ',' +
                cast(ReactionTypeId    as nvarchar(20)) + ',' +
                cast(ServiceLocationId as nvarchar(20)) + ',' +
                cast(ProactiveSlaId    as nvarchar(20)) 
            ) persisted

        , PRIMARY KEY CLUSTERED (SlaHash, Sla)
    );

    insert into @tmp 
    select    @cnt as       CountryId
            , wg.Id as      WgId
            , av.id as      AvailabilityId
            , dur.Id as     DurationId
            , rtime.Id as   ReactionTimeId
            , rtype.Id as   ReactionTypeId
            , loc.Id as     ServiceLocationId
            , @pro as       ProActiveId

            , wg.Name as    Wg
            , av.Name as    Availability
            , dur.Name as   Duration
            , rtime.Name as ReactionTime
            , rtype.Name    ReactionType
            , loc.Name      ServiceLocation

            , case when av.Name = '9x5' then 0 else 1 end as AvailabilityOrder
            , case when rtime.Name = 'NBD' then 0
                   when rtime.Name = '24h' then 1
                   when rtime.Name = '4h'  then 2
                   else 3 end as ReactionTimeOrder

            , case when rtype.Name = 'response' then 0 else 1 end as ReactionTypeOrder

    from @sla p
    inner join InputAtoms.Wg wg on UPPER(wg.Name) = p.Wg
    inner join Dependencies.Availability av on UPPER(av.Name) = p.Availability or av.ExternalName like '%' + p.Availability + '%'
    inner join Dependencies.Duration dur on UPPER(dur.Name) = p.Duration and dur.IsProlongation = 0
    inner join Dependencies.ReactionType rtype on UPPER(rtype.ExternalName) = p.ReactionType
    inner join Dependencies.ReactionTime rtime on UPPER(rtime.ExternalName) = p.ReactionTime
    inner join Dependencies.ServiceLocation loc on UPPER(loc.ExternalName) = p.ServiceLocation;

    declare @cntId dbo.ListId;
    declare @locId dbo.ListId;
    declare @avId dbo.ListId;
    declare @reactiontimeId dbo.ListId;
    declare @reactiontypeId dbo.ListId;
    declare @wgId dbo.ListId;
    declare @durId dbo.ListId;
    declare @proId dbo.ListId;

    insert into @cntId values (@cnt);
    insert into @wgId select id from InputAtoms.Wg wg where exists (select * from @tmp where WgId = wg.Id);
    insert into @avId select id from Dependencies.Availability a where exists (select * from @tmp where AvailabilityId = a.Id);
    insert into @durId select id from Dependencies.Duration d where exists (select * from @tmp where DurationId = d.Id);
    insert into @reactiontimeId select id from Dependencies.ReactionTime r where exists (select * from @tmp where ReactionTimeId = r.Id);
    insert into @reactiontypeId select id from Dependencies.ReactionType r where exists (select * from @tmp where ReactionTypeId = r.Id);
    insert into @locId select id from Dependencies.ServiceLocation l where exists (select * from @tmp where ServiceLocationId = l.Id);
    insert into @proId values (@pro);

    declare @cntGroup nvarchar(255) = (select Name from InputAtoms.CountryGroup cg
                                            where Id = (select CountryGroupId from InputAtoms.Country where Id = @cnt)
                                        );

    select    sla.ReactionTime + sla.ReactionType + sla.Availability + sla.Wg as 'Key'
            , @cntGroup as CountryGroup
            , sla.ServiceLocation
            , sla.Availability
            , sla.ReactionTime
            , sla.ReactionType
            , sla.Wg
            , sla.Duration
            , coalesce(c.ServiceTCManual, c.ServiceTC) as ServiceTC
            , c.ServiceTP_Released as ServiceTP
            , case when c.Year >= 1 then  (mc.ServiceTP1_Released * c.ExchangeRate) / c.Year / 12 else null end as ServiceTPMonthly1
            , case when c.Year >= 2 then  (mc.ServiceTP2_Released * c.ExchangeRate) / c.Year / 12 else null end as ServiceTPMonthly2
            , case when c.Year >= 3 then  (mc.ServiceTP3_Released * c.ExchangeRate) / c.Year / 12 else null end as ServiceTPMonthly3
            , case when c.Year >= 4 then  (mc.ServiceTP4_Released * c.ExchangeRate) / c.Year / 12 else null end as ServiceTPMonthly4
            , case when c.Year >= 5 then  (mc.ServiceTP5_Released * c.ExchangeRate) / c.Year / 12 else null end as ServiceTPMonthly5
    from @tmp sla 
    left join Hardware.GetCosts(1, @cntId, @wgId, @avId, @durId, @reactiontimeId, @reactiontypeId, @locId, @proId, 0, -1) c 
        on sla.SlaHash = c.SlaHash and sla.Sla = c.Sla COLLATE SQL_Latin1_General_CP1_CI_AS
    left join Hardware.ManualCost mc on mc.PortfolioId = c.Id
    
    order by Wg, AvailabilityOrder, ReactionTimeOrder, ReactionTypeOrder, Availability, ReactionTime;

END