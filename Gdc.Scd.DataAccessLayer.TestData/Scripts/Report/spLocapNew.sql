USE [SCD_2]

IF OBJECT_ID('[Report].[spLocapNew]') IS NOT NULL
  DROP PROCEDURE [Report].[spLocapNew];
go 

CREATE PROCEDURE [Report].[spLocapNew]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@isProjectCalculator bit = 0
)
AS
BEGIN

    declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where SogId in (
            select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
        )
        and IsSoftware = 0
        and SogId is not null
        and DeactivatedDateTime is null;

    if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

    declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogNew(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @isProjectCalculator) m
        where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    m.Id
            , m.Fsp
            , m.WgDescription
            , m.ServiceLevel

            , m.Duration
            , m.ServiceLocation
            , m.Availability
            , m.ReactionTime
            , m.ReactionType
            , m.ProActiveSla

            , m.ServicePeriod

            , m.Wg
            , pla.Name as PLA

            , m.StdWarranty
            , m.StdWarrantyLocation

            , m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
            , m.ServiceTcSog * m.ExchangeRate as ServiceTC
            , m.ServiceTpSog_Released  * m.ExchangeRate as ServiceTP_Released
            , m.ReleaseDate

            , m.Currency
         
            , m.Country
            , m.Availability                       + ', ' +
                  m.ReactionType                   + ', ' +
                  m.ReactionTime                   + ', ' +
                  m.ServicePeriod                  + ', ' +
                  m.ServiceLocation                + ', ' +
                  m.ProActiveSla as ServiceType

            , null as PlausiCheck
            , wg.ServiceTypes as PortfolioType
            , m.Sog

    from cte2 m
    INNER JOIN InputAtoms.Wg wg on wg.id = m.WgId
    INNER JOIN InputAtoms.Pla pla on pla.Id = wg.PlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
GO


