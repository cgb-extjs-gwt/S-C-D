IF OBJECT_ID('Portfolio.GetBySlaSog') IS NOT NULL
  DROP FUNCTION Portfolio.GetBySlaSog;
go

CREATE FUNCTION Portfolio.GetBySlaSog(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint
)
RETURNS TABLE 
AS
RETURN 
(
    with cte as (
        select id
        from InputAtoms.Wg 
        where SogId in (
                select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
            )
            and IsSoftware = 0
    )
    select m.*
    from Portfolio.LocalPortfolio m
    join cte wg on wg.Id = m.WgId
    where   m.CountryId = @cnt

        AND (@av           is null or @av           = m.AvailabilityId    )
        AND (@dur          is null or @dur          = m.DurationId        )
        AND (@reactiontime is null or @reactiontime = m.ReactionTimeId    )
        AND (@reactiontype is null or @reactiontype = m.ReactionTypeId    )
        AND (@loc          is null or @loc          = m.ServiceLocationId )
        AND (@pro          is null or @pro          = m.ProActiveSlaId    )
)
go