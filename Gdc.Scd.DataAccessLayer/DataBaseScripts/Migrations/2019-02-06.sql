  DELETE FROM [Scd_2].[Report].[ReportFilter]
  WHERE ReportId = 5 AND [Name] = 'dur'
  GO

  DELETE FROM [Scd_2].[Report].[ReportColumn]
  WHERE ReportId = 5 AND [Name] = 'Duration'
  GO

  ALTER FUNCTION [Report].[HddRetentionByCountry]
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
    select m.Id
         , m.Country
         , c.CountryGroup
         , wg.Name as Wg
         , wg.Description as WgDescription
         , m.Fsp
         , m.Fsp as TopFsp
     
         , m.HddRet
         , m.ServiceTP
         , m.DealerPrice
         , m.ListPrice

         , wg.Sog

         , m.Availability
         , m.Duration
         , m.ReactionTime
         , m.ReactionType
         , m.ServiceLocation
         , m.ProActiveSla

    from Report.GetCosts(@cnt, @wg, @av, (SELECT Id FROM [Dependencies].[Duration] WHERE [Value] = 5 AND [IsProlongation] = 0), @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.CountryView c on c.id = m.CountryId
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)