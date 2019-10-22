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
