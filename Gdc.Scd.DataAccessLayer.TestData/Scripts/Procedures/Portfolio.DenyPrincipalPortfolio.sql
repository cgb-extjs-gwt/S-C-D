IF OBJECT_ID('Portfolio.DenyPrincipalPortfolio') IS NOT NULL
  DROP PROCEDURE Portfolio.DenyPrincipalPortfolio;
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