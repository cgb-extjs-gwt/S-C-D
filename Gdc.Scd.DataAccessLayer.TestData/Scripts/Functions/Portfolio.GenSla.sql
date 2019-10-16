IF OBJECT_ID('Portfolio.GenSla') IS NOT NULL
  DROP FUNCTION Portfolio.GenSla;
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