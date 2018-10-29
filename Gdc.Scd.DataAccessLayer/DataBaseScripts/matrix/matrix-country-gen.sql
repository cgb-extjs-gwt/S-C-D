ALTER DATABASE SCD_2 SET RECOVERY SIMPLE
GO 

IF OBJECT_ID('tempdb..#Temp_SLA') IS NOT NULL DROP TABLE #Temp_SLA
IF OBJECT_ID('tempdb..#Temp_Cnt') IS NOT NULL DROP TABLE #Temp_Cnt
GO

ALTER INDEX IX_Matrix_AvailabilityId ON Matrix.Matrix DISABLE;  
ALTER INDEX IX_Matrix_DurationId ON Matrix.Matrix DISABLE;  
ALTER INDEX IX_Matrix_ReactionTimeId ON Matrix.Matrix DISABLE;  
ALTER INDEX IX_Matrix_ReactionTypeId ON Matrix.Matrix DISABLE;  
ALTER INDEX IX_Matrix_ServiceLocationId ON Matrix.Matrix DISABLE;  
ALTER INDEX IX_Matrix_CountryId ON Matrix.Matrix DISABLE;  
ALTER INDEX IX_Matrix_WgId ON Matrix.Matrix DISABLE;  

-- Disable all table constraints
ALTER TABLE Matrix.Matrix NOCHECK CONSTRAINT ALL

DELETE FROM Matrix.Matrix;

SELECT wg.Id AS wg, 
		av.Id AS av, 
		dur.Id AS dur, 
		rtype.Id AS reacttype, 
		rtime.Id AS reacttime,
		sv.Id AS srvloc
INTO #Temp_Sla
FROM InputAtoms.Wg AS wg
CROSS JOIN Dependencies.Availability AS av
CROSS JOIN Dependencies.Duration AS dur
CROSS JOIN Dependencies.ReactionType AS rtype
CROSS JOIN Dependencies.ReactionTime AS rtime
CROSS JOIN Dependencies.ServiceLocation AS sv

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

    INSERT INTO Matrix.Matrix (CountryId, WgId, AvailabilityId, DurationId, ReactionTypeId, ReactionTimeId, ServiceLocationId, Denied) (

	    SELECT @cnt, sla.wg, sla.av, sla.dur, sla.reacttype, sla.reacttime, sla.srvloc, 0
	    FROM #Temp_Sla sla
    );

end;

ALTER INDEX IX_Matrix_AvailabilityId ON Matrix.Matrix REBUILD;  
GO  

ALTER INDEX IX_Matrix_DurationId ON Matrix.Matrix REBUILD;  
GO  

ALTER INDEX IX_Matrix_ReactionTimeId ON Matrix.Matrix REBUILD;  
GO  

ALTER INDEX IX_Matrix_ReactionTypeId ON Matrix.Matrix REBUILD;  
GO  

ALTER INDEX IX_Matrix_ServiceLocationId ON Matrix.Matrix REBUILD;  
GO  

ALTER INDEX IX_Matrix_CountryId ON Matrix.Matrix REBUILD;  
GO  

ALTER INDEX IX_Matrix_WgId ON Matrix.Matrix REBUILD;  
GO  

-- Enable all table constraints
ALTER TABLE Matrix.Matrix CHECK CONSTRAINT ALL
GO

IF OBJECT_ID('tempdb..#Temp_SLA') IS NOT NULL DROP TABLE #Temp_SLA
IF OBJECT_ID('tempdb..#Temp_Cnt') IS NOT NULL DROP TABLE #Temp_Cnt

ALTER DATABASE SCD_2 SET RECOVERY FULL
GO 
