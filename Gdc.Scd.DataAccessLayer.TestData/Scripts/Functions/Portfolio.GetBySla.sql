IF OBJECT_ID('[Portfolio].[GetBySla]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySla];
go 

CREATE FUNCTION [Portfolio].[GetBySla](
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN 
(
    select m.*
    from Portfolio.LocalPortfolio m
    where   exists(select id from @cnt where id = m.CountryId)

        AND (not exists(select 1 from @wg           ) or exists(select 1 from @wg           where id = m.WgId              ))

        AND exists(select * from InputAtoms.Wg where Deactivated = 0 and Id = m.WgId)

        AND (not exists(select 1 from @av           ) or exists(select 1 from @av           where id = m.AvailabilityId    ))
        AND (not exists(select 1 from @dur          ) or exists(select 1 from @dur          where id = m.DurationId        ))
        AND (not exists(select 1 from @reactiontime ) or exists(select 1 from @reactiontime where id = m.ReactionTimeId    ))
        AND (not exists(select 1 from @reactiontype ) or exists(select 1 from @reactiontype where id = m.ReactionTypeId    ))
        AND (not exists(select 1 from @loc          ) or exists(select 1 from @loc          where id = m.ServiceLocationId ))
        AND (not exists(select 1 from @pro          ) or exists(select 1 from @pro          where id = m.ProActiveSlaId    ))
)