USE Scd_2
GO

EXECUTE sp_refreshsqlmodule N'[Report].[GetCostsFull]';

GO

ALTER FUNCTION [Report].[GetCosts](
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
RETURN 
(
    select Id

         , Fsp
         , FspDescription

         , CountryId
         , Country
         , WgId
         , Wg
         , DurationId
         , Duration
         , Year
         , IsProlongation
         , AvailabilityId
         , Availability
         , ReactionTimeId
         , ReactionTime
         , ReactionTypeId
         , ReactionType
         , ServiceLocationId
         , ServiceLocation
         , ProActiveSlaId
         , ProActiveSla

         , AvailabilityFee
         , HddRet
         , TaxAndDutiesW
         , TaxAndDutiesOow
         , Reinsurance
         , ProActive
         , ServiceSupportCost

         , MaterialW
         , MaterialOow
         , FieldServiceCost
         , Logistic
         , OtherDirect
         , LocalServiceStandardWarranty
         , Credits

         , ListPrice
         , DealerDiscount
         , DealerPrice

		 , coalesce(ServiceTCManual, ServiceTC) ServiceTC
         , coalesce(ServiceTPManual, ServiceTP) ServiceTP
         , ServiceTC_Released
         , ServiceTP_Released

    FROM Report.GetCostsFull(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
)
GO

ALTER FUNCTION [Report].[Locap]
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
    select m.Id
         , m.Fsp
         , wg.Description as WgDescription
         , m.FspDescription as ServiceLevel
         , m.ReactionTime
         , m.Year as ServicePeriod
         , wg.Name as Wg
         , (m.ProActive + m.ServiceTP_Released) as Dcos
         , m.ServiceTP_Released
         , m.Country
         , null as ServiceType
         , null as PlausiCheck
         , null as PortfolioType
         , null as ReleaseCreated
         , wg.Sog
    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)
GO

UPDATE [Report].[ReportColumn]
   SET [Name] = 'ServiceTP_Released', [Text]='Service TP (Released)'
 WHERE ReportId = 1 and "Index" = 8
GO

ALTER FUNCTION [Report].[LocapDetailed]
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
select     m.Id
         , m.Fsp
         , wg.Description as WgDescription
         , wg.Name as Wg
         , wg.SogDescription as SogDescription
         , m.ServiceLocation as ServiceLevel
         , m.ReactionTime
         , m.Year as ServicePeriod
         , wg.Sog as Sog
         , m.ProActiveSla
         , m.ProActive + m.ServiceTP_Released as Dcos
         , m.ServiceTP_Released
         , m.ListPrice
         , m.DealerPrice
         , m.Country
         , m.FieldServiceCost as FieldServiceCost
         , m.ServiceSupportCost as ServiceSupportCost 
         , m.MaterialOow as MaterialOow
         , m.MaterialW as MaterialW
         , m.TaxAndDutiesW as TaxAndDutiesW
         , m.Logistic as LogisticW
         , m.Logistic as LogisticOow
         , m.Reinsurance as Reinsurance
         , m.Reinsurance as ReinsuranceOow
         , m.OtherDirect as OtherDirect
         , m.Credits as Credits
         , null as IndirectCostOpex
         , null as ServiceType
         , null as PlausiCheck
         , null as PortfolioType
    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)
GO
UPDATE [Report].[ReportColumn]
   SET [Name] = 'ServiceTP_Released', [Text]='Service TP (Released)'
 WHERE ReportId = 2 and "Index" = 11
GO

ALTER FUNCTION [Report].[Contract]
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
		 , case when m.DurationId >= 1 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP1
         , case when m.DurationId >= 2 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP2
         , case when m.DurationId >= 3 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP3
         , case when m.DurationId >= 4 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP4
         , case when m.DurationId >= 5 then  m.ServiceTP_Released / dur.Value else null end as ServiceTP5

		 , case when m.DurationId >= 1 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly1
		 , case when m.DurationId >= 2 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly2
		 , case when m.DurationId >= 3 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly3
		 , case when m.DurationId >= 4 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly4
		 , case when m.DurationId >= 5 then  m.ServiceTP_Released / dur.Value / 12 else null end as ServiceTPMonthly5

         , m.StdWarranty as WarrantyLevel
         , null as PortfolioType
         , wg.Sog as Sog	

    from Hardware.GetCostsFull(1, @cnt, @wg, @av, (select top(1) id from Dependencies.Duration where IsProlongation = 0 and Value = 5), @reactiontime, @reactiontype, @loc, @pro, 0, -1) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
	join Dependencies.Duration dur on dur.id = m.DurationId
)

