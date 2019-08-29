ALTER FUNCTION Report.ProActive
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    select    m.Id
            , m.Country
            , c.CountryGroup
            , m.Fsp
            , m.Wg

            , m.ServiceLocation
            , m.ReactionTime
            , m.ReactionType
            , m.Availability
            , m.ProActiveSla

            , case 
                    when dur.IsProlongation = 1 then 'Prolongation'
                    else CAST(dur.Value as varchar(1))
              end as Duration

             , m.ServiceTPResult * m.ExchangeRate as ReActive
             , m.ProActive * m.ExchangeRate as ProActive
             , (m.ServiceTPResult + coalesce(m.ProActive, 0)) * m.ExchangeRate as ServiceTP

            , m.Currency

            , wg.Sog
            , wg.SogDescription

            , m.FspDescription

    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    JOIN InputAtoms.CountryView c on c.Id = m.CountryId
    join Dependencies.Duration dur on dur.Id = m.DurationId
    JOIN InputAtoms.WgSogView wg on wg.Id = m.WgId
)
GO