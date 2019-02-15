USE [Scd_2]
GO

CREATE TYPE [dbo].[ListName] AS TABLE(
	[Name] nvarchar(max) NULL
)
GO

ALTER FUNCTION [Report].[GetServiceCostsBySla]
(
    @cnt nvarchar(max),
    @loc dbo.ListName readonly,
    @av dbo.ListName readonly,
    @reactiontime dbo.ListName readonly,
    @reactiontype dbo.ListName readonly,
    @wg dbo.ListName readonly,
    @dur dbo.ListName readonly
)
RETURNS @tbl TABLE (
    Country nvarchar(200),
    ServiceTC float, 
    ServiceTP float, 
    ServiceTP1 float,
    ServiceTP2 float,
    ServiceTP3 float,
    ServiceTP4 float,
    ServiceTP5 float
)
AS
BEGIN

    declare @cntId dbo.ListId;
    declare @locId dbo.ListId;
    declare @avId dbo.ListId;
    declare @reactiontimeId dbo.ListId;
    declare @reactiontypeId dbo.ListId;
    declare @wgId dbo.ListId;
    declare @durId dbo.ListId;
	declare @proId dbo.ListId;

    insert into @cntId select id from InputAtoms.Country where UPPER(Name)= UPPER(@cnt);
    insert into @locId select  id from Dependencies.ServiceLocation where UPPER(Name) in (select UPPER(Name) from @loc)

    insert into @avId 
	select id 
	from Dependencies.Availability avDb
	join @av avInput on avDb.ExternalName like '%' + avInput.Name + '%';

    insert into @reactiontimeId select   id from Dependencies.ReactionTime where UPPER(Name) in (select UPPER(Name) from @reactiontime)
    insert into @reactiontypeId select   id from Dependencies.ReactionType where UPPER(Name) in (select UPPER(Name) from @reactiontype)
    insert into @wgId select id from InputAtoms.Wg where UPPER(Name) in (select UPPER(Name) from @wg)
    insert into @durId select id from Dependencies.Duration where UPPER(Name) in (select UPPER(Name) from @dur)

    INSERT @tbl
    select costs.Country,
           coalesce(costs.ServiceTCManual, costs.ServiceTC) as ServiceTC, 
		   costs.ServiceTP_Released as ServiceTP, 
           costs.ServiceTP1,
           costs.ServiceTP2,
           costs.ServiceTP3,
           costs.ServiceTP4,
           costs.ServiceTP5
     from Hardware.GetCostsFull(0, @cntId, @wgId, @avId, @durId, @reactiontimeId, @reactiontypeId, @locId, @proId, 0, -1) costs

    return;
END
GO
