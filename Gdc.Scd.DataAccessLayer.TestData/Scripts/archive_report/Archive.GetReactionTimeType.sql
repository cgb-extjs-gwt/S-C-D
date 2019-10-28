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