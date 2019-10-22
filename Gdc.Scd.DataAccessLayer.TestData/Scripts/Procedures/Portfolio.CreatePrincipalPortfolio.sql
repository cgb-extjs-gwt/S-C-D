IF OBJECT_ID('Portfolio.CreatePrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.CreatePrincipalPortfolio;
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