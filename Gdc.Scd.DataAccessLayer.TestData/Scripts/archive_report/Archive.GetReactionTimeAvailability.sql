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