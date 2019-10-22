IF OBJECT_ID('Portfolio.AllowPrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.AllowPrincipalPortfolio;
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