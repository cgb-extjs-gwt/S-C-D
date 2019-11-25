IF OBJECT_ID('[Portfolio].[GetBySlaSingle]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaSingle];
go 

CREATE FUNCTION [Portfolio].[GetBySlaSingle](
    @cnt          bigint,
    @wg           bigint,
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
    select m.*
    from Portfolio.LocalPortfolio m
    where   (@cnt          is null or @cnt          = m.CountryId         )
        AND (@wg           is null or @wg           = m.WgId              )
        AND (@av           is null or @av           = m.AvailabilityId    )
        AND (@dur          is null or @dur          = m.DurationId        )
        AND (@reactiontime is null or @reactiontime = m.ReactionTimeId    )
        AND (@reactiontype is null or @reactiontype = m.ReactionTypeId    )
        AND (@loc          is null or @loc          = m.ServiceLocationId )
        AND (@pro          is null or @pro          = m.ProActiveSlaId    )
)
GO