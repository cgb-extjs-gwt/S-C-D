IF OBJECT_ID('Report.GetProActiveByCountryAndWg') IS NOT NULL
  DROP FUNCTION Report.GetProActiveByCountryAndWg;
go 

CREATE FUNCTION [Report].[GetProActiveByCountryAndWg]
(
    @cnt nvarchar(max),
    @wgList nvarchar(max)   
)
RETURNS TABLE 
AS
RETURN (
    select wg.[Name] as Wg,	  
		pro.[ProActiveSla] as ProActiveModel, 
		coalesce(pro.Service_Approved , 0) as Cost,
		coalesce(pro.Setup_Approved, 0) as OneTimeTasks	
		from [Hardware].[ProActiveView] pro
		left join InputAtoms.Wg wg on wg.[Name] in (select Name from Report.SplitString(@wgList))
		left join InputAtoms.Country cnt on cnt.Name = @cnt
		where Wg=wg.Id and Country=cnt.Id and pro.[ProActiveSla] in (6,7,3,4))

GO

IF OBJECT_ID('Report.GetServiceCostsBySla') IS NOT NULL
  DROP FUNCTION Report.GetServiceCostsBySla;
go 

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

    declare @cntId dbo.ListId;
    declare @locId dbo.ListId;
    declare @avId dbo.ListId;
    declare @reactiontimeId dbo.ListId;
    declare @reactiontypeId dbo.ListId;
    declare @wgId dbo.ListId;
    declare @durId dbo.ListId;
	declare @proId dbo.ListId;

    insert into @cntId select id from InputAtoms.Country where UPPER(Name)= UPPER(@cnt);
    insert into @locId select  id from Dependencies.ServiceLocation where UPPER(Name) = UPPER(@loc);
    insert into @avId select   id from Dependencies.Availability where ExternalName like '%' + @av + '%';
    insert into @reactiontimeId select   id from Dependencies.ReactionTime where UPPER(Name)=UPPER(@reactiontime);
    insert into @reactiontypeId select   id from Dependencies.ReactionType where UPPER(Name)=UPPER(@reactiontype);
    insert into @wgId select id from InputAtoms.Wg where UPPER(Name)=UPPER(@wg);
    insert into @durId select id from Dependencies.Duration where UPPER(Name)=UPPER(@dur);

    INSERT @tbl
    select costs.Country,
           coalesce(costs.ServiceTCManual, costs.ServiceTC) as ServiceTC, 
		   costs.ServiceTP_Released as ServiceTP, 
           costs.ServiceTP1,
           costs.ServiceTP2,
           costs.ServiceTP3,
           costs.ServiceTP4,
           costs.ServiceTP5
     from Hardware.GetCosts(0, @cntId, @wgId, @avId, @durId, @reactiontimeId, @reactiontypeId, @locId, @proId, 0, -1) costs

    return;
END