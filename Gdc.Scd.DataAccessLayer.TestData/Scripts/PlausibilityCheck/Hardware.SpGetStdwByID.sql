if OBJECT_ID('Hardware.SpGetStdwByID') is not null
    drop procedure [Hardware].SpGetStdwByID;
go

create procedure [Hardware].SpGetStdwByID(
    @approved       bit, 
    @cntID          bigint,
    @wgID           bigint
)
as
begin
    declare @cntlist dbo.ListID; insert into @cntlist(id) values(@cntID);
    declare @wglist dbo.ListID; insert into @wglist(id) values(@wgID);

    select 
              av.Name as Availability
            , dur.Name as Duration
            , rtime.Name as ReactionTime
            , rtype.Name as ReactionType
            , loc.Name as ServiceLocation
            , pro.ExternalName as ProActiveSla
            , std.*
    from Hardware.CalcStdw(@approved, @cntlist, @wglist) std
    join Fsp.HwFspCodeTranslation fsp on fsp.Id = std.StdFspId

    join Dependencies.Availability av on av.id = fsp.AvailabilityId
    join Dependencies.Duration dur on dur.id = fsp.DurationId
    join Dependencies.ReactionTime rtime on rtime.Id = fsp.ReactionTimeId
    join Dependencies.ReactionType rtype on rtype.Id = fsp.ReactionTypeId
    join Dependencies.ServiceLocation loc on loc.Id = fsp.ServiceLocationId
    join Dependencies.ProActiveSla pro on pro.Id = fsp.ProactiveSlaId

end
go

exec Hardware.SpGetStdwByID 0, 113, 1;