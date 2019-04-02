if not exists(SELECT * FROM sys.schemas WHERE name = N'Archive')
    exec('CREATE SCHEMA Archive');
go

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

if OBJECT_ID('Archive.GetWg') is not null
    drop function Archive.GetWg;
go

create function Archive.GetWg(@software bit null)
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , Description nvarchar(255)
    , Pla nvarchar(255)
    , ClusterPla nvarchar(255)
    , Sog nvarchar(255)
)
begin

    insert into @tbl
    select  wg.Id
          , wg.Name as Wg
          , wg.Description
          , pla.Name as Pla
          , cpla.Name as ClusterPla
          , sog.Name as Sog
    from InputAtoms.Wg wg 
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId
    left join InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
          and (@software is null or wg.IsSoftware = @software)

    return;

end
go

if OBJECT_ID('Archive.GetReactionTimeType') is not null
    drop function Archive.GetReactionTimeType;
go

create function Archive.GetReactionTimeType()
returns @tbl table (
      Id bigint not null primary key
    , ReactionTime nvarchar(255)
    , ReactionTimeEx nvarchar(255)
    , ReactionType nvarchar(255)
    , ReactionTypeEx nvarchar(255)
)
begin

    insert into @tbl
    select  rtt.Id

          , rtime.Name 
          , rtime.ExternalName
          , rtype.Name 
          , rtype.ExternalName

    from Dependencies.ReactionTime_ReactionType rtt 
    join Dependencies.ReactionTime rtime on rtime.Id = rtt.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = rtt.ReactionTypeId

    where rtt.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetReactionTimeAvailability') is not null
    drop function Archive.GetReactionTimeAvailability;
go

create function Archive.GetReactionTimeAvailability()
returns @tbl table (
      Id bigint not null primary key
    , Availability nvarchar(255)
    , AvailabilityEx nvarchar(255)
    , ReactionTime nvarchar(255)
    , ReactionTimeEx nvarchar(255)
)
begin

    insert into @tbl
    select  rta.Id
            
          , av.Name
          , av.ExternalName
        
          , rtime.Name 
          , rtime.ExternalName

    from Dependencies.ReactionTime_Avalability rta 
    join Dependencies.ReactionTime rtime on rtime.Id = rta.ReactionTimeId
    join Dependencies.Availability av on av.Id = rta.AvailabilityId

    where rta.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetReactionTimeTypeAvailability') is not null
    drop function Archive.GetReactionTimeTypeAvailability;
go

create function Archive.GetReactionTimeTypeAvailability()
returns @tbl table (
      Id bigint not null primary key
    , Availability nvarchar(255)
    , AvailabilityEx nvarchar(255)
    , ReactionTime nvarchar(255)
    , ReactionTimeEx nvarchar(255)
    , ReactionType nvarchar(255)
    , ReactionTypeEx nvarchar(255)
)
begin

    insert into @tbl
    select  tta.Id
            
          , av.Name
          , av.ExternalName
        
          , rtime.Name 
          , rtime.ExternalName

          , rtype.Name 
          , rtype.ExternalName

    from Dependencies.ReactionTime_ReactionType_Avalability tta 
    join Dependencies.ReactionTime rtime on rtime.Id = tta.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = tta.ReactionTypeId
    join Dependencies.Availability av on av.Id = tta.AvailabilityId

    where tta.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetDurationAvailability') is not null
    drop function Archive.GetDurationAvailability;
go

create function Archive.GetDurationAvailability()
returns @tbl table (
      Id bigint not null primary key
    , Duration nvarchar(255)
    , DurationEx nvarchar(255)
    , Availability nvarchar(255)
    , AvailabilityEx nvarchar(255)
)
begin

    insert into @tbl
    select  da.Id
          , dur.Name 
          , dur.ExternalName
          , av.Name 
          , av.ExternalName 
    from Dependencies.Duration_Availability da
    join Dependencies.Duration dur on dur.Id = da.YearId
    join Dependencies.Availability av on av.Id = da.AvailabilityId

    where da.IsDisabled = 0

    return;
end
go

if OBJECT_ID('Archive.GetSwDigit') is not null
    drop function Archive.GetSwDigit;
go

create function Archive.GetSwDigit()
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , Description nvarchar(255)
    , Sog nvarchar(255)
    , Sfab nvarchar(255)
    , Pla nvarchar(255)
    , ClusterPla nvarchar(255)
)
begin

    insert into @tbl
    select  dig.Id
          , dig.Name
          , dig.Description

          , sog.Name as Sog
          , sfab.Name as Sfab
          , pla.Name as Pla
          , cpla.Name as ClusterPla
    from InputAtoms.SwDigit dig
    left join InputAtoms.Sog sog on sog.Id = dig.SogId and sog.DeactivatedDateTime is null
    left join InputAtoms.Sfab sfab on sfab.Id = dig.SogId and sfab.DeactivatedDateTime is null
    left join InputAtoms.Pla pla on pla.Id = sfab.PlaId 
    left join InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId

    where dig.DeactivatedDateTime is null

    return;

end
go

