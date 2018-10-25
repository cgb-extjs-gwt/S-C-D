ALTER DATABASE SCD_2 SET RECOVERY SIMPLE
GO 

-- Disable all table constraints
ALTER TABLE Hardware.ServiceCostCalculation NOCHECK CONSTRAINT ALL

DELETE FROM Hardware.ServiceCostCalculation;

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

    INSERT INTO Hardware.ServiceCostCalculation (MatrixId) (
	    SELECT m.Id
	    FROM Matrix.Matrix m
        where m.CountryId = @cnt
    );

end;

----------------------------------------------------------------------------------

-- Enable all table constraints
ALTER TABLE Hardware.ServiceCostCalculation CHECK CONSTRAINT ALL
GO

IF OBJECT_ID('tempdb..#ShrinkLog') IS NOT NULL DROP PROC #ShrinkLog

ALTER DATABASE SCD_2 SET RECOVERY FULL
GO 


