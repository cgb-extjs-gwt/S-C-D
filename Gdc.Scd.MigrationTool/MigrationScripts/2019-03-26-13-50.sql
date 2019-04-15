ALTER VIEW [Hardware].[LogisticsCostView] AS
    SELECT lc.Country,
           lc.Wg, 
           rt.ReactionTypeId as ReactionType, 
           rt.ReactionTimeId as ReactionTime,
           
           lc.StandardHandling          / er.Value as StandardHandling,
           lc.StandardHandling_Approved / er.Value as StandardHandling_Approved,

           lc.HighAvailabilityHandling          / er.Value as HighAvailabilityHandling,
           lc.HighAvailabilityHandling_Approved / er.Value as HighAvailabilityHandling_Approved,

           lc.StandardDelivery          / er.Value as StandardDelivery,
           lc.StandardDelivery_Approved / er.Value as StandardDelivery_Approved,

           lc.ExpressDelivery          / er.Value as ExpressDelivery,
           lc.ExpressDelivery_Approved / er.Value as ExpressDelivery_Approved,

           lc.TaxiCourierDelivery          / er.Value as TaxiCourierDelivery,
           lc.TaxiCourierDelivery_Approved / er.Value as TaxiCourierDelivery_Approved,

           lc.ReturnDeliveryFactory          / er.Value as ReturnDeliveryFactory,
           lc.ReturnDeliveryFactory_Approved / er.Value as ReturnDeliveryFactory_Approved

    FROM Hardware.LogisticsCosts lc
    JOIN Dependencies.ReactionTime_ReactionType rt on rt.Id = lc.ReactionTimeType
    JOIN InputAtoms.Country c on c.Id = lc.Country
    LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where lc.DeactivatedDateTime is null
go

ALTER FUNCTION [Report].[LogisticCostInputCountry]
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select c.Region
         , c.Name as Country
         , wg.Name as Wg

         , c.Currency as Currency

         , (time.Name + ' ' + type.Name) as ReactionType

         , l.StandardHandling_Approved * er.Value as StandardHandling
         , l.HighAvailabilityHandling_Approved * er.Value as HighAvailabilityHandling
         , l.StandardDelivery_Approved * er.Value as StandardDelivery
         , l.ExpressDelivery_Approved * er.Value as ExpressDelivery
         , l.TaxiCourierDelivery_Approved * er.Value as TaxiCourierDelivery
         , l.ReturnDeliveryFactory_Approved * er.Value as ReturnDeliveryFactory

    FROM Hardware.LogisticsCosts l
    join InputAtoms.CountryView c on c.Id = l.Country
    join InputAtoms.WgSogView wg on wg.Id = l.Wg
    JOIN Dependencies.ReactionTime_ReactionType rtt on rtt.Id = l.ReactionTimeType
    JOIN Dependencies.ReactionTime time ON time.id = rtt.ReactionTimeId
    JOIN Dependencies.ReactionType type ON type.Id = rtt.ReactionTypeId
    join [References].Currency cur on cur.Id = c.CurrencyId
    join [References].ExchangeRate er on er.CurrencyId = cur.Id

    where l.DeactivatedDateTime is null
      and (@cnt is null or l.Country = @cnt)
      and (@wg is null or l.Wg = @wg)
      and (@reactiontime is null or rtt.ReactionTimeId = @reactiontime)
      and (@reactiontype is null or rtt.ReactionTypeId = @reactiontype)
)
