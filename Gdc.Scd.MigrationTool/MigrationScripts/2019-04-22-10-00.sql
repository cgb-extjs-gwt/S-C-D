ALTER FUNCTION Report.Contract
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    select 
           m.Id
         , m.Country
         , wg.Name as Wg
         , wg.Description as WgDescription
         , null as SLA
         , m.ServiceLocation
         , m.ReactionTime
         , m.ReactionType
         , m.Availability
         , m.ProActiveSla

        , case when m.DurationId >= 1 then mc.ServiceTP1_Released * m.ExchangeRate end as ServiceTP1
        , case when m.DurationId >= 2 then mc.ServiceTP2_Released * m.ExchangeRate end as ServiceTP2
        , case when m.DurationId >= 3 then mc.ServiceTP3_Released * m.ExchangeRate end as ServiceTP3
        , case when m.DurationId >= 4 then mc.ServiceTP4_Released * m.ExchangeRate end as ServiceTP4
        , case when m.DurationId >= 5 then mc.ServiceTP5_Released * m.ExchangeRate end as ServiceTP5

        , case when m.DurationId >= 1 then mc.ServiceTP1_Released * m.ExchangeRate / 12 end as ServiceTPMonthly1
        , case when m.DurationId >= 2 then mc.ServiceTP2_Released * m.ExchangeRate / 12 end as ServiceTPMonthly2
        , case when m.DurationId >= 3 then mc.ServiceTP3_Released * m.ExchangeRate / 12 end as ServiceTPMonthly3
        , case when m.DurationId >= 4 then mc.ServiceTP4_Released * m.ExchangeRate / 12 end as ServiceTPMonthly4
        , case when m.DurationId >= 5 then mc.ServiceTP5_Released * m.ExchangeRate / 12 end as ServiceTPMonthly5
        , cur.Name as Currency

         , m.StdWarranty as WarrantyLevel
         , null as PortfolioType
         , wg.Sog as Sog

    from Report.GetCosts(@cnt, @wg, @av, (select top(1) id from Dependencies.Duration where IsProlongation = 0 and Value = 5), @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
    join Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0
    join [References].Currency cur on cur.Id = m.CurrencyId

    left join Hardware.ManualCost mc on mc.PortfolioId = m.Id
)
GO