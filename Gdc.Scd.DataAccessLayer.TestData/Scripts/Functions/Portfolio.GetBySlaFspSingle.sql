IF OBJECT_ID('Portfolio.GetBySlaFspSingle') IS NOT NULL
  DROP FUNCTION Portfolio.GetBySlaFspSingle;
go 

CREATE FUNCTION Portfolio.GetBySlaFspSingle(
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
    select    m.*
            , fsp.Name               as Fsp
            , fsp.ServiceDescription as FspDescription
    from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    left JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla 
)
GO