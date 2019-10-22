IF OBJECT_ID('Portfolio.UpdatePrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.UpdatePrincipalPortfolio;
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