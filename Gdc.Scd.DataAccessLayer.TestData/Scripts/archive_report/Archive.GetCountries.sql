if OBJECT_ID('Archive.GetCountries') is not null
    drop function Archive.GetCountries;
go

create function Archive.GetCountries()
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , ISO3CountryCode nvarchar(255)
    , Region nvarchar(255)
    , ClusterRegion nvarchar(255)
    , Currency nvarchar(255)
    , ExchangeRate float
)
begin

    insert into @tbl
    select  c.Id
        , c.Name as Country
        , c.ISO3CountryCode
        , r.Name as Region
        , cr.Name as ClusterRegion
        , cur.Name
        , er.Value

    from InputAtoms.Country c 
    left join InputAtoms.CountryGroup cg on cg.id = c.CountryGroupId
    left join InputAtoms.Region r on r.id = c.RegionId
    left join InputAtoms.ClusterRegion cr on cr.Id = r.ClusterRegionId
    left join [References].Currency cur on cur.Id = c.CurrencyId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    return;

end
go