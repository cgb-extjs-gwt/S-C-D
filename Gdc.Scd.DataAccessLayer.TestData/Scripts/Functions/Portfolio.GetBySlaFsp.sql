IF OBJECT_ID('[Portfolio].[GetBySlaFsp]') IS NOT NULL
  DROP FUNCTION [Portfolio].[GetBySlaFsp];
go 

CREATE FUNCTION [Portfolio].[GetBySlaFsp](
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
    select    m.*
            , fsp.Name               as Fsp
            , fsp.ServiceDescription as FspDescription
    from Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    left JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla 
)
GO