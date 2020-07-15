if OBJECT_ID('Hardware.SpGetCostsByID') is not null
    drop procedure [Hardware].SpGetCostsByID;
go

create procedure [Hardware].SpGetCostsByID(
    @approved       bit , 
    @id             bigint
)
as
begin


    --=== sla ==========================================================
    declare @cntID bigint;
    declare @wgID  bigint;
    declare @avID  bigint;
    declare @durID bigint;
    declare @rtimeID bigint;
    declare @rtypeID bigint;
    declare @rttID bigint;
    declare @rtaID bigint;
    declare @locID bigint;
    declare @proID   bigint;

    select  @cntID = CountryId
          , @wgID = WgId
          , @durID = DurationId
          , @avID = AvailabilityId
          , @rtimeID = ReactionTimeId
          , @rtypeID = ReactionTypeId
          , @rttID = ReactionTime_ReactionType
          , @rtaID = ReactionTime_Avalability
          , @locID = ServiceLocationId
          , @proID = ProActiveSlaId
    from Portfolio.LocalPortfolio m
    where m.id = @id;

    declare @cntlist dbo.ListID; insert into @cntlist(id) values(@cntID);
    declare @wglist dbo.ListID; insert into @wglist(id) values(@wgID);
    declare @avlist dbo.ListID; insert into @avlist(id) values(@avID);
    declare @durlist dbo.ListID; insert into @durlist(id) values(@durID);
    declare @rtimelist dbo.ListID; insert into @rtimelist(id) values(@rtimeID);
    declare @rtypelist dbo.ListID; insert into @rtypelist(id) values(@rtypeID);
    declare @loclist dbo.ListID; insert into @loclist(id) values (@locID);
    declare @prolist dbo.ListID; insert into @prolist(id) values(@proID);

    select top(1) * 
    from Hardware.GetCosts(@approved, @cntlist, @wglist, @avlist, @durlist, @rtimelist, @rtypelist, @loclist, @prolist, null, null)
    where id = @id;

end
go

exec Hardware.SpGetCostsByID 0, 3598858;