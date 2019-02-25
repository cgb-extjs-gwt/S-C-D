IF OBJECT_ID('Report.spLocap') IS NOT NULL
  DROP PROCEDURE Report.spLocap;
go 

CREATE PROCEDURE [Report].[spLocap]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       bigint,
    @limit        int,
    @total        int output
)
AS
BEGIN

    select @total = count(id) from Portfolio.GetBySlaFspSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    declare @sla Portfolio.Sla;
    insert into @sla select * from Portfolio.GetBySlaFspSinglePaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    select m.Id
         , m.Fsp
         , wg.Description as WgDescription
         , m.FspDescription as ServiceLevel

         , m.ReactionTime
         , m.Year as ServicePeriod
         , wg.Name as Wg

         , m.LocalServiceStandardWarranty * m.ExchangeRate as LocalServiceStandardWarranty
         , m.ServiceTC * m.ExchangeRate as ServiceTC
         , m.ServiceTP_Released  * m.ExchangeRate as ServiceTP_Released
         , cur.Name as Currency
         
         , m.Country
         , m.Availability                       + ', ' +
               m.ReactionType                   + ', ' +
               m.ReactionTime                   + ', ' +
               cast(m.Year as nvarchar(1))      + ', ' +
               m.ServiceLocation                + ', ' +
               m.ProActiveSla as ServiceType

         , null as PlausiCheck
         , null as PortfolioType
         , null as ReleaseCreated
         , wg.Sog
    from Hardware.GetCostsSla(1, @sla) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
    join [References].Currency cur on cur.Id = m.CurrencyId

END
go

IF OBJECT_ID('Report.spLocapDetailed') IS NOT NULL
  DROP PROCEDURE Report.spLocapDetailed;
go 

CREATE PROCEDURE [Report].[spLocapDetailed]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       bigint,
    @limit        int,
    @total        int output
)
AS
BEGIN

    select @total = count(id) from Portfolio.GetBySlaFspSingle(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    declare @sla Portfolio.Sla;
    insert into @sla select * from Portfolio.GetBySlaFspSinglePaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    select m.Id
         , m.Fsp
         , wg.Description as WgDescription
         , wg.Name as Wg
         , wg.SogDescription as SogDescription
         , m.ServiceLocation as ServiceLevel
         , m.ReactionTime
         , m.Year as ServicePeriod
         , wg.Sog as Sog
         , m.ProActiveSla
         , m.Country

         , m.ServiceTC * m.ExchangeRate as ServiceTC
         , m.ServiceTP_Released * m.ExchangeRate as ServiceTP_Released
         , m.ListPrice * m.ExchangeRate as ListPrice
         , m.DealerPrice * m.ExchangeRate as DealerPrice
         , m.FieldServiceCost * m.ExchangeRate as FieldServiceCost
         , m.ServiceSupportCost * m.ExchangeRate as ServiceSupportCost 
         , m.MaterialOow * m.ExchangeRate as MaterialOow
         , m.MaterialW * m.ExchangeRate as MaterialW
         , m.TaxAndDutiesW * m.ExchangeRate as TaxAndDutiesW
         , m.Logistic * m.ExchangeRate as LogisticW
         , m.Logistic * m.ExchangeRate as LogisticOow
         , m.Reinsurance * m.ExchangeRate as Reinsurance
         , m.Reinsurance * m.ExchangeRate as ReinsuranceOow
         , m.OtherDirect * m.ExchangeRate as OtherDirect
         , m.Credits * m.ExchangeRate as Credits
         , m.LocalServiceStandardWarranty * m.ExchangeRate as LocalServiceStandardWarranty
         , cur.Name as Currency

         , null as IndirectCostOpex
         , m.Availability                       + ', ' +
               m.ReactionType                   + ', ' +
               m.ReactionTime                   + ', ' +
               cast(m.Year as nvarchar(1))      + ', ' +
               m.ServiceLocation                + ', ' +
               m.ProActiveSla as ServiceType
         
         , null as PlausiCheck
         , null as PortfolioType
    from Hardware.GetCostsSla(1, @sla) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
    join [References].Currency cur on cur.Id = m.CurrencyId

END

go

IF OBJECT_ID('Report.spLocapGlobalSupport') IS NOT NULL
  DROP PROCEDURE Report.spLocapGlobalSupport;
go 

CREATE PROCEDURE [Report].[spLocapGlobalSupport]
(
    @cnt     dbo.ListID readonly,
    @wg      dbo.ListID readonly,
    @av      dbo.ListID readonly,
    @dur     dbo.ListID readonly,
    @rtime   dbo.ListID readonly,
    @rtype   dbo.ListID readonly,
    @loc     dbo.ListID readonly,
    @pro     dbo.ListID readonly,
    @lastid  bigint,
    @limit   int,
    @total   int output
)
AS
BEGIN

    select @total = count(id) from Portfolio.GetBySlaFsp(@cnt, @wg, @av, @dur, @rtime, @rtype, @loc, @pro);

    declare @sla Portfolio.Sla;
    insert into @sla select * from Portfolio.GetBySlaFspPaging(@cnt, @wg, @av, @dur, @rtime, @rtype, @loc, @pro, @lastid, @limit) m

    select    c.Country
            , cnt.ISO3CountryCode
            , c.Fsp
            , c.FspDescription

            , sog.Description as SogDescription
            , sog.Name        as Sog

            , c.ServiceLocation
            , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
            , c.Year as ServicePeriod
            , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

            , c.LocalServiceStandardWarranty
            , coalesce(ServiceTPManual, ServiceTP) ServiceTP
            , c.DealerPrice
            , c.ListPrice

    from Hardware.GetCostsSla(1, @sla) c
    inner join InputAtoms.Country cnt on cnt.id = c.CountryId
    inner join InputAtoms.WgSogView sog on sog.Id = c.WgId

END
go
