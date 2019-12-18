IF OBJECT_ID('Hardware.SpReleaseCosts') IS NOT NULL
  DROP PROCEDURE Hardware.SpReleaseCosts;
go

CREATE PROCEDURE [Hardware].[SpReleaseCosts]
    @usr          int, 
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

    declare @now datetime = getdate();    

	SELECT * INTO #temp 
	FROM Hardware.GetReleaseCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, 0) costs
	WHERE (not exists(select 1 from @portfolioIds) or costs.Id in (select Id from @portfolioIds))   
	--TODO: @portfolioIds case to be fixed in a future release 

	UPDATE mc
	SET [ServiceTP1_Released]  = case when dur.Value >= 1 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP1) end,
		[ServiceTP2_Released]  = case when dur.Value >= 2 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP2) end,
		[ServiceTP3_Released]  = case when dur.Value >= 3 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP3) end,
		[ServiceTP4_Released]  = case when dur.Value >= 4 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP4) end,
		[ServiceTP5_Released]  = case when dur.Value >= 5 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP5) end,
		[ServiceTP1P_Released] = case when dur.IsProlongation = 1                    then  COALESCE(costs.ServiceTPManual, costs.ServiceTP1P)            end,

		[ServiceTP_Released]   = COALESCE(costs.ServiceTPManual, costs.ServiceTP),

		[ReleaseUserId] = @usr,
        [ReleaseDate] = @now

	FROM [Hardware].[ManualCost] mc
	JOIN #temp costs on mc.PortfolioId = costs.Id
	JOIN Dependencies.Duration dur on costs.DurationId = dur.Id
	where costs.ServiceTPManual is not null or costs.ServiceTP is not null

	INSERT INTO [Hardware].[ManualCost] (
                [PortfolioId], 
				[ReleaseUserId], 
                [ReleaseDate],
				[ServiceTP1_Released], [ServiceTP2_Released], [ServiceTP3_Released], [ServiceTP4_Released], [ServiceTP5_Released], [ServiceTP1P_Released], ServiceTP_Released)
	SELECT  costs.Id, 

            @usr, 
            @now,

			case when dur.Value >= 1 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP1) end,
			case when dur.Value >= 2 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP2) end,
			case when dur.Value >= 3 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP3) end,
			case when dur.Value >= 4 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP4) end,
			case when dur.Value >= 5 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP5) end,
			case when dur.IsProlongation = 1                    then  COALESCE(costs.ServiceTPManual, costs.ServiceTP1P)            end,
            COALESCE(costs.ServiceTPManual, costs.ServiceTP)

	FROM [Hardware].[ManualCost] mc
	RIGHT JOIN #temp costs on mc.PortfolioId = costs.Id
	JOIN Dependencies.Duration dur on costs.DurationId = dur.Id
	where mc.PortfolioId is null and (costs.ServiceTPManual is not null or costs.ServiceTP is not null)

	DROP table #temp
   
END
go