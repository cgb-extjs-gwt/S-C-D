ALTER PROCEDURE [Hardware].[SpReleaseCosts]
	@usr		  int, 
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;
    
	UPDATE mc
	SET [ServiceTP_Released] = COALESCE(costs.ServiceTPManual, costs.ServiceTP),
		[ChangeUserId] = @usr
	FROM [Hardware].[ManualCost] mc
	JOIN Hardware.GetCosts(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, 0) costs on costs.Id = mc.PortfolioId

   
END