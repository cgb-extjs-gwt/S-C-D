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

--IF TYPE_ID('dbo.ListID') IS NOT NULL
--  DROP Type dbo.ListID;
--go

--CREATE TYPE dbo.ListID AS TABLE(
--    id bigint NULL
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

CREATE PROCEDURE Portfolio.DenyLocalPortfolioById
    @cnt bigint,
    @ids dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;

    DELETE FROM Portfolio.LocalPortfolio
    WHERE   (CountryId = @cnt) 
        AND (Id in (select Id from @ids));

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
    INSERT INTO Portfolio.LocalPortfolio (CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId)
    SELECT @cnt, sla.WgId, sla.AvailabilityId, sla.DurationId, sla.ReactionTypeId, sla.ReactionTimeId, sla.ServiceLocationId, sla.ProActiveSlaId
    FROM PrincipleSlaCte sla
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


