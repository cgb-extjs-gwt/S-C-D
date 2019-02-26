ALTER DATABASE SCD_2 SET RECOVERY SIMPLE
GO 

IF OBJECT_ID('tempdb..#Temp_SLA') IS NOT NULL DROP TABLE #Temp_SLA
IF OBJECT_ID('tempdb..#Temp_Cnt') IS NOT NULL DROP TABLE #Temp_Cnt
GO

ALTER INDEX IX_LocalPortfolio_Country_Wg        ON Portfolio.LocalPortfolio DISABLE;  
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

select p.WgId
     , p.AvailabilityId
     , p.DurationId
     , p.ReactionTypeId
     , p.ReactionTimeId
     , p.ServiceLocationId
     , p.ProActiveSlaId
     , rta.Id as ReactionTime_Avalability
     , rtt.Id as ReactionTime_ReactionType
     , rtta.Id as ReactionTime_ReactionType_Avalability
INTO #Temp_SLA
from Portfolio.PrincipalPortfolio p
join Dependencies.ReactionTime_Avalability rta on rta.AvailabilityId = p.AvailabilityId and rta.ReactionTimeId = p.ReactionTimeId
join Dependencies.ReactionTime_ReactionType rtt on rtt.ReactionTimeId = p.ReactionTimeId and rtt.ReactionTypeId = p.ReactionTypeId
join Dependencies.ReactionTime_ReactionType_Avalability rtta on rtta.AvailabilityId = p.AvailabilityId and rtta.ReactionTimeId = p.ReactionTimeId and rtta.ReactionTypeId = p.ReactionTypeId

where IsGlobalPortfolio = 1

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

    INSERT INTO Portfolio.LocalPortfolio (
            CountryId
            , WgId
            , AvailabilityId
            , DurationId
            , ReactionTypeId
            , ReactionTimeId
            , ServiceLocationId
            , ProActiveSlaId
            , ReactionTime_Avalability
            , ReactionTime_ReactionType
            , ReactionTime_ReactionType_Avalability) (

        SELECT @cnt, sla.WgId, sla.AvailabilityId, sla.DurationId, sla.ReactionTypeId, sla.ReactionTimeId, sla.ServiceLocationId, ProActiveSlaId, ReactionTime_Avalability, ReactionTime_ReactionType, ReactionTime_ReactionType_Avalability
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

ALTER INDEX IX_LocalPortfolio_Country_Wg ON Portfolio.LocalPortfolio REBUILD;  
GO

-- Enable all table constraints
ALTER TABLE Portfolio.LocalPortfolio CHECK CONSTRAINT ALL
GO

IF OBJECT_ID('tempdb..#Temp_SLA') IS NOT NULL DROP TABLE #Temp_SLA
IF OBJECT_ID('tempdb..#Temp_Cnt') IS NOT NULL DROP TABLE #Temp_Cnt

ALTER DATABASE SCD_2 SET RECOVERY FULL
GO 
