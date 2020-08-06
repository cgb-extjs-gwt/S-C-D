if OBJECT_ID('[Report].[spProActive_Full_KPG]') is not null
    drop procedure [Report].[spProActive_Full_KPG];
go

create procedure [Report].[spProActive_Full_KPG]
as
BEGIN

    declare @tbl table (
          rownum             int  
        , Id                 bigint
        , Country            nvarchar(255)
        , CountryGroup       nvarchar(255)
        , Fsp                nvarchar(255)
        , Wg                 nvarchar(255)
        , Pla                nvarchar(255)
        , ServiceLocation    nvarchar(255)
        , ReactionTime       nvarchar(255)
        , ReactionType       nvarchar(255)
        , Availability       nvarchar(255)
        , ProActiveSla       nvarchar(255)
        , Duration           nvarchar(255)
        , ReActive           float
        , ProActive          float
        , ServiceTP          float
        , Currency           nvarchar(255)
        , Sog                nvarchar(255)
        , SogDescription     nvarchar(255)
        , FspDescription     nvarchar(255)
    );

    declare @wg dbo.ListID ;
    declare @cnt bigint;
    declare @rownum int = 1;
    declare @countries table (
          rownum int
        , Id     bigint
    );

    insert into @countries
    SELECT ROW_NUMBER() over(order by cnt.Id) as rownum, cnt.Id
    FROM InputAtoms.Country cnt
    where cnt.IsMaster = 1

    while 1 = 1
    begin

        set @cnt = (select Id from @countries where rownum = @rownum);
        set @rownum = @rownum + 1;

        if @cnt is null break;

        insert into @tbl exec Report.spProActive @cnt, null, null, null, null, null, null, null, null, null;

    end;

    select 
            Country         
          , CountryGroup    
          , Fsp             
          , Wg              
          , Pla
          , ServiceLocation 
          , ReactionTime    
          , ReactionType    
          , Availability    
          , ProActiveSla    
          , Duration        
          , ReActive        
          , ProActive       
          , ServiceTP       
          , Currency        
          , Sog             
          , SogDescription  
          , FspDescription  
    
    from  @tbl;

end
go