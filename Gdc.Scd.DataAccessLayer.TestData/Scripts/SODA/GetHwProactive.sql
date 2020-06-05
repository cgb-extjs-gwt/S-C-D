if OBJECT_ID('SODA.GetHwProactive') is not null
    drop function SODA.GetHwProactive;
go

CREATE FUNCTION SODA.GetHwProactive()
returns @tbl table (
          Country           nvarchar(255)
        , CountryGroup      nvarchar(255)
        , Fsp               nvarchar(255)
        , Wg                nvarchar(255)
        , ServiceLocation   nvarchar(255)
        , ReactionTime      nvarchar(255)
        , ReactionType      nvarchar(255)
        , Availability      nvarchar(255)
        , ProActiveSla      nvarchar(255)
        , Duration          nvarchar(255)
        , ReActive          float
        , ProActive         float
        , ServiceTP         float
        , Currency          nvarchar(16)
        , Sog               nvarchar(16)
        , SogDescription    nvarchar(255)
        , FspDescription    nvarchar(255)
    )
as
BEGIN

    declare @cnt table (
          Id bigint not null INDEX IX1 CLUSTERED
        , CountryGroup nvarchar(255)
    );
    insert into @cnt
    select  c.Id
        , cg.Name
    from InputAtoms.Country c 
    left join InputAtoms.CountryGroup cg on cg.id = c.CountryGroupId;

    declare @cntTable dbo.ListId; insert into @cntTable(id) select id from @cnt;

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where IsSoftware = 0
              and SogId is not null
              and Deactivated = 0;

    declare @avTable dbo.ListId; 
    declare @durTable dbo.ListId;
    declare @rtimeTable dbo.ListId;
    declare @rtypeTable dbo.ListId;
    declare @locTable dbo.ListId; 
    declare @proTable dbo.ListId; 

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSog(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable) m
    )
    , cte2 as (
        select  
                  m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as FspDescription

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla and fsp.IsStandardWarranty <> 1
    )
    insert into @tbl
    select    m.Country
            , c.CountryGroup
            , m.Fsp
            , m.Wg

            , m.ServiceLocation
            , m.ReactionTime
            , m.ReactionType
            , m.Availability
            , m.ProActiveSla

            , m.ServicePeriod as Duration

             , m.ServiceTpSog * m.ExchangeRate as ReActive
             , m.ProActiveSog * m.ExchangeRate as ProActive
             , (m.ServiceTpSog + coalesce(m.ProActiveSog, 0)) * m.ExchangeRate as ServiceTP

            , m.Currency

            , sog.Name as Sog
            , sog.Description as SogDescription

            , m.FspDescription
    from cte2 m
    JOIN @cnt c on c.Id = m.CountryId
    JOIN InputAtoms.Sog sog on Sog.Id = m.SogId;

    return;

END
go

if OBJECT_ID('SODA.HwProactiveCost') is not null
    drop view SODA.HwProactiveCost;
go

create view SODA.HwProactiveCost as
    select * from SODA.GetHwProactive();
go

