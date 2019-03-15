DROP INDEX IX_PrincipalPortfolio_AvailabilityId    ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_DurationId        ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ReactionTimeId    ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ReactionTypeId    ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ServiceLocationId ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_WgId              ON Portfolio.PrincipalPortfolio;  
DROP INDEX IX_PrincipalPortfolio_ProActiveSlaId    ON Portfolio.PrincipalPortfolio;  
go

CREATE NONCLUSTERED INDEX [IX_PrincipalPortfolio_SLA] ON [Portfolio].[PrincipalPortfolio]
(
    [AvailabilityId] ASC,
    [DurationId] ASC,
    [ProActiveSlaId] ASC,
    [ReactionTimeId] ASC,
    [ReactionTypeId] ASC,
    [ServiceLocationId] ASC,
    [WgId] ASC
)
INCLUDE ([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE Portfolio.LocalPortfolio
    add  ReactionTime_Avalability              bigint
       , ReactionTime_ReactionType             bigint
       , ReactionTime_ReactionType_Avalability bigint
       , Sla AS cast(CountryId         as nvarchar(20)) + 
                    cast(WgId              as nvarchar(20)) + 
                    cast(AvailabilityId    as nvarchar(20)) + 
                    cast(DurationId        as nvarchar(20)) + 
                    cast(ReactionTimeId    as nvarchar(20)) + 
                    cast(ReactionTypeId    as nvarchar(20)) + 
                    cast(ServiceLocationId as nvarchar(20)) + 
                    cast(ProactiveSlaId    as nvarchar(20))

       , SlaHash  AS checksum (
                    cast(CountryId         as nvarchar(20)) + 
                    cast(WgId              as nvarchar(20)) + 
                    cast(AvailabilityId    as nvarchar(20)) + 
                    cast(DurationId        as nvarchar(20)) + 
                    cast(ReactionTimeId    as nvarchar(20)) + 
                    cast(ReactionTypeId    as nvarchar(20)) + 
                    cast(ServiceLocationId as nvarchar(20)) + 
                    cast(ProactiveSlaId    as nvarchar(20)) 
                );
GO

update p set  ReactionTime_Avalability = rta.Id
            , ReactionTime_ReactionType = rtt.Id
            , ReactionTime_ReactionType_Avalability = rtta.Id
from Portfolio.LocalPortfolio p
join Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = p.AvailabilityId and rta.ReactionTimeId = p.ReactionTimeId
join Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = p.ReactionTimeId and rtt.ReactionTypeId = p.ReactionTypeId
join Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = p.AvailabilityId and rtta.ReactionTimeId = p.ReactionTimeId and rtta.ReactionTypeId = p.ReactionTypeId
go

ALTER TABLE Portfolio.LocalPortfolio ALTER column ReactionTime_Avalability bigint NOT NULL;
ALTER TABLE Portfolio.LocalPortfolio ALTER column ReactionTime_ReactionType bigint NOT NULL;
ALTER TABLE Portfolio.LocalPortfolio ALTER column ReactionTime_ReactionType_Avalability bigint NOT NULL;
go

DROP INDEX [IX_HwFspCodeTranslation_AvailabilityId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_CountryId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_DurationId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ProactiveSlaId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ReactionTimeId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ReactionTypeId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_ServiceLocationId] ON [Fsp].[HwFspCodeTranslation]
GO
DROP INDEX [IX_HwFspCodeTranslation_WgId] ON [Fsp].[HwFspCodeTranslation]
GO

ALTER TABLE Fsp.HwFspCodeTranslation 
    ADD Sla  AS cast(coalesce(CountryId, 0) as nvarchar(20)) + 
                cast(WgId                   as nvarchar(20)) + 
                cast(AvailabilityId         as nvarchar(20)) + 
                cast(DurationId             as nvarchar(20)) + 
                cast(ReactionTimeId         as nvarchar(20)) + 
                cast(ReactionTypeId         as nvarchar(20)) + 
                cast(ServiceLocationId      as nvarchar(20)) + 
                cast(ProactiveSlaId         as nvarchar(20))
      
      , SlaHash  AS checksum (
                        cast(coalesce(CountryId, 0) as nvarchar(20)) + 
                        cast(WgId                   as nvarchar(20)) + 
                        cast(AvailabilityId         as nvarchar(20)) + 
                        cast(DurationId             as nvarchar(20)) + 
                        cast(ReactionTimeId         as nvarchar(20)) + 
                        cast(ReactionTypeId         as nvarchar(20)) + 
                        cast(ServiceLocationId      as nvarchar(20)) + 
                        cast(ProactiveSlaId         as nvarchar(20))
                    );
GO

CREATE INDEX IX_HwFspCodeTranslation_Sla ON Fsp.HwFspCodeTranslation(Sla);
GO

CREATE INDEX IX_HwFspCodeTranslation_SlaHash ON Fsp.HwFspCodeTranslation(SlaHash);
GO

IF OBJECT_ID('Portfolio.AllowPrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.AllowPrincipalPortfolio;
go

IF OBJECT_ID('Portfolio.AllowPrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.AllowPrincipalPortfolio;
go

IF OBJECT_ID('Portfolio.DenyPrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.DenyPrincipalPortfolio;
go

IF OBJECT_ID('Portfolio.DenyLocalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.DenyLocalPortfolio;
go

IF OBJECT_ID('Portfolio.DenyLocalPortfolioById') IS NOT NULL
  DROP PROCEDURE Portfolio.DenyLocalPortfolioById;
go

IF OBJECT_ID('Portfolio.AllowLocalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.AllowLocalPortfolio;
go

IF OBJECT_ID('Portfolio.CreatePrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.CreatePrincipalPortfolio;
go

IF OBJECT_ID('Portfolio.UpdatePrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.UpdatePrincipalPortfolio;
go

IF OBJECT_ID('Portfolio.DeletePrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.DeletePrincipalPortfolio;
go

IF OBJECT_ID('Portfolio.GenSla') IS NOT NULL
  DROP FUNCTION Portfolio.GenSla;
go

IF OBJECT_ID('Portfolio.IsListEmpty') IS NOT NULL
  DROP FUNCTION Portfolio.IsListEmpty;
go

IF OBJECT_ID('Portfolio.GetListOrNull') IS NOT NULL
  DROP FUNCTION Portfolio.GetListOrNull;
go

IF TYPE_ID('dbo.ListID') IS NOT NULL
  DROP Type dbo.ListID;
go

IF OBJECT_ID('[Portfolio].[GetBySla]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySla];
go 

CREATE TYPE dbo.ListID AS TABLE(
    id bigint NULL
)
go

CREATE TYPE [Portfolio].[Sla] AS TABLE(
    [rownum] [int] NOT NULL,
    [Id] [bigint] NOT NULL,
    [CountryId] [bigint] NOT NULL,
    [WgId] [bigint] NOT NULL,
    [AvailabilityId] [bigint] NOT NULL,
    [DurationId] [bigint] NOT NULL,
    [ReactionTimeId] [bigint] NOT NULL,
    [ReactionTypeId] [bigint] NOT NULL,
    [ServiceLocationId] [bigint] NOT NULL,
    [ProActiveSlaId] [bigint] NOT NULL,
    [Sla] [nvarchar](255) NOT NULL,
    [SlaHash] [int] NOT NULL,
    [ReactionTime_Avalability] [bigint] NOT NULL,
    [ReactionTime_ReactionType] [bigint] NOT NULL,
    [ReactionTime_ReactionType_Avalability] [bigint] NOT NULL,
    [Fsp] [nvarchar](255) NULL,
    [FspDescription] [nvarchar](255) NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_LocalPortfolio_Country_Wg] ON [Portfolio].[LocalPortfolio]
(
    [CountryId] ASC,
    [WgId] ASC
)
INCLUDE (
    [AvailabilityId],
    [DurationId],
    [ProActiveSlaId],
    [ReactionTimeId],
    [ReactionTypeId],
    [ServiceLocationId],
    [ReactionTime_ReactionType],
    [ReactionTime_Avalability],
    [ReactionTime_ReactionType_Avalability]
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE FUNCTION Portfolio.IsListEmpty(@list dbo.ListID readonly)
RETURNS bit
AS
BEGIN

    declare @result bit = 1;

    if exists(select 1 from @list)
       set @result = 0;
   
    RETURN @result;

END
go

CREATE FUNCTION Portfolio.GetListOrNull(@list dbo.ListID readonly)
RETURNS @tbl table(id bigint)
AS
BEGIN

    insert into @tbl(id) select id from @list;

	if not exists (select 1 from @tbl)
        insert into @tbl (id) values (null);
	
	RETURN 
END
go    

CREATE FUNCTION Portfolio.GenSla (
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly
)
RETURNS @sla TABLE   
(  
     WG bigint not null
   , Availability bigint not null
   , Duration bigint not null
   , ReactionType bigint not null
   , ReactionTime bigint not null
   , ServiceLocation bigint not null
   , ProActive bigint not null
)  
AS  
BEGIN 

    declare @isEmptyWG    bit = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv    bit = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur   bit = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@rtype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@rtime);
    declare @isEmptyLoc   bit = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro   bit = Portfolio.IsListEmpty(@pro);

    with WgCte as (
        select Id from InputAtoms.Wg 
        where WgType = 1 and DeactivatedDateTime is null and (@isEmptyWG = 1 or Id in (select id from @wg))
    )
    , AvCte as (
        select Id from Dependencies.Availability where (@isEmptyAv = 1 or Id in (select id from @av))
    )
    , DurCte as (
        select Id from Dependencies.Duration where (@isEmptyDur = 1 or Id in (select id from @dur))
    )
    , RtypeCte as (
        select Id from Dependencies.ReactionType where (@isEmptyRType = 1 or Id in (select id from @rtype))
    )
    , RtimeCte as (
        select Id from Dependencies.ReactionTime where (@isEmptyRTime = 1 or Id in (select id from @rtime))
    )
    , LocCte as (
        select Id from Dependencies.ServiceLocation where (@isEmptyLoc = 1 or Id in (select id from @loc))
    )
    , ProCte as (
        select Id from Dependencies.ProActiveSla where (@isEmptyPro = 1 or Id in (select id from @pro))
    )
    INSERT into @sla (WG, Availability, Duration, ReactionType, ReactionTime, ServiceLocation, ProActive)
        SELECT wg.Id, av.Id, dur.Id, rtype.Id, rtime.Id, loc.Id, pro.id
        FROM WgCte wg
           , AvCte av
           , DurCte dur
           , RtypeCte rtype
           , RtimeCte rtime
           , LocCte loc
           , ProCte pro;

   RETURN;
END; 
go

CREATE PROCEDURE Portfolio.DenyLocalPortfolio
    @cnt bigint,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;

    declare @isEmptyWG bit    = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv bit    = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur bit   = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@rtype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@rtime);
    declare @isEmptyLoc bit   = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro bit   = Portfolio.IsListEmpty(@pro);

    DELETE FROM Portfolio.LocalPortfolio
    WHERE   (CountryId = @cnt)

        AND (@isEmptyWG = 1 or WgId in (select id from @wg))
        AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
        AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
        AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @rtime))
        AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @rtype))
        AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
        AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))
END
go

CREATE PROCEDURE Portfolio.DenyLocalPortfolioById
    @ids dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;

    DELETE FROM Portfolio.LocalPortfolio
    WHERE (Id in (select Id from @ids));

END
go

CREATE PROCEDURE Portfolio.AllowLocalPortfolio
    @cnt bigint,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;

    declare @isEmptyWG bit    = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv bit    = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur bit   = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@rtype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@rtime);
    declare @isEmptyLoc bit   = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro bit   = Portfolio.IsListEmpty(@pro);

    -- Disable all table constraints
    ALTER TABLE Portfolio.LocalPortfolio NOCHECK CONSTRAINT ALL;

    with ExistSlaCte as (

        --select all existing portfolio

        SELECT Id, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId
        FROM Portfolio.LocalPortfolio
        WHERE   (CountryId = @cnt)

            AND (@isEmptyWG = 1 or WgId in (select id from @wg))
            AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
            AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
            AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @rtime))
            AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @rtype))
            AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
            AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))
    )
    , PrincipleSlaCte as (

        --find current principle portfolio

        select WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId
        FROM Portfolio.PrincipalPortfolio
        WHERE   (@isEmptyWG = 1 or WgId in (select id from @wg))
            AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
            AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
            AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @rtime))
            AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @rtype))
            AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
            AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))
    )
    INSERT INTO Portfolio.LocalPortfolio (CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId, ReactionTime_Avalability, ReactionTime_ReactionType, ReactionTime_ReactionType_Avalability)
    SELECT @cnt, sla.WgId, sla.AvailabilityId, sla.DurationId, sla.ReactionTypeId, sla.ReactionTimeId, sla.ServiceLocationId, sla.ProActiveSlaId, rta.Id, rtt.Id, rtta.Id
    FROM PrincipleSlaCte sla
    JOIN Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = sla.AvailabilityId and rta.ReactionTimeId = sla.ReactionTimeId
    JOIN Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = sla.ReactionTimeId and rtt.ReactionTypeId = sla.ReactionTypeId
    JOIN Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = sla.AvailabilityId and rtta.ReactionTimeId = sla.ReactionTimeId and rtta.ReactionTypeId = sla.ReactionTypeId

    LEFT JOIN ExistSlaCte ex on ex.WgId = sla.WgId
                            and ex.AvailabilityId = sla.AvailabilityId
                            and ex.DurationId = sla.DurationId
                            and ex.ReactionTypeId = sla.ReactionTypeId
                            and ex.ReactionTimeId = sla.ReactionTimeId
                            and ex.ServiceLocationId = sla.ServiceLocationId
                            and ex.ProActiveSlaId = sla.ProActiveSlaId

    where ex.Id is null; --exclude existing portfolio
    
    -- Enable all table constraints
    ALTER TABLE Portfolio.LocalPortfolio CHECK CONSTRAINT ALL;

END
go

CREATE PROCEDURE Portfolio.CreatePrincipalPortfolio
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;

    declare @isEmptyWG bit    = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv bit    = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur bit   = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@rtype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@rtime);
    declare @isEmptyLoc bit   = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro bit   = Portfolio.IsListEmpty(@pro);

    -- Disable all table constraints
    ALTER TABLE Portfolio.PrincipalPortfolio NOCHECK CONSTRAINT ALL;

    with ExistSlaCte as (

        --select all existing portfolio

        SELECT Id, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId
        FROM Portfolio.PrincipalPortfolio
        WHERE   (@isEmptyWG = 1 or WgId in (select id from @wg))
            AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
            AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
            AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @rtime))
            AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @rtype))
            AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
            AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))
    )
    INSERT INTO Portfolio.PrincipalPortfolio (
                WgId
            , AvailabilityId
            , DurationId
            , ReactionTypeId
            , ReactionTimeId
            , ServiceLocationId
            , ProActiveSlaId
            , IsGlobalPortfolio
            , IsMasterPortfolio
            , IsCorePortfolio
        )

    --insert new portfolio only

    SELECT WG, Availability, Duration, ReactionType, ReactionTime, ServiceLocation, ProActive, 1, 0, 0
    FROM Portfolio.GenSla(@wg, @av, @dur, @rtype, @rtime, @loc, @pro) sla
    LEFT JOIN ExistSlaCte ex on ex.WgId = sla.WG
                            and ex.AvailabilityId = sla.Availability
                            and ex.DurationId = sla.Duration
                            and ex.ReactionTypeId = sla.ReactionType
                            and ex.ReactionTimeId = sla.ReactionTime
                            and ex.ServiceLocationId = sla.ServiceLocation
                            and ex.ProActiveSlaId = sla.ProActive

    where ex.Id is null; --exclude existing portfolio

    -- Enable all table constraints
    ALTER TABLE Portfolio.PrincipalPortfolio CHECK CONSTRAINT ALL;

END

go

CREATE PROCEDURE Portfolio.UpdatePrincipalPortfolio
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @globalPortfolio bit, 
    @masterPortfolio bit, 
    @corePortfolio bit
AS
BEGIN

    SET NOCOUNT ON;

    declare @isEmptyWG bit    = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv bit    = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur bit   = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@rtype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@rtime);
    declare @isEmptyLoc bit   = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro bit   = Portfolio.IsListEmpty(@pro);

    --unset portfolio flag for master and core only

    UPDATE Portfolio.PrincipalPortfolio
        SET   IsMasterPortfolio =  case when @masterPortfolio is not null then @masterPortfolio else IsMasterPortfolio end
            , IsCorePortfolio   =  case when @corePortfolio   is not null then @corePortfolio   else IsCorePortfolio end
    WHERE   (@isEmptyWG = 1 or WgId in (select id from @wg))
        AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
        AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
        AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @rtime))
        AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @rtype))
        AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
        AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro));

END
go

CREATE PROCEDURE Portfolio.DeletePrincipalPortfolio
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;

    declare @isEmptyWG bit    = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv bit    = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur bit   = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@rtype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@rtime);
    declare @isEmptyLoc bit   = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro bit   = Portfolio.IsListEmpty(@pro);

    DELETE FROM Portfolio.PrincipalPortfolio
    WHERE   (@isEmptyWG = 1 or WgId in (select id from @wg))
        AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
        AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
        AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @rtime))
        AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @rtype))
        AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
        AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro));

END
go

CREATE PROCEDURE Portfolio.DenyPrincipalPortfolio
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @globalPortfolio bit, 
    @masterPortfolio bit, 
    @corePortfolio bit
AS
BEGIN

    SET NOCOUNT ON;

    if @globalPortfolio = 1
        begin
            exec Portfolio.DeletePrincipalPortfolio @wg, @av, @dur, @rtype, @rtime, @loc, @pro;
        end
    else
        begin
           set @masterPortfolio = case when @masterPortfolio = 1 then 0 else null end;
           set @corePortfolio = case when @corePortfolio = 1 then 0 else null end;

            --unset portfolio flag for master and core only
           exec Portfolio.UpdatePrincipalPortfolio @wg, @av, @dur, @rtype, @rtime, @loc, @pro, null, @masterPortfolio, @corePortfolio;
        end

END
go

CREATE PROCEDURE Portfolio.AllowPrincipalPortfolio
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @rtype dbo.ListID readonly,
    @rtime dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @globalPortfolio bit, 
    @masterPortfolio bit, 
    @corePortfolio bit
AS
BEGIN

    SET NOCOUNT ON;

    if @globalPortfolio = 1
    begin
        --first insert new portfolio
        exec Portfolio.CreatePrincipalPortfolio @wg, @av, @dur, @rtype, @rtime, @loc, @pro;
    end

    --and set portfolio flag for master and core only

    set @masterPortfolio = case when @masterPortfolio = 1 then 1 else null end;
    set @corePortfolio   = case when @corePortfolio = 1 then 1 else null end;

    exec Portfolio.UpdatePrincipalPortfolio @wg, @av, @dur, @rtype, @rtime, @loc, @pro, null, @masterPortfolio, @corePortfolio;

END

go


IF OBJECT_ID('[Portfolio].[GetBySla]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySla];
go 

CREATE FUNCTION [Portfolio].[GetBySla](
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN 
(
    select m.*
    from Portfolio.LocalPortfolio m
    where   exists(select id from @cnt where id = m.CountryId)

        AND (not exists(select 1 from @wg           ) or exists(select 1 from @wg           where id = m.WgId              ))
        AND (not exists(select 1 from @av           ) or exists(select 1 from @av           where id = m.AvailabilityId    ))
        AND (not exists(select 1 from @dur          ) or exists(select 1 from @dur          where id = m.DurationId        ))
        AND (not exists(select 1 from @reactiontime ) or exists(select 1 from @reactiontime where id = m.ReactionTimeId    ))
        AND (not exists(select 1 from @reactiontype ) or exists(select 1 from @reactiontype where id = m.ReactionTypeId    ))
        AND (not exists(select 1 from @loc          ) or exists(select 1 from @loc          where id = m.ServiceLocationId ))
        AND (not exists(select 1 from @pro          ) or exists(select 1 from @pro          where id = m.ProActiveSlaId    ))
)
GO

IF OBJECT_ID('[Portfolio].[GetBySlaPaging]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaPaging];
go 

CREATE FUNCTION [Portfolio].[GetBySlaPaging](
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @lastid       bigint,
    @limit        int
)
RETURNS @tbl TABLE 
            (   
                [rownum] [int] NOT NULL,
                [Id] [bigint] NOT NULL,
                [CountryId] [bigint] NOT NULL,
                [WgId] [bigint] NOT NULL,
                [AvailabilityId] [bigint] NOT NULL,
                [DurationId] [bigint] NOT NULL,
                [ReactionTimeId] [bigint] NOT NULL,
                [ReactionTypeId] [bigint] NOT NULL,
                [ServiceLocationId] [bigint] NOT NULL,
                [ProActiveSlaId] [bigint] NOT NULL,
                [Sla] nvarchar(255) NOT NULL,
                [SlaHash] [int] NOT NULL,
                [ReactionTime_Avalability] [bigint] NOT NULL,
                [ReactionTime_ReactionType] [bigint] NOT NULL,
                [ReactionTime_ReactionType_Avalability] [bigint] NOT NULL,
                [Fsp] nvarchar(255) NULL,
                [FspDescription] nvarchar(255) NULL
            )
AS
BEGIN
    
    if @limit > 0
    begin
        insert into @tbl
        select   rownum
               , Id
               , CountryId
               , WgId
               , AvailabilityId
               , DurationId
               , ReactionTimeId
               , ReactionTypeId
               , ServiceLocationId
               , ProActiveSlaId
               , Sla
               , SlaHash
               , ReactionTime_Avalability
               , ReactionTime_ReactionType
               , ReactionTime_ReactionType_Avalability
               , null
               , null
        from (
                select ROW_NUMBER() over(
                            order by m.CountryId
                                    , m.WgId
                                    , m.AvailabilityId
                                    , m.DurationId
                                    , m.ReactionTimeId
                                    , m.ReactionTypeId
                                    , m.ServiceLocationId
                                    , m.ProActiveSlaId
                        ) as rownum
                        , m.*
                from Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m    
        ) t
        where rownum > @lastid and rownum <= @lastid + @limit;
    end
    else
    begin
        insert into @tbl 
        select   -1
               , Id
               , CountryId
               , WgId
               , AvailabilityId
               , DurationId
               , ReactionTimeId
               , ReactionTypeId
               , ServiceLocationId
               , ProActiveSlaId
               , Sla
               , SlaHash
               , ReactionTime_Avalability
               , ReactionTime_ReactionType
               , ReactionTime_ReactionType_Avalability
               , null
               , null
        from Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    end 

    RETURN;
END;
go

IF OBJECT_ID('[Portfolio].[GetBySlaFsp]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaFsp];
go 

CREATE FUNCTION [Portfolio].[GetBySlaFsp](
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN 
(
    select    m.*
            , fsp.Name               as Fsp
            , fsp.ServiceDescription as FspDescription
    from Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    left JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla 
)
GO

IF OBJECT_ID('[Portfolio].[GetBySlaFspPaging]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaFspPaging];
go 

CREATE FUNCTION [Portfolio].[GetBySlaFspPaging](
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @lastid       bigint,
    @limit        int
)
RETURNS @tbl TABLE 
            (   
                [rownum] [int] NOT NULL,
                [Id] [bigint] NOT NULL,
                [CountryId] [bigint] NOT NULL,
                [WgId] [bigint] NOT NULL,
                [AvailabilityId] [bigint] NOT NULL,
                [DurationId] [bigint] NOT NULL,
                [ReactionTimeId] [bigint] NOT NULL,
                [ReactionTypeId] [bigint] NOT NULL,
                [ServiceLocationId] [bigint] NOT NULL,
                [ProActiveSlaId] [bigint] NOT NULL,
                [Sla] nvarchar(255) NOT NULL,
                [SlaHash] [int] NOT NULL,
                [ReactionTime_Avalability] [bigint] NOT NULL,
                [ReactionTime_ReactionType] [bigint] NOT NULL,
                [ReactionTime_ReactionType_Avalability] [bigint] NOT NULL,
                [Fsp] nvarchar(255) NULL,
                [FspDescription] nvarchar(255) NULL
            )
AS
BEGIN
    
    if @limit > 0
    begin
        insert into @tbl
        select rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , Fsp
              , FspDescription
        from (
                select ROW_NUMBER() over(
                            order by m.CountryId
                                    , m.WgId
                                    , m.AvailabilityId
                                    , m.DurationId
                                    , m.ReactionTimeId
                                    , m.ReactionTypeId
                                    , m.ServiceLocationId
                                    , m.ProActiveSlaId
                        ) as rownum
                        , m.*
                from Portfolio.GetBySlaFsp(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m    
        ) t
        where rownum > @lastid and rownum <= @lastid + @limit;
    end
    else
    begin
        insert into @tbl 
        select -1 as rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , m.Fsp
              , m.FspDescription
        from Portfolio.GetBySlaFsp(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    end 

    RETURN;
END;
go

IF OBJECT_ID('[Portfolio].[GetBySlaSingle]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaSingle];
go 

CREATE FUNCTION [Portfolio].[GetBySlaSingle](
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select m.*
    from Portfolio.LocalPortfolio m
    where   (@cnt          is null or @cnt          = m.CountryId         )
        AND (@wg           is null or @wg           = m.WgId              )
        AND (@av           is null or @av           = m.AvailabilityId    )
        AND (@dur          is null or @dur          = m.DurationId        )
        AND (@reactiontime is null or @reactiontime = m.ReactionTimeId    )
        AND (@reactiontype is null or @reactiontype = m.ReactionTypeId    )
        AND (@loc          is null or @loc          = m.ServiceLocationId )
        AND (@pro          is null or @pro          = m.ProActiveSlaId    )
)
GO

IF OBJECT_ID('[Portfolio].[GetBySlaSinglePaging]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaSinglePaging];
go 

CREATE FUNCTION [Portfolio].[GetBySlaSinglePaging](
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
RETURNS @tbl TABLE 
            (   
                [rownum] [int] NOT NULL,
                [Id] [bigint] NOT NULL,
                [CountryId] [bigint] NOT NULL,
                [WgId] [bigint] NOT NULL,
                [AvailabilityId] [bigint] NOT NULL,
                [DurationId] [bigint] NOT NULL,
                [ReactionTimeId] [bigint] NOT NULL,
                [ReactionTypeId] [bigint] NOT NULL,
                [ServiceLocationId] [bigint] NOT NULL,
                [ProActiveSlaId] [bigint] NOT NULL,
                [Sla] nvarchar(255) NOT NULL,
                [SlaHash] [int] NOT NULL,
                [ReactionTime_Avalability] [bigint] NOT NULL,
                [ReactionTime_ReactionType] [bigint] NOT NULL,
                [ReactionTime_ReactionType_Avalability] [bigint] NOT NULL,
                [Fsp] nvarchar(255) NULL,
                [FspDescription] nvarchar(255) NULL
            )
AS
BEGIN
    
    if @limit > 0
    begin
        insert into @tbl
        select rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , null as Fsp
              , null as FspDescription
        from (
                select ROW_NUMBER() over(
                            order by m.CountryId
                                    , m.WgId
                                    , m.AvailabilityId
                                    , m.DurationId
                                    , m.ReactionTimeId
                                    , m.ReactionTypeId
                                    , m.ServiceLocationId
                                    , m.ProActiveSlaId
                        ) as rownum
                        , m.*
                from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m    
        ) t
        where rownum > @lastid and rownum <= @lastid + @limit;
    end
    else
    begin
        insert into @tbl 
        select -1 as rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , null as Fsp
              , null as FspDescription
        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    end 

    RETURN;
END;
go

IF OBJECT_ID('[Portfolio].[GetBySlaFspSingle]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaFspSingle];
go 

CREATE FUNCTION [Portfolio].[GetBySlaFspSingle](
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select    m.*
            , fsp.Name               as Fsp
            , fsp.ServiceDescription as FspDescription
    from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    left JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla 
)
GO

IF OBJECT_ID('[Portfolio].[GetBySlaFspSinglePaging]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaFspSinglePaging];
go 

CREATE FUNCTION [Portfolio].[GetBySlaFspSinglePaging](
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
RETURNS @tbl TABLE 
            (   
                [rownum] [int] NOT NULL,
                [Id] [bigint] NOT NULL,
                [CountryId] [bigint] NOT NULL,
                [WgId] [bigint] NOT NULL,
                [AvailabilityId] [bigint] NOT NULL,
                [DurationId] [bigint] NOT NULL,
                [ReactionTimeId] [bigint] NOT NULL,
                [ReactionTypeId] [bigint] NOT NULL,
                [ServiceLocationId] [bigint] NOT NULL,
                [ProActiveSlaId] [bigint] NOT NULL,
                [Sla] nvarchar(255) NOT NULL,
                [SlaHash] [int] NOT NULL,
                [ReactionTime_Avalability] [bigint] NOT NULL,
                [ReactionTime_ReactionType] [bigint] NOT NULL,
                [ReactionTime_ReactionType_Avalability] [bigint] NOT NULL,
                [Fsp] nvarchar(255) NULL,
                [FspDescription] nvarchar(255) NULL
            )
AS
BEGIN
    
    if @limit > 0
    begin
        insert into @tbl
        select rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , Fsp
              , FspDescription
        from (
                select ROW_NUMBER() over(
                            order by m.CountryId
                                    , m.WgId
                                    , m.AvailabilityId
                                    , m.DurationId
                                    , m.ReactionTimeId
                                    , m.ReactionTypeId
                                    , m.ServiceLocationId
                                    , m.ProActiveSlaId
                        ) as rownum
                        , m.*
                from Portfolio.GetBySlaFspSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m    
        ) t
        where rownum > @lastid and rownum <= @lastid + @limit;
    end
    else
    begin
        insert into @tbl 
        select -1 as rownum
              , Id
              , CountryId
              , WgId
              , AvailabilityId
              , DurationId
              , ReactionTimeId
              , ReactionTypeId
              , ServiceLocationId
              , ProActiveSlaId
              , Sla
              , SlaHash
              , ReactionTime_Avalability
              , ReactionTime_ReactionType
              , ReactionTime_ReactionType_Avalability
              , m.Fsp
              , m.FspDescription
        from Portfolio.GetBySlaFspSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    end 

    RETURN;
END;
go

IF OBJECT_ID('Portfolio.GetBySlaSog') IS NOT NULL
  DROP FUNCTION Portfolio.GetBySlaSog;
go

CREATE FUNCTION [Portfolio].[GetBySlaSog](
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint
)
RETURNS TABLE 
AS
RETURN 
(
    with cte as (
        select id
        from InputAtoms.Wg 
        where SogId in (
                select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
            )
            and IsSoftware = 0
    )
    select m.*
    from Portfolio.LocalPortfolio m
    join cte wg on wg.Id = m.WgId
    where   m.CountryId = @cnt

        AND (@av           is null or @av           = m.AvailabilityId    )
        AND (@dur          is null or @dur          = m.DurationId        )
        AND (@reactiontime is null or @reactiontime = m.ReactionTimeId    )
        AND (@reactiontype is null or @reactiontype = m.ReactionTypeId    )
        AND (@loc          is null or @loc          = m.ServiceLocationId )
        AND (@pro          is null or @pro          = m.ProActiveSlaId    )
)
go
