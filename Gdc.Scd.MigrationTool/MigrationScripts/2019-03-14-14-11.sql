declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-COUNTRY');
UPDATE  Report.ReportColumn SET Text = 'Region' WHERE Name = 'Region' and ReportId=@reportId
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-CALC-COUNTRY');
UPDATE  Report.ReportColumn SET Text = 'Region' WHERE Name = 'Region' and ReportId=@reportId
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-INPUT-COUNTRY');
UPDATE  Report.ReportColumn SET Text = 'Region' WHERE Name = 'Region' and ReportId=@reportId
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-CENTRAL');
delete from Report.ReportColumn where ReportId = @reportId;
delete from Report.ReportFilter where ReportId = @reportId;
delete from Report.Report where Id = @reportId;

IF OBJECT_ID('Report.LogisticCostCentral') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCentral;
go 

IF OBJECT_ID('Report.LogisticCostCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCountry;
go 

CREATE FUNCTION Report.LogisticCostCountry
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

         , 'EUR' as Currency

         , (time.Name + ' ' + type.Name) as ReactionType

         , l.StandardHandling_Approved as StandardHandling
         , l.HighAvailabilityHandling_Approved as HighAvailabilityHandling
         , l.StandardDelivery_Approved as StandardDelivery
         , l.ExpressDelivery_Approved as ExpressDelivery
         , l.TaxiCourierDelivery_Approved as TaxiCourierDelivery
         , l.ReturnDeliveryFactory_Approved as ReturnDeliveryFactory

    from Hardware.LogisticsCostView l
    join InputAtoms.CountryView c on c.Id = l.Country
    join InputAtoms.WgSogView wg on wg.Id = l.Wg
    JOIN Dependencies.ReactionTime time ON time.id = l.ReactionTime
    JOIN Dependencies.ReactionType type ON type.Id = l.ReactionType

    where (@cnt is null or l.Country = @cnt)
      and (@wg is null or l.Wg = @wg)
      and (@reactiontime is null or l.ReactionTime = @reactiontime)
      and (@reactiontype is null or l.ReactionType = @reactiontype)

)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-CALC-CENTRAL');
delete from Report.ReportColumn where ReportId = @reportId;
delete from Report.ReportFilter where ReportId = @reportId;
delete from Report.Report where Id = @reportId;

IF OBJECT_ID('Report.LogisticCostCalcCentral') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCalcCentral;
go 

IF OBJECT_ID('Report.LogisticCostCalcCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCalcCountry;
go 

CREATE FUNCTION Report.LogisticCostCalcCountry
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
    select rep.Id
         , c.Region
         , rep.Country
         , rep.Wg

         , rep.ServiceLocation as ServiceLevel
         , rep.ReactionTime
         , rep.ReactionType
         , rep.Duration
         , rep.Availability
         , rep.ProActiveSla

         , rep.ServiceTC * er.Value as ServiceTC
         , lc.StandardHandling_Approved * er.Value as Handling
         , rep.TaxAndDutiesW * er.Value as TaxAndDutiesW
         , rep.TaxAndDutiesOow * er.Value as TaxAndDutiesOow

         , rep.Logistic * er.Value as LogisticW
         , null as LogisticOow

         , rep.AvailabilityFee * er.Value as Fee
		 , cur.Name as Currency
    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) rep
    join InputAtoms.CountryView c on c.Id = rep.CountryId
    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = rep.CountryId AND lc.Wg = rep.WgId AND lc.ReactionTime = rep.ReactionTimeId AND lc.ReactionType = rep.ReactionTypeId
	join [References].Currency cur on cur.Id = c.CurrencyId
	join [References].ExchangeRate er on er.CurrencyId = cur.Id
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-INPUT-CENTRAL');
delete from Report.ReportColumn where ReportId = @reportId;
delete from Report.ReportFilter where ReportId = @reportId;
delete from Report.Report where Id = @reportId;

IF OBJECT_ID('Report.LogisticCostInputCentral') IS NOT NULL
  DROP FUNCTION Report.LogisticCostInputCentral;
go 

IF OBJECT_ID('Report.LogisticCostInputCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostInputCountry;
go 

CREATE FUNCTION Report.LogisticCostInputCountry
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

    where (@cnt is null or l.Country = @cnt)
      and (@wg is null or l.Wg = @wg)
      and (@reactiontime is null or rtt.ReactionTimeId = @reactiontime)
      and (@reactiontype is null or rtt.ReactionTypeId = @reactiontype)
)

GO

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