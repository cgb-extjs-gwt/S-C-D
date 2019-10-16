IF OBJECT_ID('Portfolio.DenyLocalPortfolioById') IS NOT NULL
  DROP PROCEDURE Portfolio.DenyLocalPortfolioById;
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