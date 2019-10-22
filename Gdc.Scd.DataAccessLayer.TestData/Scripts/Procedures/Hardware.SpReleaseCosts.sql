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

    SELECT   
              case when costs.Year >= 1 and costs.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / costs.Year, costs.ServiceTP1) end * costs.ExchangeRate as TP1_Released
            , case when costs.Year >= 2 and costs.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / costs.Year, costs.ServiceTP2) end * costs.ExchangeRate as TP2_Released
            , case when costs.Year >= 3 and costs.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / costs.Year, costs.ServiceTP3) end * costs.ExchangeRate as TP3_Released
            , case when costs.Year >= 4 and costs.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / costs.Year, costs.ServiceTP4) end * costs.ExchangeRate as TP4_Released
            , case when costs.Year >= 5 and costs.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / costs.Year, costs.ServiceTP5) end * costs.ExchangeRate as TP5_Released
            , case when costs.IsProlongation = 1                     then  COALESCE(costs.ServiceTPManual, costs.ServiceTP1P)             end * costs.ExchangeRate as TP1P_Released

            , COALESCE(costs.ServiceTPManual, costs.ServiceTP) * costs.ExchangeRate as TP_Released

            , costs.*
    INTO #temp 
	FROM Hardware.GetReleaseCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, 100) costs
	WHERE (not exists(select 1 from @portfolioIds) or costs.Id in (select Id from @portfolioIds))   
	--TODO: @portfolioIds case to be fixed in a future release 

    UPDATE mc
    SET   [ServiceTP1_Released]  = TP1_Released
        , [ServiceTP2_Released]  = TP2_Released
        , [ServiceTP3_Released]  = TP3_Released
        , [ServiceTP4_Released]  = TP4_Released
        , [ServiceTP5_Released]  = TP5_Released
        , [ServiceTP1P_Released] = TP1P_Released
        , [ServiceTP_Released]   = TP_Released
    
        , [ChangeUserId] = @usr
        , [ReleaseDate] = @now

    FROM [Hardware].[ManualCost] mc
    JOIN #temp costs on mc.PortfolioId = costs.Id
    where costs.ServiceTPManual is not null or costs.ServiceTP is not null

	INSERT INTO [Hardware].[ManualCost] 
				([PortfolioId], 
				[ChangeUserId], 
                [ReleaseDate],
				[ServiceTP1_Released], [ServiceTP2_Released], [ServiceTP3_Released], [ServiceTP4_Released], [ServiceTP5_Released], [ServiceTP1P_Released], ServiceTP_Released)
	SELECT    costs.Id
            , @usr
            , @now
            , TP1_Released
            , TP2_Released
            , TP3_Released
            , TP4_Released
            , TP5_Released
            , TP1P_Released
            , TP_Released
    FROM [Hardware].[ManualCost] mc
    RIGHT JOIN #temp costs on mc.PortfolioId = costs.Id
    where mc.PortfolioId is null and (costs.ServiceTPManual is not null or costs.ServiceTP is not null)

    DROP table #temp
   
END
GO