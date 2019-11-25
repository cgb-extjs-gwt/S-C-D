IF OBJECT_ID('Portfolio.AllowLocalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.AllowLocalPortfolio;
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