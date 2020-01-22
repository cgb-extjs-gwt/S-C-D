if OBJECT_ID('SODA.GetSwProactive') is not null
    drop function SODA.GetSwProactive;
go

create function SODA.GetSwProactive()
returns @tbl table (
      Country           nvarchar(64)   
    , CountryGroup      nvarchar(64)
    , ISO3CountryCode   nvarchar(64)
    , Fsp               nvarchar(64)
    , Sog               nvarchar(64)   
    , SwDigit           nvarchar(64)   
    , Availability      nvarchar(64)
    , Year              nvarchar(64)
    , ProactiveSla      nvarchar(128)
    , ProActive         float
)
as
begin

    declare @cnt dbo.ListID ;
    declare @digit dbo.ListID ;
    declare @av dbo.ListID ;
    declare @year dbo.ListID ;

    declare @country table (
          Id bigint not null INDEX IX1 CLUSTERED
        , Name nvarchar(255)
        , ISO3CountryCode nvarchar(255)
        , CountryGroup nvarchar(255)
        , Currency nvarchar(255)
        , ExchangeRate float
        , CanOverrideTransferCostAndPrice bit
        , CanStoreListAndDealerPrices     bit
    );

    insert into @country
    select  c.Id
        , c.Name as Country
        , c.ISO3CountryCode
        , cg.Name
        , cur.Name
        , er.Value

        , c.CanOverrideTransferCostAndPrice
        , c.CanStoreListAndDealerPrices

    from InputAtoms.Country c 
    left join InputAtoms.CountryGroup cg on cg.id = c.CountryGroupId
    left join [References].Currency cur on cur.Id = c.CurrencyId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId;


    insert into @cnt(id) select id from InputAtoms.Country;

    insert into @tbl
    select    c.Name as Country               
            , c.CountryGroup
            , c.ISO3CountryCode

            , m.Fsp

            , sog.Name as Sog                   
            , d.Name as SwDigit               

            , av.Name as Availability
            , y.Name as Year
            , pro.ExternalName as ProactiveSla

            , m.ProActive

    FROM SoftwareSolution.GetProActiveCosts2(1, @cnt, null, 1, @digit, @av, @year, null, null) m
    JOIN @country c on c.id = m.Country
    join InputAtoms.SwDigit d on d.Id = m.SwDigit
    join InputAtoms.Sog sog on sog.Id = d.SogId
    left join Dependencies.Availability av on av.Id = m.AvailabilityId
    left join Dependencies.Year y on y.Id = m.DurationId
    left join Dependencies.ProActiveSla pro on pro.Id = m.ProactiveSlaId;

    return;

end
go

if OBJECT_ID('SODA.SwProactiveCost') is not null
    drop view SODA.SwProactiveCost;
go

create view SODA.SwProactiveCost as
    select * from SODA.GetSwProactive();
go