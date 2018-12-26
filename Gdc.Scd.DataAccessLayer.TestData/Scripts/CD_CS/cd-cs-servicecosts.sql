/****** Object:  UserDefinedFunction [Report].[GetServiceCostsBySla]    Script Date: 26.10.2018 15:29:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [Report].[GetServiceCostsBySla]
(
    @cnt nvarchar(200),
    @loc nvarchar(200),
    @av nvarchar(200),
    @reactiontime nvarchar(200),
    @reactiontype nvarchar(200),
    @wg nvarchar(200),   
    @dur nvarchar(200)
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

    declare @cntId bigint;
    declare @locId bigint;
    declare @avId bigint;
    declare @reactiontimeId bigint;
    declare @reactiontypeId bigint;
    declare @wgId bigint;
    declare @durId bigint;

    select @cntId = id from InputAtoms.Country where Name= @cnt;
    select @locId = id from Dependencies.ServiceLocation where Name = @loc;
    select @avId = id from Dependencies.Availability where ExternalName like '%' + @av + '%';
    select @reactiontimeId = id from Dependencies.ReactionTime where Name=@reactiontime;
    select @reactiontypeId = id from Dependencies.ReactionType where Name=@reactiontype;
    select @wgId = id from InputAtoms.Wg where Name=@wg;
    select @durId = id from Dependencies.Duration where Name=@dur;

    INSERT @tbl
    select costs.Country,
           coalesce(costs.ServiceTCManual, costs.ServiceTC) as ServiceTC, 
           coalesce(costs.ServiceTPManual, costs.ServiceTP) as ServiceTP, 
           costs.ServiceTP1,
           costs.ServiceTP2,
           costs.ServiceTP3,
           costs.ServiceTP4,
           costs.ServiceTP5
     from Hardware.GetCostsFull(0, @cntId, @wgId, @avId, @durId, @reactiontimeId, @reactiontypeId, @locId, null, 0, -1) costs

     return;

END

