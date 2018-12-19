ALTER DATABASE SCD_2 SET RECOVERY SIMPLE
GO 

IF OBJECT_ID('tempdb..#Temp_SLA') IS NOT NULL DROP TABLE #Temp_SLA
IF OBJECT_ID('tempdb..#Temp_Cnt') IS NOT NULL DROP TABLE #Temp_Cnt
GO

ALTER INDEX IX_LocalPortfolio_AvailabilityId    ON Portfolio.LocalPortfolio DISABLE;  
ALTER INDEX IX_LocalPortfolio_DurationId        ON Portfolio.LocalPortfolio DISABLE;  
ALTER INDEX IX_LocalPortfolio_ReactionTimeId    ON Portfolio.LocalPortfolio DISABLE;  
ALTER INDEX IX_LocalPortfolio_ReactionTypeId    ON Portfolio.LocalPortfolio DISABLE;  
ALTER INDEX IX_LocalPortfolio_ServiceLocationId ON Portfolio.LocalPortfolio DISABLE;  
ALTER INDEX IX_LocalPortfolio_CountryId         ON Portfolio.LocalPortfolio DISABLE;  
ALTER INDEX IX_LocalPortfolio_WgId              ON Portfolio.LocalPortfolio DISABLE;  
ALTER INDEX IX_LocalPortfolio_ProActiveSlaId    ON Portfolio.LocalPortfolio DISABLE;  

-- Disable all table constraints
ALTER TABLE Portfolio.LocalPortfolio NOCHECK CONSTRAINT ALL;
GO

truncate table Portfolio.LocalPortfolio;
GO

select WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId
INTO #Temp_SLA
from Portfolio.PrincipalPortfolio
where PortfolioType = 2

declare @rownum int = 1;
declare @cnt bigint;
declare @flag bit = 1;

SELECT ROW_NUMBER() over(order by cnt.Id) as rownum, cnt.Id
INTO #Temp_Cnt
FROM InputAtoms.Country cnt
where cnt.IsMaster = 1

while @flag = 1
begin
    set @flag = 0;
    select @flag = 1, @cnt = Id from #Temp_Cnt where rownum = @rownum;
    set @rownum = @rownum + 1;

    if @flag = 0 break;

    INSERT INTO Portfolio.LocalPortfolio (CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, ProActiveSlaId) (

	    SELECT @cnt, sla.WgId, sla.AvailabilityId, sla.DurationId, sla.ReactionTypeId, sla.ReactionTimeId, sla.ServiceLocationId, ProActiveSlaId
	    FROM #Temp_Sla sla
    );

end;

ALTER INDEX IX_LocalPortfolio_AvailabilityId ON Portfolio.LocalPortfolio REBUILD;  
GO  

ALTER INDEX IX_LocalPortfolio_DurationId ON Portfolio.LocalPortfolio REBUILD;  
GO  

ALTER INDEX IX_LocalPortfolio_ReactionTimeId ON Portfolio.LocalPortfolio REBUILD;  
GO  

ALTER INDEX IX_LocalPortfolio_ReactionTypeId ON Portfolio.LocalPortfolio REBUILD;  
GO  

ALTER INDEX IX_LocalPortfolio_ServiceLocationId ON Portfolio.LocalPortfolio REBUILD;  
GO  

ALTER INDEX IX_LocalPortfolio_CountryId ON Portfolio.LocalPortfolio REBUILD;  
GO  

ALTER INDEX IX_LocalPortfolio_WgId ON Portfolio.LocalPortfolio REBUILD;  
GO  

ALTER INDEX IX_LocalPortfolio_ProActiveSlaId ON Portfolio.LocalPortfolio REBUILD;  
GO

-- Enable all table constraints
ALTER TABLE Portfolio.LocalPortfolio CHECK CONSTRAINT ALL
GO

IF OBJECT_ID('tempdb..#Temp_SLA') IS NOT NULL DROP TABLE #Temp_SLA
IF OBJECT_ID('tempdb..#Temp_Cnt') IS NOT NULL DROP TABLE #Temp_Cnt

ALTER DATABASE SCD_2 SET RECOVERY FULL
GO 
