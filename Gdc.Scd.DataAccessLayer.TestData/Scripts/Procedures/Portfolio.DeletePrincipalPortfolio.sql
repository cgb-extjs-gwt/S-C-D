IF OBJECT_ID('Portfolio.DeletePrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.DeletePrincipalPortfolio;
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