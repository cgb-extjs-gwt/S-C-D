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
    
	SELECT * INTO #temp 
	FROM Hardware.GetCosts(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, 0)   

	UPDATE mc
	SET [ServiceTP_Released] = COALESCE(costs.ServiceTPManual, costs.ServiceTP),
		[ChangeUserId] = @usr
	FROM [Hardware].[ManualCost] mc
	JOIN #temp costs on mc.PortfolioId = costs.Id
	where costs.ServiceTPManual is not null or costs.ServiceTP is not null

	INSERT INTO [Hardware].[ManualCost] 
				([PortfolioId], 
				[ChangeUserId], 
				[ServiceTP_Released])
	SELECT  costs.Id, 
			@usr, 
			COALESCE(costs.ServiceTPManual, costs.ServiceTP)
	FROM [Hardware].[ManualCost] mc
	RIGHT JOIN #temp costs on mc.PortfolioId = costs.Id
	where mc.PortfolioId is null and (costs.ServiceTPManual is not null or costs.ServiceTP is not null)

	DROP table #temp
   
END