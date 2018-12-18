
IF OBJECT_ID('Portfolio.DenyLocalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.DenyLocalPortfolio;
go

IF OBJECT_ID('Portfolio.AllowLocalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.AllowLocalPortfolio;
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

--IF TYPE_ID('dbo.ListID') IS NOT NULL
--  DROP Type dbo.ListID;
--go

--CREATE TYPE dbo.ListID AS TABLE(
--	id bigint NULL
--)
--go

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
    INSERT INTO Portfolio.LocalPortfolio (CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId)

    --insert new portfolio only

    SELECT @cnt, WG, Availability, Duration, ReactionType, ReactionTime, ServiceLocation, ProActive
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
    ALTER TABLE Portfolio.LocalPortfolio CHECK CONSTRAINT ALL;

END

go
