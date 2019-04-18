alter table Hardware.ManualCost drop column ServiceTP_Released;

alter table Hardware.ManualCost
add ServiceTP1_Released float null,
	ServiceTP2_Released float null,
	ServiceTP3_Released float null,
	ServiceTP4_Released float null,
	ServiceTP5_Released float null,
    ServiceTP_Released as ( case when ServiceTP1_Released is null 
								 then null 
								 else			 ServiceTP1_Released + 
										COALESCE(ServiceTP2_Released, 0) + 
										COALESCE(ServiceTP3_Released, 0) + 
										COALESCE(ServiceTP4_Released, 0) + 
										COALESCE(ServiceTP5_Released, 0) end)
GO

IF OBJECT_ID('Hardware.SpReleaseCosts') IS NOT NULL
  DROP PROCEDURE Hardware.SpReleaseCosts;
go

CREATE PROCEDURE [Hardware].[SpReleaseCosts]
	@usr		  int, 
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
	@portfolioIds dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;
    

	SELECT * INTO #temp 
	FROM Hardware.GetCosts(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, 0) costs
	WHERE (not exists(select 1 from @portfolioIds) or costs.Id in (select Id from @portfolioIds))   
	--TODO: @portfolioIds case to be fixed in a future release 

	UPDATE mc
	SET [ServiceTP1_Released] = case when dur.Value >= 1 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP1) else null end,
		[ServiceTP2_Released] = case when dur.Value >= 2 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP2) else null end,
		[ServiceTP3_Released] = case when dur.Value >= 3 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP3) else null end,
		[ServiceTP4_Released] = case when dur.Value >= 4 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP4) else null end,
		[ServiceTP5_Released] = case when dur.Value >= 5 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP5) else null end,
		[ChangeUserId] = @usr,
        [ReleaseDate] = getdate()
	FROM [Hardware].[ManualCost] mc
	JOIN #temp costs on mc.PortfolioId = costs.Id
	JOIN Dependencies.Duration dur on costs.DurationId = dur.Id
	where costs.ServiceTPManual is not null or costs.ServiceTP is not null

	INSERT INTO [Hardware].[ManualCost] 
				([PortfolioId], 
				[ChangeUserId], 
                [ReleaseDate],
				[ServiceTP1_Released], [ServiceTP2_Released], [ServiceTP3_Released], [ServiceTP4_Released], [ServiceTP5_Released])
	SELECT  costs.Id, 
			@usr, 
            getdate(),
			case when dur.Value >= 1 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP1) else null end,
			case when dur.Value >= 2 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP2) else null end,
			case when dur.Value >= 3 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP3) else null end,
			case when dur.Value >= 4 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP4) else null end,
			case when dur.Value >= 5 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP5) else null end
	FROM [Hardware].[ManualCost] mc
	RIGHT JOIN #temp costs on mc.PortfolioId = costs.Id
	JOIN Dependencies.Duration dur on costs.DurationId = dur.Id
	where mc.PortfolioId is null and (costs.ServiceTPManual is not null or costs.ServiceTP is not null)

	DROP table #temp
   
END
GO
