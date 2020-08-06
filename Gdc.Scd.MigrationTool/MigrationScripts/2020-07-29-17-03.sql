ALTER PROCEDURE [Hardware].[SpReleaseCosts]
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

    with cte as (
        SELECT *
               , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP_sum
        FROM Hardware.GetReleaseCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, 0) m
        WHERE (not exists(select 1 from @portfolioIds) or m.Id in (select Id from @portfolioIds))   
        --TODO: @portfolioIds case to be fixed in a future release 
    )
    select 

          costs.Id

        , case when costs.Year >= 1 and costs.IsProlongation = 0 then COALESCE(costs.ServiceTPManual * costs.ServiceTP1 / costs.ServiceTP_sum, costs.ServiceTP1) end * costs.ExchangeRate as tp_release1
        , case when costs.Year >= 2 and costs.IsProlongation = 0 then COALESCE(costs.ServiceTPManual * costs.ServiceTP2 / costs.ServiceTP_sum, costs.ServiceTP2) end * costs.ExchangeRate as tp_release2
        , case when costs.Year >= 3 and costs.IsProlongation = 0 then COALESCE(costs.ServiceTPManual * costs.ServiceTP3 / costs.ServiceTP_sum, costs.ServiceTP3) end * costs.ExchangeRate as tp_release3
        , case when costs.Year >= 4 and costs.IsProlongation = 0 then COALESCE(costs.ServiceTPManual * costs.ServiceTP4 / costs.ServiceTP_sum, costs.ServiceTP4) end * costs.ExchangeRate as tp_release4
        , case when costs.Year >= 5 and costs.IsProlongation = 0 then COALESCE(costs.ServiceTPManual * costs.ServiceTP5 / costs.ServiceTP_sum, costs.ServiceTP5) end * costs.ExchangeRate as tp_release5
        , case when costs.IsProlongation = 1                     then COALESCE(costs.ServiceTPManual, costs.ServiceTP1P)                                         end * costs.ExchangeRate as tp_releaseP

        , ISNULL(costs.ProActive, 0) * costs.ExchangeRate as ProActive_Released

        , COALESCE(costs.ReActiveTPManual, costs.ReActiveTP) * costs.ExchangeRate as ReActiveTP_Released

        , COALESCE(costs.ServiceTPManual, costs.ServiceTP) * costs.ExchangeRate as ServiceTP_Released

    into #temp 
    from cte costs;

    UPDATE mc
    SET [ServiceTP1_Released]  = costs.tp_release1,
        [ServiceTP2_Released]  = costs.tp_release2,
        [ServiceTP3_Released]  = costs.tp_release3,
        [ServiceTP4_Released]  = costs.tp_release4,
        [ServiceTP5_Released]  = costs.tp_release5,
        [ServiceTP1P_Released] = costs.tp_releaseP,

        [ProActive_Released]   = costs.ProActive_Released,

        [ReActiveTP_Released]  = costs.ReActiveTP_Released,
        [ServiceTP_Released]   = costs.ServiceTP_Released,

        [ReleaseUserId] = @usr,
        [ReleaseDate] = @now

    FROM [Hardware].[ManualCost] mc
    JOIN #temp costs on mc.PortfolioId = costs.Id;

    INSERT INTO [Hardware].[ManualCost] (
                [PortfolioId], 
                [ReleaseUserId], 
                [ReleaseDate],

                [ServiceTP1_Released], 
                [ServiceTP2_Released], 
                [ServiceTP3_Released], 
                [ServiceTP4_Released], 
                [ServiceTP5_Released], 
                [ServiceTP1P_Released], 
                ServiceTP_Released,
                ReActiveTP_Released,
                ProActive_Released
        )
    SELECT    costs.Id 

            , @usr
            , @now

            , tp_release1
            , tp_release2
            , tp_release3
            , tp_release4
            , tp_release5
            , tp_releaseP

            , ServiceTP_Released

            , ReActiveTP_Released

            , ProActive_Released

    FROM #temp costs
    where not exists(select * from Hardware.ManualCost where PortfolioId = costs.Id) 
            and ServiceTP_Released is not null;

    DROP table #temp;
   
END