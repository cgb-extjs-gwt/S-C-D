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
